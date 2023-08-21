using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hardware;

public class Ppu
{
    public IBus Bus { get; }
    public IPixelBuffer Buffer { get; }
    private PpuCtrl control;
    private byte mask;
    private PpuStatus status;
    private byte oamAddr;
    private byte oamData;
    private byte scroll;
    private byte addr;
    private byte data;

    private bool addressLatch;


    // Control

    // Mask
    private bool ShowBackground => (mask & 0x8) > 0;
    private bool ShowBackgroundLeft => (mask & 0x2) > 0;
    private bool ShowSprite => (mask & 0x10) > 0;
    private bool ShowSpriteLeft => (mask & 0x4) > 0;

    private VRamAddress vram;
    private VRamAddress tram;
    private byte lastWrittenValue;

    private byte FineX;

    private ushort BackgroundBaseAddress => (ushort) ((control & 0x10) > 0 ? 0x1000 : 0x0000);
    public bool NonMaskableInterrupt { get; private set; }
    public bool FrameComplete { get; set; }

    public Memory<ObjectAttributeEntry> OAM = new(new ObjectAttributeEntry[64]);

    public Ppu(IBus bus, IPixelBuffer buffer)
    {
        Bus = bus;
        Buffer = buffer;
    }

    public void Reset()
    {
        FineX = 0;
        addressLatch = false;
        scanline = 0;
        cycle = 0;
        backgroundNextTileId = 0;
        nextAttribute = 0;
        nextPatternLow = 0;
        nextPatternHigh = 0;
        nextAttribute = 0;
        status = 0;
        control = 0;
        mask = 0;
        vram = 0;
        tram = 0;
        backgroundPixelLow = 0;
        backgroundPixelHigh = 0;
        backgroundAttributeLow = 0;
        backgroundAttributeHigh = 0;
        lastWrittenValue = 0;
        NonMaskableInterrupt = false;
    }

    public byte CpuRead(ushort address)
    {
        address &= 0x0007;
        byte value = 0;
        switch (address)
        {
            case 2:
                value = (byte) ((status & 0b11100000) | (lastWrittenValue & 0x1F));
                addressLatch = false;
                status.VerticalBlank = false;
                lastWrittenValue = value;
                return value;
            case 4:
                return GetOamByte();
            case 7:
                value = lastWrittenValue;
                lastWrittenValue = Read(vram);

                if (vram.Raw >= 0x3F00)
                    value = lastWrittenValue;

                vram += control.VramIncrement;
                return value;
            default:
                return 0;
        }
    }

    public void CpuWrite(ushort address, byte value)
    {
        address &= 0x0007;

        lastWrittenValue = value;

        switch (address)
        {
            case 0:
                control = value;
                break;
            case 1:
                mask = value;
                break;
            case 3:
                oamAddr = value;
                break;
            case 4:
                SetOamByte(oamAddr, value);
                break;
            case 5:
                if (!addressLatch)
                {
                    tram = (ushort) ((tram & 0xFFE0) | (value >> 3));
                    FineX = (byte) (value & 0x7);
                    addressLatch = true;
                }
                else
                {
                    tram = (ushort) (tram & 0x8FFF | ((value & 0x7) << 12));
                    tram = (ushort) (tram & 0xFC1F | (value << 2));
                    vram = tram;
                    addressLatch = false;
                }

                break;
            case 6:
                if (!addressLatch)
                {
                    // Clear high bits and set high byte
                    tram = (ushort) ((tram & 0x00FF) | (value & 0x3F) << 8);
                    addressLatch = true;
                }
                else
                {
                    tram = (ushort) ((tram & 0xFF00) | value);
                    vram = tram;
                    addressLatch = false;
                }

                break;
            case 7:
                Write(vram, value);
                vram += control.VramIncrement;
                break;
        }
    }

    public void SetOamByte(byte address, byte value)
    {
        var byteMemory = MemoryMarshal.Cast<ObjectAttributeEntry, byte>(OAM.Span);
        byteMemory[address] = value;
    }

    private byte GetOamByte()
    {
        var byteMemory = MemoryMarshal.Cast<ObjectAttributeEntry, byte>(OAM.Span);
        return byteMemory[oamAddr];
    }

    private short scanline = -1;
    private ushort cycle = 0;

    private byte spriteCount = 0;
    private ObjectAttributeEntry[] sprites = new ObjectAttributeEntry[8];
    private byte[] spritePixelLow = new byte[8];
    private byte[] spritePixelHigh = new byte[8];

