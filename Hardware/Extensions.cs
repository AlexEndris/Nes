namespace Hardware;

public static class Extensions
{
    public static CpuFlags With(this CpuFlags flags, CpuFlags flag, bool value)
    {
        if (value)
            flags |= flag;
        else
            flags &= ~flag;

        return flags;
    }

    public static bool IsSet(this CpuFlags flags, CpuFlags flag)
    {
        return (flags & flag) != 0;
    }

    public static byte ToByte(this bool value)
    {
        return (byte) (value ? 1 : 0);
    }

    public static byte RotateLeft(this byte value, bool carry)
    {
        return (byte) ((value << 1) | carry.ToByte());
    }        
    
    public static ushort RotateLeft(this ushort value, bool carry)
    {
        return (ushort) ((value << 1) | carry.ToByte());
    }    
    
    public static byte RotateRight(this byte value, bool carry)
    {
        return (byte) ((value >> 1) | (carry.ToByte() << 7));
    }
}