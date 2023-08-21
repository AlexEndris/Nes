// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private byte BRK(Func<ushort> _, ushort __)
    {
        PC++;
        
        PushToStack(PC);
        byte status = (byte)((byte)Status | 0x10 | 0x20);
        PushToStack(status);
        InterruptDisable = true;
        
        PC = Read16Bit(0xFFFE);
        
        return 0;
    }

    private byte RTI(Func<ushort> _, ushort __)
    {
        Status = (CpuFlags) PopFromStack();
        PC = PopFromStack16Bit();
        
        return 0;
    }
}