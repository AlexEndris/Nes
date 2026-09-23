using System;

namespace Hardware;

public class Cartridge
{
    public IMapper Mapper { get; }
    public Memory<byte> PrgRom { get; }
    public Memory<byte> ChrRom { get; }
    public Memory<byte> PrgRam { get; }
    
    public Cartridge(IMapper mapper, byte[] prgMem, byte[] chrMem, byte[] prgRam)
    {
        Mapper = mapper;
        PrgRom = prgMem.AsMemory();
        ChrRom = chrMem.AsMemory();
        PrgRam = prgRam.AsMemory();
    }

    public bool CpuRead(ushort address, out byte value)
    {
        // Needs to be initialised anyhow
        value = 0;
        if (!Mapper.IsCpuRead(address))
            return false;

        if (address is >= 0x6000 and <= 0x7FFF)
        {
            if (! Mapper.PrgRamEnabled)
                return false;
            
            // TODO: Access RAM
            return true;
        }
        
        // If the mapped address doesn't get a value, despite the mapper saying
        // it'll handle the mapping, then the mapper already handled the reading as well
        var mappedAddress = Mapper.CpuRead(address);
        
        if (mappedAddress.HasValue)
            value = PrgRom.Span[mappedAddress.Value];
        
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
            PrgRom.Span[mappedAddress.Value] = value;
        
        return true;
    }

    public bool PpuRead(ushort address, out byte value)
    {
        if (Mapper.PpuRead(address, out var mappedAddress))
        {
            value = ChrRom.Span[mappedAddress];
            return true;
        }

        value = 0;
        return false;
    }

    public bool PpuWrite(ushort address, byte value)
    {
        if (!Mapper.PpuWrite(address, out var mappedAddress))
            return false;

        ChrRom.Span[mappedAddress] = value;
        return true;
    }
}