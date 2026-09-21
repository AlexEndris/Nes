using System;

namespace Hardware;

public interface IMapper
{
    public ushort PrgBanks { get; }
    public ushort ChrBanks { get; }
    public bool IsCpuRead(ushort address);
    public bool IsCpuWrite(ushort address);
    public ushort? CpuRead(ushort address, ref byte data);
    public ushort? CpuWrite(ushort address, byte data);
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