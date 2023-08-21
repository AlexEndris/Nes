// ReSharper disable once CheckNamespace
namespace Hardware;

public partial class Cpu
{
    private void SetTransferFlag(byte result)
    {
        Zero = result == 0;
        Negative = (result & 0x80) > 0;
    }
    
    private byte TAX(ushort _, ushort __)
    {
        X = A;
        SetTransferFlag(X);
        return 0;
    }

    private byte TAY(ushort _, ushort __)
    {
        Y = A;
        SetTransferFlag(Y);
        return 0;
    }

    private byte TSX(ushort _, ushort __)
    {
        X = SP;
        SetTransferFlag(X);
        return 0;
    }

    private byte TXA(ushort _, ushort __)
    {
        A = X;
        SetTransferFlag(A);
        return 0;
    }

    private byte TXS(ushort _, ushort __)
    {
         SP = X;
         return 0;
    }

    private byte TYA(ushort _, ushort __)
    {
        A = Y;
        SetTransferFlag(A);
        return 0;
    }
}