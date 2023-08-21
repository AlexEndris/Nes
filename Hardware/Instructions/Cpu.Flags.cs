// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private byte CLC(Func<ushort> _, ushort __)
    {
        Carry = false;
        return 0;
    }

    private byte CLD(Func<ushort> _, ushort __)
    {
        DecimalMode = false;
        return 0;
    }

    private byte CLI(Func<ushort> _, ushort __)
    {
        InterruptDisable = false;
        return 0;
    }

    private byte CLV(Func<ushort> _, ushort __)
    {
        Overflow = false;
        return 0;
    }

    private byte SEC(Func<ushort> _, ushort __)
    {
        Carry = true;
        return 0;
    }

    private byte SED(Func<ushort> _, ushort __)
    {
        DecimalMode = true;
        return 0;
    }

    private byte SEI(Func<ushort> _, ushort __)
    {
        InterruptDisable = true;
        return 0;
    }
}