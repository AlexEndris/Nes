// ReSharper disable once CheckNamespace
namespace Hardware;

public partial class Cpu
{
    private void SetTransferFlag(byte result)
    {
        Zero = result == 0;
        Negative = (result & 0x80) > 0;
    }
    
    private byte TAX()
    {
        X = A;
        SetTransferFlag(X);
        return 2;
    }

    private byte TAY()
    {
        Y = A;
        SetTransferFlag(Y);
        return 2;
    }

    private byte TSX()
    {
        X = SP;
        SetTransferFlag(X);
        return 2;
    }

    private byte TXA()
    {
        A = X;
        SetTransferFlag(A);
        return 2;
    }

    private byte TXS()
    {
         SP = X;
         return 2;
    }

    private byte TYA()
    {
        A = Y;
        SetTransferFlag(A);
        return 2;
    }
}