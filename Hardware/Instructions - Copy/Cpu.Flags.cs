// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private byte CLC(ushort _, ushort __)
    {
        Carry = false;
        return 0;
    }

    private byte CLD(ushort _, ushort __)
    {
        DecimalMode = false;
        return 0;
    }

    private byte CLI(ushort _, ushort __)
    {
        InterruptDisable = false;
        return 0;
    }

    private byte CLV(ushort _, ushort __)
    {
        Overflow = false;
        return 0;
    }

    private byte SEC(ushort _, ushort __)
    {
        Carry = true;
        return 0;
    }

    private byte SED(ushort _, ushort __)
    {
        DecimalMode = true;
        return 0;
    }

    private byte SEI(ushort _, ushort __)
    {
        InterruptDisable = true;
        return 0;
    }
}