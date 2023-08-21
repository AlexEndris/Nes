// ReSharper disable once CheckNamespace

using System;

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

    private byte AND(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        A &= value;
        SetBitwiseFlags(A);
        return 0;
    }

    private byte ORA(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        A |= value;
        SetBitwiseFlags(A);
        return 0;
    }

    private byte EOR(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        A ^= value;
        SetBitwiseFlags(A);
        return 0;
    }

    private byte BIT(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        SetBITFlags(value);
        return 0;
    }
}