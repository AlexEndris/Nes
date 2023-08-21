namespace Hardware;

public static class ByteUtil
{
    public static ushort To16Bit(this byte highByte, byte lowByte) => (ushort) (highByte << 8 | lowByte);
    
    public static byte Reverse(this byte b) 
    {
        b = (byte) ((b & 0xF0) >> 4 | (b & 0x0F) << 4);
        b = (byte) ((b & 0xCC) >> 2 | (b & 0x33) << 2);
        b = (byte) ((b & 0xAA) >> 1 | (b & 0x55) << 1);
        return b;
    }
}