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

    public bool IsCpuRead(ushort address)
    {
        return address >= 0x8000;
    }

    public ushort? CpuRead(ushort address, ref byte data)
    {
        if (address < 0x8000)
            return null;
        
        return (ushort) (address & (PrgBanks == 1 ? 0x3FFF : 0x7FFF));
    }

    public bool IsCpuWrite(ushort address)
    {
        return address >= 0x8000;
    }

    public ushort? CpuWrite(ushort address, byte data)
    {
        if (address < 0x8000) 
            return null;
        
        return (ushort) (address & (PrgBanks == 1 ? 0x3FFF : 0x7FFF));
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