namespace Hardware.Headers;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct ChrRamNvRam
{
    private byte raw;
    
    public byte ChrRamShift => (byte) (raw & 0xF);
    public byte ChrNvRamShift => (byte) (raw >> 4 & 0xF);
}