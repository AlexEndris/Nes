namespace Hardware.Headers;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Flags7
{
    private byte raw;
     
    public ConsoleType ConsoleType => (ConsoleType)(raw & 0x3);
    public byte Identifier => (byte) (raw >> 2 & 0x3);
    public byte MapperFirstHigherNibble => (byte)(raw >> 4 & 0xF);
}