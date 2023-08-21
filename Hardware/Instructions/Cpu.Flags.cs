// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private byte CLC()
    {
        Carry = false;
        return 2;
    }

    private byte CLD()
    {
        DecimalMode = false;
        return 2;
    }

    private byte CLI()
    {
        InterruptDisable = false;
        return 2;
    }

    private byte CLV()
    {
        Overflow = false;
        return 2;
    }

    private byte SEC()
    {
        Carry = true;
        return 2;
    }

    private byte SED()
    {
        DecimalMode = true;
        return 2;
    }

    private byte SEI()
    {
        InterruptDisable = true;
        return 2;
    }
}