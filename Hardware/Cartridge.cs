using System;
using Hardware.Headers;

namespace Hardware;

public class Cartridge
{
    public IMapper Mapper { get; }
    public ushort PrgBanks { get; }
    public ushort ChrBanks { get; }
    public Memory<byte> PrgMem { get; }
    public Memory<byte> ChrMem { get; }
    public Mirroring Mirroring { get; } 
    
    public Cartridge(Mirroring mirroring, IMapper mapper, ushort prgBanks, byte[] prgMem, ushort chrBanks, byte[] chrMem)
    {
        Mirroring = mirroring;
        Mapper = mapper;
        PrgBanks = prgBanks;
        ChrBanks = chrBanks;
        PrgMem = prgMem.AsMemory();
        ChrMem = chrMem.AsMemory();
    }

    public bool CpuRead(ushort address, out byte value)
    {
        if (Mapper.CpuRead(address, out var mappedAddress))
        {
            value = PrgMem.Span[mappedAddress];
            return true;
        }

        value = 0;
        return false;
    }

    public bool CpuWrite(ushort address, byte value)
    {
        if (!Mapper.CpuWrite(address, out var mappedAddress))
            return false;

        PrgMem.Span[mappedAddress] = value;
        return true;
    }

    public bool PpuRead(ushort address, out byte value)
    {
        if (Mapper.PpuRead(address, out var mappedAddress))
        {
            value = ChrMem.Span[mappedAddress];
            return true;
        }

        value = 0;
        return false;
    }

    public bool PpuWrite(ushort address, byte value)
    {
        if (!Mapper.PpuWrite(address, out var mappedAddress))
            return false;

        ChrMem.Span[mappedAddress] = value;
        return true;
    }
}