// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private void SetLoadFlag(byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
    }

    private byte LDA(ushort data, ushort _)
    {
        A = (byte) data;
        SetLoadFlag(A);
        return 1;
    }

    private byte LDX(ushort data, ushort _)
    {
        X = (byte) data;
        SetLoadFlag(X);
        return 1;
    }

    private byte LDY(ushort data, ushort _)
    {
        Y = (byte) data;
        SetLoadFlag(Y);
        return 1;
    }
}