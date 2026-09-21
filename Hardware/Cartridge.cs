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
        // Needs to be initialised anyhow
        value = 0;
        if (!Mapper.IsCpuRead(address))
            return false;

        // If the mapped address doesn't get a value, despite the mapper saying
        // it'll handle the mapping, then the mapper already handled the reading as well
        var mappedAddress = Mapper.CpuRead(address, ref value);
        
        if (mappedAddress.HasValue)
            value = PrgMem.Span[mappedAddress.Value];
        
        return true;
    }

    public bool CpuWrite(ushort address, byte value)
    {
        if (!Mapper.IsCpuWrite(address))
            return false;
        
        // If the mapped address doesn't get a value, despite the mapper saying
        // it'll handle the mapping, then the mapper already handled the writing as well
        var mappedAddress = Mapper.CpuWrite(address, value);
        
        if (mappedAddress.HasValue)
            PrgMem.Span[mappedAddress.Value] = value;
        
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