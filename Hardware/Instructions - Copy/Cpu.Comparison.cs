// ReSharper disable once CheckNamespace

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

    private byte CMP(ushort data, ushort _)
    {
        byte value = (byte) data;
        SetFlagsForCmp(A, value);
        return 0;
    }

    private byte CPX(ushort data, ushort _)
    {
        byte value = (byte) data;
        SetFlagsForCmp(X, value);
        return 0;
    }

    private byte CPY(ushort data, ushort _)
    {
        byte value = (byte) data;
        SetFlagsForCmp(Y, value);
        return 0;
    }
}