    public void Cycle()
    {
        switch (scanline)
        {
            case -1 or 261: // Pre-Render
            case >= 0 and < 240: // Render
                ProcessPixels();
                SpriteEvaluation();
                SpriteFetching();
                break;
            case 240:
                // Post-Render
                break;
            case >= 240 and < 261:
                FinishFrame();
                break;
        }

        var (bgPixel, bgPalette) = RenderBackground();
        var (fgPixel, fgPalette, fgPriority, spriteZero) = RenderForeground();

        var (pixel, palette) = ChoosePixel(bgPixel, fgPixel, fgPalette, bgPalette, fgPriority, spriteZero);

        if (cycle is > 0 and < 256
            && scanline is >= 0 and < 240)
        {
            var colour = GetColourFromPalette(palette, pixel);
            Buffer.SetPixel((ushort) (cycle - 1), (ushort) scanline, colour);
        }

        cycle++;
        if (cycle < 341) return;
        cycle = 0;
        scanline++;
        if (scanline < 261) return;
        scanline = -1;
        FrameComplete = true;
    }

    private (byte pixel, byte palette) ChoosePixel(byte bgPixel, byte fgPixel, byte fgPalette, byte bgPalette,
        bool fgPriority, bool spriteZero)
    {
        if (bgPixel == 0 && fgPixel == 0)
            return (0, 0);

        if (bgPixel == 0 && fgPixel > 0)
            return (fgPixel, fgPalette);

        if (bgPixel > 0 && fgPixel == 0)
            return (bgPixel, bgPalette);

        if (bgPixel <= 0 || fgPixel <= 0) 
            return (0, 0);
        
        if (!zeroSpriteHitPossible 
            || !spriteZero
            || !ShowBackground
            || !ShowSprite)
            return fgPriority
                ? (fgPixel, fgPalette)
                : (bgPixel, bgPalette);
            
        if (!(ShowBackgroundLeft || ShowSpriteLeft))
        {
            if (cycle is >= 9 and < 258)
                status.SpriteZeroHit = true;
        }
        else
        {
            if (cycle is >= 1 and < 258)
                status.SpriteZeroHit = true;
        }

        return fgPriority 
            ? (fgPixel, fgPalette) 
            : (bgPixel, bgPalette);
    }

    private void SpriteFetching()
    {
        if (cycle is not 340)
            return;

        for (int i = 0; i < spriteCount % 8; i++)
        {
            var sprite = sprites[i];

            ushort addressLow = !control.SpriteSize
                ? GetSpriteAddress8x8(sprite)
                : GetSpriteAddress8x16(sprite);
            ushort addressHigh = (ushort) (addressLow + 8);

            byte spriteLow = Read(addressLow);
            byte spriteHigh = Read(addressHigh);

            if (sprite.FlipHorizontal)
            {
                spriteLow = spriteLow.Reverse();
                spriteHigh = spriteHigh.Reverse();
            }

            spritePixelLow[i] = spriteLow;
            spritePixelHigh[i] = spriteHigh;
        }
    }

    private ushort GetSpriteAddress8x16(ObjectAttributeEntry sprite)
    {
        // 8x16
        if (!sprite.FlipVertical)
        {
            // Normal Orientation
            // Upper part
            if (scanline - sprite.Y < 8)
            {
                return (ushort) (sprite.Bank8x16
                                 | (sprite.TileId8x16 << 4)
                                 | (byte) ((scanline - sprite.Y) & 0x7));
            }

            // Lower part
            return (ushort) (sprite.Bank8x16
                             | ((sprite.TileId8x16 + 1) << 4)
                             | (byte) ((scanline - sprite.Y) & 0x7));
        }

        // Normal Orientation
        // Upper part
        if (scanline - sprite.Y < 8)
        {
            return (ushort) (sprite.Bank8x16
                             | ((sprite.TileId8x16 + 1) << 4)
                             | (byte) (7 - (scanline - sprite.Y) & 0x7));
        }

        // Lower part
        return (ushort) (sprite.Bank8x16
                         | ((sprite.TileId8x16 + 1) << 4)
                         | (sprite.TileId8x16 << 4)
                         | (byte) (7 - (scanline - sprite.Y) & 0x7));
    }

    private ushort GetSpriteAddress8x8(ObjectAttributeEntry sprite)
    {
        if (!sprite.FlipVertical)
        {
            // Normal Orientation
            return (ushort) ((control.SpriteTableBase << 12)
                             | (sprite.Id << 4)
                             | (scanline - sprite.Y));
        }

        // Flipped
        return (ushort) ((control.SpriteTableBase << 12)
                         | (sprite.Id << 4)
                         | (7 - (scanline - sprite.Y)));
    }

