using System;

namespace Hardware;

public interface IMapper
{
    public ushort PrgBanks { get; }
    public ushort ChrBanks { get; }
    public bool CpuRead(ushort address, out ushort mappedAddress);
    public bool CpuWrite(ushort address, out ushort mappedAddress);
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