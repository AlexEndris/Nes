// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetStackFlags(byte result)
    {
        Zero = result == 0;
        Negative = (result & 0x80) > 0;
    }

    private byte PHA(Func<ushort> _, ushort __)
    {
        PushToStack(A);
        return 0;
    }

    private byte PHP(Func<ushort> _, ushort __)
    {
        PushToStack((byte)(Status | CpuFlags.BreakCommand | CpuFlags.Unused));
        return 0;
    }

    private byte PLA(Func<ushort> _, ushort __)
    {
        A = PopFromStack();
        SetStackFlags(A);
        return 0;
    }

    private byte PLP(Func<ushort> _, ushort __)
    {
        Status = (CpuFlags)(PopFromStack() & 0b11001111);
        return 0;
    }
}