    private void SpriteEvaluation()
    {
        if (cycle != 257
            || scanline < 0)
            return;
        
        sprites = new ObjectAttributeEntry[8];
        spriteCount = 0;

        for (int i = 0; i < 64; i++)
        {
            var sprite = OAM.Span[i];
            short diff = (short) (scanline - sprite.Y);

            if (diff < 0
                || diff >= (control.SpriteSize ? 16 : 8))
                continue;

            if (spriteCount >= 8)
            {
                // increment for potential sprite overflow
                spriteCount++;
                break;
            }

            if (i == 0)
            {
                // zero sprite hit?
                zeroSpriteHitPossible = true;
            }

            sprites[spriteCount] = OAM.Span[i];
            spriteCount++;
        }

        spriteOverflow = spriteCount >= 8;
    }

    private readonly uint[] nesColorPalette =
    {
        0x7C7C7CFF, 0x0000FCFF, 0x0000BCFF, 0x4428BCFF, 0x940084FF, 0xA80020FF, 0xA81000FF, 0x881400FF,
        0x503000FF, 0x007800FF, 0x006800FF, 0x005800FF, 0x004058FF, 0x000000FF, 0x000000FF, 0x000000FF,
        0xBCBCBCFF, 0x0078F8FF, 0x0058F8FF, 0x6844FCFF, 0xD800CCFF, 0xE40058FF, 0xF83800FF, 0xE45C10FF,
        0xAC7C00FF, 0x00B800FF, 0x00A800FF, 0x00A844FF, 0x008888FF, 0x000000FF, 0x000000FF, 0x000000FF,
        0xF8F8F8FF, 0x3CBCFCFF, 0x6888FCFF, 0x9878F8FF, 0xF878F8FF, 0xF85898FF, 0xF87858FF, 0xFCA044FF,
        0xF8B800FF, 0xB8F818FF, 0x58D854FF, 0x58F898FF, 0x00E8D8FF, 0x787878FF, 0x000000FF, 0x000000FF,
        0xFCFCFCFF, 0xA4E4FCFF, 0xB8B8F8FF, 0xD8B8F8FF, 0xF8B8F8FF, 0xF8A4C0FF, 0xF0D0B0FF, 0xFCE0A8FF,
        0xF8D878FF, 0xD8F878FF, 0xB8F8B8FF, 0xB8F8D8FF, 0x00FCFCFF, 0xF8D8F8FF, 0x000000FF, 0x000000FF
    };

    public uint[] GetPalette(byte i)
    {
        uint[] colours = new uint[4];

        for (byte pixel = 0; pixel < 4; pixel++)
        {
            colours[pixel] = GetColourFromPalette(i, pixel);
        }

        return colours;
    }

    public uint[] GetPatternTable(byte i, byte palette = 0)
    {
        uint[] pixels = new uint[256 * 256];

        for (ushort tileY = 0; tileY < 16; tileY++)
        {
            for (ushort tileX = 0; tileX < 16; tileX++)
            {
                ushort offset = (ushort) (tileY * 256 + tileX * 16);

                for (ushort row = 0; row < 8; row++)
                {
                    byte lsb = Read((ushort) (i * 0x1000 + offset + row));
                    byte msb = Read((ushort) (i * 0x1000 + offset + row + 8));

                    for (ushort col = 0; col < 8; col++)
                    {
                        byte pixel = (byte) ((lsb & 0x1) + (msb & 0x1));

                        lsb >>= 1;
                        msb >>= 1;

                        uint colour = GetColourFromPalette(palette, pixel);

                        ushort x = (ushort) (tileX * 8 + (7 - col));
                        ushort y = (ushort) (tileY * 8 + row);

                        pixels[y * 256 + x] = colour;
                    }
                }
            }
        }

        return pixels;
    }

    public uint[] GetOamTable(byte patternTable)
    {
        uint[] pixels = new uint[64 * 64];
        
        for (int i = 0; i < 64; i++)
        {
            var sprite = OAM.Span[i];
            var palette = (byte)((sprite.Attribute & 0x3) + 0x4);
            ushort offset = (ushort) (sprite.Id << 4);

            for (ushort row = 0; row < 8; row++)
            {
                byte lsb = Read((ushort) (patternTable * 0x1000 + offset + row));
                byte msb = Read((ushort) (patternTable * 0x1000 + offset + row + 8));

                for (ushort col = 0; col < 8; col++)
                {
                    byte pixel = (byte) ((lsb & 0x1) + (msb & 0x1));

                    lsb >>= 1;
                    msb >>= 1;
                    
                    uint colour = GetColourFromPalette(palette, pixel);

                    byte tileY = (byte) (i / 8);
                    byte tileX = (byte) (i % 8);
                    
                    ushort x = (ushort) (tileX * 8 + (7 - col));
                    ushort y = (ushort) (tileY * 8 + row);

                    pixels[y * 64 + x] = colour;
                }
            }
        }

        return pixels;
    }

