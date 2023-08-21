using System;

namespace Hardware;

public class Memory
{
    private Memory<byte> ram = new byte[65536].AsMemory();

    public byte Read(ushort address)
    {
        return ram.Span[address];
    }

    public ushort Read16Bit(ushort address)
    {
        byte lowByte = ram.Span[address];
        byte highByte = ram.Span[address+1];

        return (ushort) (highByte << 8 | lowByte);
    }
    
    public void Write(ushort address, byte value)
    {
        ram.Span[address] = value;
    }    
    
    public void Write(ushort address, ushort value)
    {
        byte lowByte = (byte) (value & 0x00FF);
        byte highByte = (byte) (value >> 8);

        ram.Span[address] = lowByte;
        ram.Span[address + 1] = highByte;
    }

    public static bool CrossesPageBoundary(ushort baseAddress, ushort actualAddress)
    {
        return (baseAddress & 0xFF00) != (actualAddress & 0xFF00);
    }
}