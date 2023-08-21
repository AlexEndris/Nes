// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private void SetStackFlags(byte result)
    {
        Zero = result == 0;
        Negative = (result & 0x80) > 0;
    }

    private byte PHA(ushort _, ushort __)
    {
        PushToStack(A);
        return 0;
    }

    private byte PHP(ushort _, ushort __)
    {
        PushToStack((byte)(Status | CpuFlags.BreakCommand | CpuFlags.Unused));
        return 0;
    }

    private byte PLA(ushort _, ushort __)
    {
        A = PopFromStack();
        SetStackFlags(A);
        return 0;
    }

    private byte PLP(ushort _, ushort __)
    {
        Status = (CpuFlags)(PopFromStack() & 0b11001111);
        return 0;
    }
}