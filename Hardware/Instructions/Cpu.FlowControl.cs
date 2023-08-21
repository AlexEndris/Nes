// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private byte JMPAbs(ushort _, ushort address)
    {
        PC = address;
        return 0; 
    }

    private byte JMPInd(ushort data, ushort _)
    {
        PC = data;
        return 0;
    }

    private byte JSR(ushort _, ushort address)
    {
        ushort returnAddress = (ushort) (PC - 1);
        PushToStack(returnAddress);
        PC = address;
        return 0;
    }

    private byte RTS(ushort _, ushort __)
    {
        ushort returnAddress = PopFromStack16Bit();

        PC = (ushort) (returnAddress + 1);
        return 6; 
    }

    private byte BCC(ushort data, ushort _)
    {
        sbyte offset = (sbyte) data;
        
        if (Carry)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 2
            : 1);
    }

    private byte BCS(ushort data, ushort _)
    {
        sbyte offset = (sbyte) data;
        
        if (!Carry)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 2
            : 1);
    }

    private byte BEQ(ushort data, ushort _)
    {
        sbyte offset = (sbyte) data;
        
        if (!Zero)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 2
            : 1);
    }

    private byte BNE(ushort data, ushort _)
    {
        sbyte offset = (sbyte) data;
        
        if (Zero)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 2
            : 1);
    }

    private byte BPL(ushort data, ushort _)
    {
        sbyte offset = (sbyte) data;
        
        if (Negative)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 2
            : 1);
    }

    private byte BMI(ushort data, ushort _)
    {
        sbyte offset = (sbyte) data;
        
        if (!Negative)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 2
            : 1);
    }

    private byte BVC(ushort data, ushort _)
    {
        sbyte offset = (sbyte) data;
        
        if (Overflow)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 2
            : 1);
    }

    private byte BVS(ushort data, ushort _)
    {
        sbyte offset = (sbyte) data;
        
        if (!Overflow)
            return 0;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 2
            : 1);
    }
}