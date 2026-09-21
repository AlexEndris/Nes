using System;

namespace Hardware;

public interface IMapper
{
    public ushort PrgBanks { get; }
    public ushort ChrBanks { get; }
    public bool IsCpuRead(ushort address);
    public bool IsCpuWrite(ushort address);
    public int? CpuRead(ushort address);
    public int? CpuWrite(ushort address, byte data);
    public bool PpuRead(ushort address, out ushort mappedAddress);
    public bool PpuWrite(ushort address, out ushort mappedAddress);
}

public class MapperIdAttribute : Attribute
{
    public int MapperId { get; }

    public MapperIdAttribute(int mapperId)
    {
        MapperId = mapperId;
    }
}