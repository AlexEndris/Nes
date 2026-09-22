namespace Hardware.Headers;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct RomMsb
{
    private byte raw;
    
    public byte PrgRomSizeMsb => (byte) (raw & 0xF);
    public byte ChrRomSizeMsb => (byte) (raw >> 4 & 0xF);
}