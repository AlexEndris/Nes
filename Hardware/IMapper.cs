using System;

namespace Hardware;

using Headers;

public interface IMapper
{
    public ushort PrgBanks { get; }
    public ushort ChrBanks { get; }
    public ushort PrgRamBanks { get; }
    public ushort ChrRamBanks { get; }
    public Mirroring Mirroring { get; }
    public bool PrgRamEnabled { get; }
    public bool PrgRamWriteAllowed { get; }
    public bool IsCpuRead(ushort address);
    public bool IsCpuWrite(ushort address);
    public int? CpuRead(ushort address);
    public int? CpuWrite(ushort address, byte data);
    public bool PpuRead(ushort address, out int mappedAddress);
    public bool PpuWrite(ushort address, out ushort mappedAddress);
}

public abstract class AbstractMapper : IMapper
{
    public AbstractMapper(Mirroring? mirroring, ushort prgBanks, ushort chrBanks, ushort prgRamBanks, ushort chrRamBanks)
    {
        PrgBanks = prgBanks;
        ChrBanks = chrBanks;
        PrgRamBanks = prgRamBanks;
        ChrRamBanks = chrRamBanks;
        Mirroring = mirroring!.Value;
    }
    
    public ushort PrgBanks { get; }

    public ushort ChrBanks { get; }

    public ushort PrgRamBanks { get; }

    public ushort ChrRamBanks { get; }

    public virtual Mirroring Mirroring { get; }

    public virtual bool PrgRamEnabled { get; } = false;

    public virtual bool PrgRamWriteAllowed { get; } = true;

    public abstract bool IsCpuRead(ushort address);

    public abstract bool IsCpuWrite(ushort address);

    public abstract int? CpuRead(ushort address);

    public abstract int? CpuWrite(ushort address, byte data);

    public abstract bool PpuRead(ushort address, out int mappedAddress);

    public abstract bool PpuWrite(ushort address, out ushort mappedAddress);
}

public class MapperIdAttribute : Attribute
{
    public int MapperId { get; }

    public MapperIdAttribute(int mapperId)
    {
        MapperId = mapperId;
    }
}