    public uint GetColourFromPalette(byte palette, byte pixel)
    {
        ushort address = (ushort) (0x3F00 + (palette << 2) + pixel);
        byte index = Read(address);
        return nesColorPalette[index & 0x3f].ToPackedColor();
    }

    private (byte pixel, byte palette) RenderBackground()
    {
        if (!ShowBackground)
            return (0, 0);

        // Composite current pixel

        ushort bitMux = (ushort) (0x8000 >> FineX);

        byte pixelPlane1 = (byte) ((backgroundPixelLow & bitMux) > 0 ? 1 : 0);
        byte pixelPlane2 = (byte) ((backgroundPixelHigh & bitMux) > 0 ? 1 : 0);

        byte pixel = (byte) (pixelPlane2 << 1 | pixelPlane1);

        byte palettePlane1 = (byte) ((backgroundAttributeLow & bitMux) > 0 ? 1 : 0);
        byte palettePlane2 = (byte) ((backgroundAttributeHigh & bitMux) > 0 ? 1 : 0);

        byte palette = (byte) (palettePlane2 << 1 | palettePlane1);

        return (pixel, palette);
    }

    private (byte pixel, byte palette, bool priority, bool spriteZero) RenderForeground()
    {
        if (!ShowSprite)
            return (0, 0, false, false);

        // Composite current pixel
        for (int i = 0; i < (spriteCount % 9) ; i++)
        {
            var sprite = sprites[i];
            if (sprite.X != 0)
                continue;

            byte pixelPlane1 = (byte) ((spritePixelLow[i] & 0x80) > 0 ? 1 : 0);
            byte pixelPlane2 = (byte) ((spritePixelHigh[i] & 0x80) > 0 ? 1 : 0);

            byte pixel = (byte) (pixelPlane2 << 1 | pixelPlane1);
            byte palette = (byte) ((byte) (sprite.Attribute & 0x3) + 0x4);
            bool priority = (sprite.Attribute & 0x20) == 0;

            if (pixel != 0)
                return (pixel, palette, priority, i==0);
        }


        return (0, 0, false, false);
    }

    private ushort backgroundPixelLow;
    private ushort backgroundPixelHigh;
    private ushort backgroundAttributeLow;
    private ushort backgroundAttributeHigh;

    private byte backgroundNextTileId;
    private byte nextAttribute;
    private byte nextPatternLow;
    private byte nextPatternHigh;

    private bool spriteOverflow;
    private bool zeroSpriteHitPossible;

    private void ProcessPixels()
    {
        if (scanline is 0
            && cycle is 0)
            cycle = 1;

        if (scanline is -1
            && cycle is 1)
        {
            status.VerticalBlank = false;
            status.SpriteOverflow = spriteOverflow;
            spritePixelLow = new byte[8];
            spritePixelHigh = new byte[8];
        }

        if (cycle is >= 2 and < 258 or >= 321 and < 338)
        {
            // Shift pixel register
            ShiftBackgroundRegisters();
            ShiftSpriteRegisters();

            switch ((cycle - 1) % 8)
            {
                case 0:
                    UpdateBackgroundRegisters();

                    // Load next tile id
                    LoadNextTile();
                    break;
                case 2:
                    LoadNextAttribute();
                    break;
                case 4:
                    SetNextPatternLow();
                    break;
                case 6:
                    SetNextPatternHigh();
                    break;
                case 7:
                    ScrollX();
                    break;
            }
        }

        // End of visible scanline
        if (cycle is 256)
            ScrollY();

        // reset to beginning of next scanline
        if (cycle is 257)
        {
            UpdateBackgroundRegisters();
            TransferTempX();
        }

        if (cycle is 338 or 340)
            LoadNextTile();

        if (scanline is -1 &&
            cycle is >= 280 and < 305)
            TransferTempY();
    }

    private void SetNextPatternHigh()
    {
        // load next pattern high bits
        // BackgroundTableBase is either 0 or 1. if 1 << 13 it would be $1000
        ushort tileAddressHigh = (ushort) ((control.BackgroundTableBase << 12)
                                           + (backgroundNextTileId << 4)
                                           + vram.FineY
                                           + 8);

        nextPatternHigh = Read(tileAddressHigh);
    }

