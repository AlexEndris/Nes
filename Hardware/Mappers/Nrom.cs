namespace Hardware.Mappers;

using Headers;

[MapperId(0)]
public class NRom : AbstractMapper
{
    public NRom(Mirroring? mirroring, ushort prgBanks, ushort chrBanks, ushort prgRamBanks, ushort chrRamBanks)
        : base(mirroring, prgBanks, chrBanks, prgRamBanks, chrRamBanks)
    {
    }

    public override bool IsCpuRead(ushort address)
    {
        return address >= 0x8000;
    }

    public override int? CpuRead(ushort address)
    {
        if (address < 0x8000)
            return null;

        return (ushort)(address & (PrgBanks == 1 ? 0x3FFF : 0x7FFF));
    }

    public override bool IsCpuWrite(ushort address)
    {
        return address >= 0x8000;
    }

    public override int? CpuWrite(ushort address, byte data)
    {
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
