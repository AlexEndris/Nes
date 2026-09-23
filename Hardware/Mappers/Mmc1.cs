namespace Hardware.Mappers;

using System;
using System.Diagnostics;
using System.Windows.Forms;

using Headers;

[MapperId(1)]
public class Mmc1 : IMapper
{
    public ushort PrgBanks { get; }
    public ushort ChrBanks { get; }
    public ushort PrgRamBanks { get; }
    public ushort ChrRamBanks { get; }

    public Mirroring Mirroring
    {
        get
        {
            switch (ControlRegister & 0x3)
            {
                case 0:
                    return Mirroring.OneScreenA;
                case 1:
                    return Mirroring.OneScreenB;
                case 2:
                    return Mirroring.Vertical;
                case 3:
                    return Mirroring.Horizontal;
                default:
                    throw new UnreachableException();
            }
        }
    }

    public bool PrgRamEnabled => (PrgBankRegister & 0x10) == 0;   

    private byte ControlRegister { get; set; }
    private byte ChrBank0Register { get; set; }
    private byte ChrBank1Register { get; set; }
    private byte PrgBankRegister { get; set; }
    
    private byte ShiftRegister { get; set; }

    public Mmc1(Mirroring mirroring, ushort prgBanks, ushort chrBanks, ushort prgRamBanks, ushort chrRamBanks)
    {
        PrgBanks = prgBanks;
        ChrBanks = chrBanks;
        PrgRamBanks = prgRamBanks;
        ChrRamBanks = chrRamBanks;
        
        ControlRegister = 0xC;
        ResetShiftRegister();
    }

    public bool IsCpuRead(ushort address)
    {
        return address >= 0x6000;
    }

    public bool IsCpuWrite(ushort address)
    {
        return address >= 0x6000;
    }

    public int? CpuRead(ushort address)
    {
        if (address < 0x8000) 
            return null;

        var bank16 = (PrgBankRegister) & 0xF;
        var bank32 = (PrgBankRegister >> 1) & 0x7;
        
        switch ((ControlRegister >> 2) & 0x3)
        {
            // switch 32 KB at $8000, ignoring low bit of bank number
            case 0:
            case 1:
                return (address & 0x7FFF) | (bank32 << 15);
            // fix first bank at $8000 and switch 16 KB bank at $C000
            case 2:
                if (address < 0xC000)
                    return address & 0x3FFF;
                return (address & 0x3FFF) | (bank16 << 14);
            // fix last bank at $C000 and switch 16 KB bank at $8000
            case 3:
                if (address < 0xC000)
                    return (address & 0x3FFF) | (bank16 << 14);
                return (address & 0x3FFF) | ((PrgBanks - 1) << 14);
            default:
                throw new UnreachableException();
        }
    }

    public int? CpuWrite(ushort address, byte data)
    {
        if (address < 0x8000)
            return null;
        
        if ((data & 0x80) > 0)
        {
            ResetShiftRegister();
            ControlRegister |= 0xC; 
            return null;
        }

        if ((ShiftRegister & 0x1) > 0)
        {
            AddToShiftRegister(data);

            switch (address)
            {
                case >=0xE000:
                    PrgBankRegister = ShiftRegister;
                    break;
                case >=0xC000:
                    ChrBank1Register = ShiftRegister;
                    break;
                case >=0xA000:
                    ChrBank0Register = ShiftRegister;
                    break;
                case >=0x8000:
                    ControlRegister = ShiftRegister;
                    break;
            }
            ResetShiftRegister();
            return null;
        }

        AddToShiftRegister(data);
        
        return null;
    }

    private void AddToShiftRegister(byte data)
    {
        ShiftRegister >>= 1;
        ShiftRegister |= (byte)((data & 0x1) << 4);
    }

    private void ResetShiftRegister()
    {
        ShiftRegister = 0x10;
    }

    public bool PpuRead(ushort address, out ushort mappedAddress)
    {
        mappedAddress = 0;
        if (address >= 0x2000)
            return false;

        if (ChrRamBanks > 0)
        {
            mappedAddress = address;
            return true;
        }

        switch ((ControlRegister >> 4) & 0x1)
        {
            case 0:
                mappedAddress = (ushort)(address | ((ChrBank0Register >> 1) << 13));
                break;
            case 1:
                if (address < 0x1000)
                    mappedAddress = (ushort)(address | (ChrBank0Register << 12));
                else 
                    mappedAddress = (ushort)((address & 0x0FFF) | (ChrBank1Register << 12));
                break;
        }
        
        return true;
    }

    public bool PpuWrite(ushort address, out ushort mappedAddress)
    {
        if (address <= 0x1FFF && ChrRamBanks > 0)
        {
            mappedAddress = address;
            return true;
        }

        mappedAddress = 0;
        return false;
    }
}
