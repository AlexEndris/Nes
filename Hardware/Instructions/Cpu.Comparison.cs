// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetFlagsForCmp(byte original, byte value)
    {
        int result = original - value;

        Zero = result == 0;
        Carry = original >= value;
        Negative = (result & 0x80) > 0;
    }

    private byte CMP(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        SetFlagsForCmp(A, value);
        return 0;
    }

    private byte CPX(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        SetFlagsForCmp(X, value);
        return 0;
    }

    private byte CPY(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        SetFlagsForCmp(Y, value);
        return 0;
    }
}