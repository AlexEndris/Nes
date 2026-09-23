namespace Hardware.Headers;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Flags6
{
    private byte raw;
     
    public Mirroring Mirroring => (Mirroring)(raw & 0x1);
    public bool Battery => (raw >> 1 & 0x1) == 1;
    public bool Trainer => (raw >> 2 & 0x1) == 1;
    public bool FourScreen => (raw >> 3 & 0x1) == 1;
    public byte MapperFirstLowerNibble => (byte)(raw >> 4 & 0xF);
}