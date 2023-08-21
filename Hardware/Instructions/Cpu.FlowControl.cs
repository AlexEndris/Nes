// ReSharper disable once CheckNamespace

using System;
using System.Windows.Forms;

namespace Hardware;

public partial class Cpu
{
    private byte JMPAbs(Func<ushort> _, ushort address)
    {
        PC = address;
        return 0; 
    }

    private byte JMPInd(Func<ushort> fetch, ushort _)
    {
        PC = fetch();
        return 0;
    }

    private byte JSR(Func<ushort> _, ushort address)
    {
        ushort returnAddress = (ushort) (PC - 1);
        PushToStack(returnAddress);
        PC = address;
        return 0;
    }

    private byte RTS(Func<ushort> _, ushort __)
    {
        ushort returnAddress = PopFromStack16Bit();

        PC = (ushort) (returnAddress + 1);
        return 0; 
    }

    private byte BCC(Func<ushort> fetch, ushort _)
    {
        sbyte offset = (sbyte) fetch();
        
        if (Carry)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        Cycles++;
        if (Memory.CrossesPageBoundary(previousPC, PC))
            Cycles++;
        
        return 0;
    }

    private byte BCS(Func<ushort> fetch, ushort _)
    {
        sbyte offset = (sbyte) fetch();
        
        if (!Carry)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        Cycles++;
        if (Memory.CrossesPageBoundary(previousPC, PC))
            Cycles++;
        
        return 0;
    }

    private byte BEQ(Func<ushort> fetch, ushort _)
    {
        sbyte offset = (sbyte) fetch();
        
        if (!Zero)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        Cycles++;
        if (Memory.CrossesPageBoundary(previousPC, PC))
            Cycles++;
        
        return 0;
    }

    private byte BNE(Func<ushort> fetch, ushort _)
    {
        sbyte offset = (sbyte) fetch();
        
        if (Zero)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        Cycles++;
        if (Memory.CrossesPageBoundary(previousPC, PC))
            Cycles++;
        
        return 0;
    }

    private byte BPL(Func<ushort> fetch, ushort _)
    {
        sbyte offset = (sbyte) fetch();
        
        if (Negative)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        Cycles++;
        if (Memory.CrossesPageBoundary(previousPC, PC))
            Cycles++;
        
        return 0;
    }

    private byte BMI(Func<ushort> fetch, ushort _)
    {
        sbyte offset = (sbyte) fetch();
        
        if (!Negative)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        Cycles++;
        if (Memory.CrossesPageBoundary(previousPC, PC))
            Cycles++;
        
        return 0;
    }

    private byte BVC(Func<ushort> fetch, ushort _)
    {
        sbyte offset = (sbyte) fetch();
        
        if (Overflow)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        Cycles++;
        if (Memory.CrossesPageBoundary(previousPC, PC))
            Cycles++;
        
        return 0;
    }

    private byte BVS(Func<ushort> fetch, ushort _)
    {
        sbyte offset = (sbyte) fetch();
        
        if (!Overflow)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);
        
        Cycles++;
        if (Memory.CrossesPageBoundary(previousPC, PC))
            Cycles++;
        
        return 0;
    }
}