using System;
using System.Runtime.CompilerServices;
using Hardware.Headers;

namespace Hardware;

public class PpuBus : IBus
{
    public Cartridge Cartridge { get; private set; }
    private Memory<byte> palette = new(new byte[0x20]);
    private Memory<byte> nameTables = new(new byte[0x800]);

    public void Insert(Cartridge cartridge)
    {
        Cartridge = cartridge;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public byte Read(ushort address)
    {
        address &= 0x3FFF;
        
        if (Cartridge.PpuRead(address, out byte value))
            return value;

        if (address is >= 0x2000 and < 0x3EFF)
        {
            address &= 0xFFF;
            switch (Cartridge.Mirroring)
            {
                case Mirroring.Vertical:
                    switch (address)
                    {
                        case <= 0x03FF:
                            return nameTables.Span[address & 0x3FF];
                        case <= 0x07FF:
                            return nameTables.Span[(address & 0x3FF) | 0x400];
                        case <= 0x0BFF:
                            return nameTables.Span[address & 0x3FF];
                        case <= 0x0FFF:
                            return nameTables.Span[(address & 0x3FF) | 0x400];
                    }

                    break;
                case Mirroring.Horizontal:
                    switch (address)
                    {
                        case <= 0x03FF:
                            return nameTables.Span[address & 0x3FF];
                        case <= 0x07FF:
                            return nameTables.Span[(address & 0x3FF)];
                        case <= 0x0BFF:
                            return nameTables.Span[(address & 0x3FF) | 0x400];
                        case <= 0x0FFF:
                            return nameTables.Span[(address & 0x3FF) | 0x400];
                    }

                    break;
            }
        }
        
        if (address is >= 0x3F00 and <= 0x3FFF)
        {
            address &= 0x1F;
            if (address is 0x10 or 0x14 or 0x18 or 0x1C)
                address &= 0xF;
            
            return palette.Span[address];
        }        
        return 0;
    }

    public ushort Read16Bit(ushort address)
    {
        // Not Gonna Happen
        throw new NotImplementedException();
    }

    public void Write(ushort address, byte value)
    {
        address &= 0x3FFF;
        
        if (Cartridge.PpuWrite(address, value))
        {}
        else if (address is >= 0x2000 and < 0x3EFF)
        {
            address &= 0xFFF;
            switch (Cartridge.Mirroring)
            {
                case Mirroring.Vertical:
                    switch (address)
                    {
                        case <= 0x03FF:
                            nameTables.Span[address & 0x3FF] = value;
                            break;
                        case <= 0x07FF:
                            nameTables.Span[(address & 0x3FF) | 0x400] = value;
                            break;
                        case <= 0x0BFF:
                            nameTables.Span[address & 0x3FF] = value;
                            break;
                        case <= 0x0FFF:
                            nameTables.Span[(address & 0x3FF) | 0x400] = value;
                            break;
                    }

                    break;
                case Mirroring.Horizontal:
                    switch (address)
                    {
                        case <= 0x03FF:
                            nameTables.Span[address & 0x3FF] = value;
                            break;
                        case <= 0x07FF:
                            nameTables.Span[(address & 0x3FF) ] = value;
                            break;
                        case <= 0x0BFF:
                            nameTables.Span[(address & 0x3FF) | 0x400] = value;
                            break;
                        case <= 0x0FFF:
                            nameTables.Span[(address & 0x3FF) | 0x400] = value;
                            break;
                    }

                    break;
            }
        }            
        else if (address is >= 0x3F00 and <= 0x3FFF)
        {
            address &= 0x1F;
            if (address is 0x10 or 0x14 or 0x18 or 0x1C)
                address &= 0b01111;
            
            palette.Span[address] = value;
        }
    }
}