namespace Hardware.Mappers;

using Headers;

[MapperId(2)]
public class UxRom : IMapper
{
    public ushort PrgBanks { get; }
    public ushort ChrBanks { get; }
    public ushort PrgRamBanks { get; }
    public ushort ChrRamBanks { get; }
    public Mirroring Mirroring { get; }
    public bool PrgRamEnabled { get; } = false;

    private byte Register { get; set; }

    public UxRom(Mirroring mirroring, ushort prgBanks, ushort chrBanks, ushort prgRamBanks, ushort chrRamBanks)
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
    
    public bool IsCpuWrite(ushort address)
    {
        return address >= 0x8000;
    }
    public int? CpuRead(ushort address)
    {
        if (address < 0x8000)
            return null;

        if (address < 0xC000)
        {
            return (address & 0x3FFF) | (Register << 14);
        }

        return (address & 0x3FFF) | ((PrgBanks-1) << 14);
    }

    public int? CpuWrite(ushort address, byte data)
    {
        if (address < 0x8000)
            return null;

        Register = (byte)(data & 0xF);
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
