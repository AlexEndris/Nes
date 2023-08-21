// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetTransferFlag(byte result)
    {
        Zero = result == 0;
        Negative = (result & 0x80) > 0;
    }
    
    private byte TAX(Func<ushort> _, ushort __)
    {
        X = A;
        SetTransferFlag(X);
        return 0;
    }

    private byte TAY(Func<ushort> _, ushort __)
    {
        Y = A;
        SetTransferFlag(Y);
        return 0;
    }

    private byte TSX(Func<ushort> _, ushort __)
    {
        X = SP;
        SetTransferFlag(X);
        return 0;
    }

    private byte TXA(Func<ushort> _, ushort __)
    {
        A = X;
        SetTransferFlag(A);
        return 0;
    }

    private byte TXS(Func<ushort> _, ushort __)
    {
         SP = X;
         return 0;
    }

    private byte TYA(Func<ushort> _, ushort __)
    {
        A = Y;
        SetTransferFlag(A);
        return 0;
    }
}