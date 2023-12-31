﻿// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetFlagsForSBC(byte originalA, ushort subtractedValue, ushort sum)
    {
        Zero = (sum & 0xFF) == 0;
        Carry = (sum & 0xFF00) > 0;
        Negative = (sum & 0x80) > 0;
        Overflow = IsOverflow(originalA, subtractedValue, sum);
    }

    private byte SBC(Func<ushort> fetch, ushort _)
    {
        ushort value = (ushort) (fetch() ^ 0x00FF) ;
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForSBC(A, value, sum);
        A = (byte)sum;
        return 0;
    }
}