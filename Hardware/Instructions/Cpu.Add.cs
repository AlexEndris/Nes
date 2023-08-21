// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetFlagsForAdc(byte originalA, byte addedValue, ushort sum)
    {
        Zero = (byte)sum == 0;
        Carry = sum > 0xff;
        Negative = (sum & 0x80) > 0;
        Overflow = IsOverflow(originalA, addedValue, sum);
    }

    private byte ADC(Func<ushort> fetch, ushort _)
    {
        byte value = (byte)fetch();
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForAdc(A, value, sum);
        A = (byte) sum;
        return 0;
    }
}