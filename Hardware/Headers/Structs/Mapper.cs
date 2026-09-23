namespace Hardware.Headers;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Mapper
{
    private byte raw;

    public byte MapperSecondLowerNibble => (byte) (raw & 0xF);
    public byte Submapper => (byte) (raw >> 4 & 0xF);
}