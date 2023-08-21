namespace Hardware.Mappers;

[MapperId(0)]
public class Nrom : IMapper
{
    public ushort PrgBanks { get; }
    public ushort ChrBanks { get; }

    public Nrom(ushort prgBanks, ushort chrBanks)
    {
        PrgBanks = prgBanks;
        ChrBanks = chrBanks;
    }

    public bool CpuRead(ushort address, out ushort mappedAddress)
    {
        if (address >= 0x8000)
        {
            mappedAddress = (ushort) (address & (PrgBanks == 1 ? 0x3FFF : 0x7FFF)); 
            
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public bool CpuWrite(ushort address, out ushort mappedAddress)
    {
        if (address >= 0x8000)
        {
            mappedAddress = (ushort) (address & (PrgBanks == 1 ? 0x3FFF : 0x7FFF));
            return true;
        }

        mappedAddress = 0;
        return false;
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