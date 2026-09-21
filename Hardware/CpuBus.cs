using System;

namespace Hardware;

using System.Linq;

public class CpuBus : IBus
{
    public Cartridge Cartridge { get; private set; }
    public Ppu Ppu { get; }
    public Apu Apu { get; }

    private Memory<byte> ram = new(new byte[0x800]);

    public Memory<byte> Temp = new(new byte[0x2000]);
    
    public Memory<byte> Ram => ram;
    
    public byte DmaData;
    public byte DmaPage;
    public byte DmaAddress;
    public bool DmaTransfer;
    public bool DmaDummy;
    public bool DmaHalt;
    
    // Set from outside
    public byte[] controllers = new byte[2];

    private byte[] controllerState = new byte[2];

    private byte openBus;
    private bool strobe;

    public CpuBus(Ppu ppu, Apu apu)
    {
        Ppu = ppu;
        Apu = apu;
    }

    public void Insert(Cartridge cartridge)
    {
        Cartridge = cartridge;
    }
    
    public byte Read(ushort address)
    {
        if (Cartridge.CpuRead(address, out var value))
            return openBus = value;

        byte readValue = address switch
        {
            <= 0x1FFF => ram.Span[address & 0x07FF],
            <= 0x3FFF => Ppu.CpuRead((ushort) (address & 0x0007)),
            <= 0x4015 => Apu.CpuRead(address),
            0x4016 or 0x4017 => GetControllerState(address),
            >= 0x6000 => Temp.Span[address & 0x1FFF],
            _ => openBus
        };

        return openBus = readValue;
    }

    private byte GetControllerState(ushort address)
    {
        int i = address & 0x1;
        if (strobe)
        {
            controllerState[i] = controllers[i]; // continuously reloaded
            return (byte)((controllerState[i] & 0x80) > 0 ? 1 : 0);
        }
        
        byte data = (byte) ((controllerState[i] & 0x80) > 0 ? 1 : 0);
        
        controllerState[i] <<= 1;
        controllerState[i] |= 1; // Fill with ones after the last read
        return data;
    }

    public ushort Read16Bit(ushort address)
    {
        byte low = Read(address);
        byte high = Read((ushort) (address+1));

        return (ushort) (high << 8 | low);
    }

    public void Write(ushort address, byte value)
    {
        openBus = value;

        if (Cartridge.CpuWrite(address, value))
            return;
        
        switch (address)
        {
            case <= 0x1FFF:
                ram.Span[address & 0x07FF] = value;
                break;
            case <= 0x3FFF:
                Ppu.CpuWrite((ushort) (address & 0x0007), value);
                break;
            case <= 0x4013 or 0x4015 or 0x4017:
                Apu.CpuWrite(address, value);
                break;
            case 0x4014:
                DmaPage = value;
                DmaAddress = 0;
                DmaTransfer = true;
                DmaHalt = true;
                break;
            case 0x4016:
                bool newStrobe = (value & 0x01) != 0;
                if (newStrobe)
                {
                    controllerState[0] = controllers[0];
                    controllerState[1] = controllers[1];
                }
                strobe = newStrobe;
                break;
            case >= 0x6000:
                Temp.Span[address & 0x1FFF] = value;
                break;
        }
    }
}