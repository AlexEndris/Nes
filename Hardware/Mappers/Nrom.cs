namespace Hardware.Mappers;

using Headers;

[MapperId(0)]
public class NRom : IMapper
{
    public ushort PrgBanks { get; }
    public ushort ChrBanks { get; }
    public ushort PrgRamBanks { get; }
    public ushort ChrRamBanks { get; }
    public Mirroring Mirroring { get; }

    public bool PrgRamEnabled { get; } = false;

    public NRom(Mirroring mirroring, ushort prgBanks, ushort chrBanks, ushort prgRamBanks, ushort chrRamBanks)
    {
        PrgBanks = prgBanks;
        ChrBanks = chrBanks;
        PrgRamBanks = prgRamBanks;
        ChrRamBanks = chrRamBanks;
        Mirroring = mirroring;
    }

    public bool IsCpuRead(ushort address)
    {
        return address >= 0x8000;
    }

    public int? CpuRead(ushort address)
    {
        if (address < 0x8000)
            return null;
        
        return (ushort) (address & (PrgBanks == 1 ? 0x3FFF : 0x7FFF));
    }

    public bool IsCpuWrite(ushort address)
    {
        return address >= 0x8000;
    }

    public int? CpuWrite(ushort address, byte data)
    {
        return null;
    }

    public bool PpuRead(ushort address, out ushort mappedAddress)
    {
        if (address <= 0x1FFF)
        {
            mappedAddress = address;
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public bool PpuWrite(ushort address, out ushort mappedAddress)
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