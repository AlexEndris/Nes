namespace Hardware.Headers;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PrgRamNvRam
{
    private byte raw;
    
    public byte PrgRamShift => (byte) (raw & 0xF);
    public byte PrgNvRamShift => (byte) (raw >> 4 & 0xF);
}