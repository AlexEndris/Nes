using System;

namespace Hardware;

public class CpuBus : IBus
{
    public Cartridge Cartridge { get; private set; }
    public Ppu Ppu { get; }

    private Memory<byte> ram = new(new byte[0x800]);

    public Memory<byte> Temp = new(new byte[0x2000]);
    
    public Memory<byte> Ram => ram;
    
    public byte DmaData;
    public byte DmaPage;
    public byte DmaAddress;
    public bool DmaTransfer;
    public bool DmaDummy;
    
    // Set from outside
    public byte[] controllers = new byte[2];

    private byte[] controllerState = new byte[2];
    public CpuBus(Ppu ppu)
    {
        Ppu = ppu;
    }

    public void Insert(Cartridge cartridge)
    {
        Cartridge = cartridge;
    }
    
    public byte Read(ushort address)
    {
        if (Cartridge.CpuRead(address, out var value))
            return value;

        return address switch
        {
            <= 0x1FFF => ram.Span[address & 0x07FF],
            <= 0x3FFF => Ppu.CpuRead((ushort) (address & 0x0007)),
            0x4016 or 0x4017 => GetControllerState(address),
            >= 0x6000 => Temp.Span[address & 0x1FFF],
            _ => 0
        };
    }

    private byte GetControllerState(ushort address)
    {
        byte data = (byte) ((controllerState[address & 0x1] & 0x80) > 0 ? 1 : 0);
        controllerState[address & 0x1] <<= 1;
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
        if (Cartridge.CpuWrite(address, value))
            return;

        switch (address)
        {
            case <= 0x1FFF:
                if (address == 0x02D1)
                    value = value;
                ram.Span[address & 0x07FF] = value;
                break;
            case <= 0x3FFF:
                Ppu.CpuWrite((ushort) (address & 0x0007), value);
                break;
            case 0x4014:
                DmaPage = value;
                DmaAddress = 0;
                DmaTransfer = true;
                break;
            case 0x4016 or 0x4017:
                controllerState[address & 0x1] = controllers[address & 0x1];
                break;
            case >= 0x6000:
                Temp.Span[address & 0x1FFF] = value;
                break;
        }
    }
}