// ReSharper disable once CheckNamespace
namespace Hardware;

public partial class Cpu
{
    private byte BRK()
    {
        PC++;
        
        Unused = true;
        BreakCommand = true;
        PushToStack(PC);
        byte status = (byte)((byte)Status);
        PushToStack(status);
        
        PC = Read16Bit(0xFFFE);
        
        return 7;
    }

    private byte RTI()
    {
        Status = (CpuFlags) PopFromStack();
        PC = PopFromStack16Bit();
        
        return 6;
    }
}