// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private void SetFlagsForCmp(byte original, byte value)
    {
        int result = original - value;

        Zero = result == 0;
        Carry = original >= value;
        Negative = (result & 0x80) > 0;
    }

    private byte CMPImm()
    {
        byte value = ReadNextProgramByte();
        SetFlagsForCmp(A, value);
        return 2;
    }

    private byte CMPZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        SetFlagsForCmp(A, value);
        return 3;
    }

    private byte CMPZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        byte value = Read(address);
        SetFlagsForCmp(A, value);
        return 4;
    }

    private byte CMPAbs()
    {
        ushort baseAddress = ReadNext16BitProgram();
        byte value = Read(baseAddress);
        SetFlagsForCmp(A, value);
        return 4;
    }

    private byte CMPAbsX()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);
        byte value = Read(actualAddress);
        SetFlagsForCmp(A, value);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    private byte CMPAbsY()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = Read(actualAddress);
        SetFlagsForCmp(A, value);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    private byte CMPIndX()
    {
        byte zeroPageAddress = (byte) (ReadNextProgramByte() + X);
        ushort actualAddress = Read16Bit(zeroPageAddress);
        byte value = Read(actualAddress);
        SetFlagsForCmp(A, value);
        return 6;
    }

    private byte CMPIndY()
    {
        byte zeroPageAddress = ReadNextProgramByte();
        ushort baseAddress = Read16Bit(zeroPageAddress);
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = Read(actualAddress);
        SetFlagsForCmp(A, value);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 6
            : 5);
    }

    private byte CPXImm()
    {
        byte value = ReadNextProgramByte();
        SetFlagsForCmp(X, value);
        return 2;
    }

    private byte CPXZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        SetFlagsForCmp(X, value);
        return 3;
    }

    private byte CPXAbs()
    {
        ushort address = ReadNext16BitProgram();
        byte value = Read(address);
        SetFlagsForCmp(X, value);
        return 4;
    }

    private byte CPYImm()
    {
        byte value = ReadNextProgramByte();
        SetFlagsForCmp(Y, value);
        return 2;
    }

    private byte CPYZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        SetFlagsForCmp(Y, value);
        return 3;
    }

    private byte CPYAbs()
    {
        ushort address = ReadNext16BitProgram();
        byte value = Read(address);
        SetFlagsForCmp(Y, value);
        return 4;
    }
}