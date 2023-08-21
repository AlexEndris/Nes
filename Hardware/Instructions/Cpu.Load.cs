// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetLoadFlag(byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
    }

    private byte LDA(Func<ushort> fetch, ushort _)
    {
        A = (byte) fetch();
        SetLoadFlag(A);
        return 1;
    }

    private byte LDX(Func<ushort> fetch, ushort _)
    {
        X = (byte) fetch();
        SetLoadFlag(X);
        return 1;
    }

    private byte LDY(Func<ushort> fetch, ushort _)
    {
        Y = (byte) fetch();
        SetLoadFlag(Y);
        return 1;
    }
}