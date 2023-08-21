// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private byte JMPAbs()
    {
        PC = ReadNext16BitProgram();
        return 3; 
    }

    private byte JMPInd()
    {
        ushort address = ReadNext16BitProgram();
        PC = Read16Bit(address);
        return 5;
    }

    private byte JSR()
    {
        ushort subroutineAddress = ReadNext16BitProgram();
        ushort returnAddress = (ushort) (PC - 1);
        PushToStack(returnAddress);
        PC = subroutineAddress;
        return 6;
    }

    private byte RTS()
    {
        ushort returnAddress = PopFromStack16Bit();

        PC = (ushort) (returnAddress + 1);
        return 6; 
    }

    private byte BCC()
    {
        sbyte offset = (sbyte) ReadNextProgramByte();
        
        if (Carry)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 4
            : 3);
    }

    private byte BCS()
    {
        sbyte offset = (sbyte) ReadNextProgramByte();
        
        if (!Carry)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 4
            : 3);
    }

    private byte BEQ()
    {
        sbyte offset = (sbyte) ReadNextProgramByte();
        
        if (!Zero)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 4
            : 3);
    }

    private byte BNE()
    {
        sbyte offset = (sbyte) ReadNextProgramByte();
        
        if (Zero)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 4
            : 3);
    }

    private byte BPL()
    {
        sbyte offset = (sbyte) ReadNextProgramByte();
        
        if (Negative)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 4
            : 3);
    }

    private byte BMI()
    {
        sbyte offset = (sbyte) ReadNextProgramByte();
        
        if (!Negative)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 4
            : 3);
    }

    private byte BVC()
    {
        sbyte offset = (sbyte) ReadNextProgramByte();
        
        if (Overflow)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 4
            : 3);
    }

    private byte BVS()
    {
        sbyte offset = (sbyte) ReadNextProgramByte();
        
        if (!Overflow)
            return 2;

        ushort previousPC = PC;
        PC = (ushort) (offset + PC);

        return (byte) (Memory.CrossesPageBoundary(previousPC, PC)
            ? 4
            : 3);
    }
}