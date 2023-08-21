// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private void SetBitwiseFlags(byte value)
    {
        Zero = value == 0;
        Negative = (value & 0x80) > 0;
    }

    private void SetBITFlags(byte value)
    {
        byte result = (byte) (A & value);

        Zero = result == 0;
        Negative = (value & 0x80) > 0;
        Overflow = (value & 0x40) > 0;
    }

    private byte AND(ushort data, ushort _)
    {
        byte value = (byte) data;
        A &= value;
        SetBitwiseFlags(A);
        return 0;
    }

    private byte ORA(ushort data, ushort _)
    {
        byte value = (byte) data;
        A |= value;
        SetBitwiseFlags(A);
        return 0;
    }

    private byte EOR(ushort data, ushort _)
    {
        byte value = (byte) data;
        A ^= value;
        SetBitwiseFlags(A);
        return 0;
    }

    private byte BIT(ushort data, ushort _)
    {
        byte value = (byte) data;
        SetBITFlags(value);
        return 0;
    }
}