    private void SetNextPatternLow()
    {
        // load next pattern low bits 
        // BackgroundTableBase is either 0 or 1. if 1 << 13 it would be $1000
        ushort tileAddressLow = (ushort) ((control.BackgroundTableBase << 12)
                                          + (backgroundNextTileId << 4)
                                          + vram.FineY);

        nextPatternLow = Read(tileAddressLow);
    }

    private void LoadNextAttribute()
    {
        // Use CoarseX and Y and Nametable to get to the attribute address
        // Address = 1yx1111YYYXXX -- where x and y are nametables and X and Y the offset
        ushort attributeAddress = (ushort) (0x2000 // Nametable base address
                                            | 0b001111000000 // Offset into Attribute Space
                                            | vram.NametableY << 11
                                            | vram.NametableX << 10
                                            | (vram.CoarseY >> 2) << 3
                                            | vram.CoarseX >> 2);
        // load next attribute 2x2
        nextAttribute = Read(attributeAddress);

        // reduce to only the 2 bits of the exact tile
        // First determine which quadrant
        // +---+---+
        // | 0 | 1 |
        // +---+---+
        // | 2 | 3 |
        // +---+---+
        if ((vram.CoarseY & 0x02) > 0) nextAttribute >>= 4;
        if ((vram.CoarseX & 0x02) > 0) nextAttribute >>= 2;
        nextAttribute &= 0x3;
    }

    private void LoadNextTile()
    {
        ushort address = (ushort) (0x2000 | vram.Raw & 0xFFF);
        backgroundNextTileId = Read(address);
    }

    private void ShiftBackgroundRegisters()
    {
        if (!ShowBackground)
            return;

        backgroundPixelLow <<= 1;
        backgroundPixelHigh <<= 1;
        backgroundAttributeLow <<= 1;
        backgroundAttributeHigh <<= 1;
    }


    private void ShiftSpriteRegisters()
    {
        if (!ShowSprite)
            return;

        if (cycle is < 1 or >= 258)
            return;

        for (int i = 0; i < spriteCount; i++)
        {
            ref var sprite = ref sprites[i];

            if (sprite.X > 0)
                sprite.X--;
            else
            {
                spritePixelLow[i] <<= 1;
                spritePixelHigh[i] <<= 1;
            }
        }
    }

    private void UpdateBackgroundRegisters()
    {
        backgroundPixelLow = (ushort) ((backgroundPixelLow & 0xFF00) | nextPatternLow);
        backgroundPixelHigh = (ushort) ((backgroundPixelHigh & 0xFF00) | nextPatternHigh);
        backgroundAttributeLow =
            (ushort) ((backgroundAttributeLow & 0xFF00) | ((nextAttribute & 0b01) == 0 ? 0x00 : 0xFF));
        backgroundAttributeHigh =
            (ushort) ((backgroundAttributeHigh & 0xFF00) | ((nextAttribute & 0b10) == 0 ? 0x00 : 0xFF));
    }

    private void FinishFrame()
    {
        if (scanline is not 241 || cycle is not 1) return;

        // End of Frame
        status.VerticalBlank = true;

        // Trigger NMI
        if (control.EnableNmi)
            NonMaskableInterrupt = true;
    }

    private void ScrollX()
    {
        if (!ShowBackground && !ShowSprite)
            return;

        if (vram.CoarseX != 0x1F)
        {
            vram.CoarseX++;
            return;
        }

        vram.CoarseX = 0;
        vram.NametableX ^= 1;
    }

    private void ScrollY()
    {
        if (!ShowBackground && !ShowSprite)
            return;

        if (vram.FineY < 7)
        {
            vram.FineY++;
            return;
        }

        vram.FineY = 0;

        switch (vram.CoarseY)
        {
            case 29:
                vram.CoarseY = 0;
                vram.NametableY ^= 1;
                return;
            case 31:
                vram.CoarseY = 0;
                return;
            default:
                vram.CoarseY++;
                break;
        }
    }

    private void TransferTempX()
    {
        if (!ShowBackground && !ShowSprite)
            return;

        vram.CoarseX = tram.CoarseX;
        vram.NametableX = tram.NametableX;
    }

    private void TransferTempY()
    {
        if (!ShowBackground && !ShowSprite)
            return;

        vram.CoarseY = tram.CoarseY;
        vram.FineY = tram.FineY;
        vram.NametableY = tram.NametableY;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Read(ushort address) => Bus.Read(address);
    private void Write(ushort address, byte data) => Bus.Write(address, data);

    public void ResetNonMaskableInterrupt()
    {
        NonMaskableInterrupt = false;
    }
}