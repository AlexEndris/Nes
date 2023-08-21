// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private void SetStackFlags(byte result)
    {
        Zero = result == 0;
        Negative = (result & 0x80) > 0;
    }

    private byte PHA()
    {
        PushToStack(A);
        return 3;
    }

    private byte PHP()
    {
        PushToStack((byte)(Status | CpuFlags.BreakCommand | CpuFlags.Unused));
        return 3;
    }

    private byte PLA()
    {
        A = PopFromStack();
        SetStackFlags(A);
        return 4;
    }

    private byte PLP()
    {
        Status = (CpuFlags)(PopFromStack() & 0b11001111);
        return 4;
    }
}