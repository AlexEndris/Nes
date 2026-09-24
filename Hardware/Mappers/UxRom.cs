namespace Hardware.Mappers;

using Headers;

[MapperId(2)]
public class UxRom : AbstractMapper
{
    private byte Register { get; set; }

    public UxRom(Mirroring? mirroring, ushort prgBanks, ushort chrBanks, ushort prgRamBanks, ushort chrRamBanks)
    : base(mirroring, prgBanks, chrBanks, prgRamBanks, chrRamBanks)
    {

    }
    
    public override bool IsCpuRead(ushort address)
    {
        return address >= 0x8000;
    }
    
    public override bool IsCpuWrite(ushort address)
    {
        return address >= 0x8000;
    }
    public override int? CpuRead(ushort address)
    {
        if (address < 0x8000)
            return null;

        if (address < 0xC000)
        {
            return (address & 0x3FFF) | (Register << 14);
        }

        return (address & 0x3FFF) | ((PrgBanks-1) << 14);
    }

    public override int? CpuWrite(ushort address, byte data)
    {
        if (address < 0x8000)
            return null;

        Register = (byte)(data & 0xF);
        return null;
    }

    public override bool PpuRead(ushort address, out int mappedAddress)
    {
        if (address <= 0x1FFF)
        {
            mappedAddress = address;
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public override bool PpuWrite(ushort address, out ushort mappedAddress)
    {
        if (address <= 0x1FFF && ChrBanks == 0)
        {
            mappedAddress = address;
            return true;
        }

        mappedAddress = 0;
        return false;
    }
}
