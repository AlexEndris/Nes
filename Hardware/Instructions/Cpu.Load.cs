// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private void SetLoadFlag(byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
    }
    
    #region Load

    #region LDA

    private byte LDAImm()
    {
        A = ReadNextProgramByte();
        SetLoadFlag(A);
        return 2;
    }

    private byte LDAZpg()
    {
        byte address = ReadNextProgramByte();
        A = Read(address);
        SetLoadFlag(A);
        return 3;
    }

    private byte LDAZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        A = Read(address);
        SetLoadFlag(A);
        return 4;
    }

    private byte LDAAbs()
    {
        ushort address = ReadNext16BitProgram();
        A = Read(address);
        SetLoadFlag(A);
        return 4;
    }

    private byte LDAAbsX()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);
        A = Read(actualAddress);
        SetLoadFlag(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    private byte LDAAbsY()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + Y);
        A = Read(actualAddress);
        SetLoadFlag(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    private byte LDAIndX()
    {
        byte zeroPageAddress = (byte) (ReadNextProgramByte() + X);
        ushort actualAddress = Read16BitZpg(zeroPageAddress);
        A = Read(actualAddress);
        SetLoadFlag(A);
        return 6;
    }

    private byte LDAIndY()
    {
        byte zeroPageAddress = ReadNextProgramByte();
        ushort baseAddress = Read16BitZpg(zeroPageAddress);
        ushort actualAddress = (ushort) (baseAddress + Y);
        A = Read(actualAddress);
        SetLoadFlag(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    #endregion

    #region LDX

    private byte LDXImm()
    {
        X = ReadNextProgramByte();
        SetLoadFlag(X);
        return 2;
    }

    private byte LDXZpg()
    {
        byte address = ReadNextProgramByte();
        X = Read(address);
        SetLoadFlag(X);
        return 3;
    }

    private byte LDXZpgY()
    {
        byte address = (byte) (ReadNextProgramByte() + Y);
        X = Read(address);
        SetLoadFlag(X);
        return 4;
    }

    private byte LDXAbs()
    {
        ushort address = ReadNext16BitProgram();
        X = Read(address);
        SetLoadFlag(X);
        return 4;
    }

    private byte LDXAbsY()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + Y);
        X = Read(actualAddress);
        SetLoadFlag(X);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    #endregion

    #region LDY

    private byte LDYImm()
    {
        Y = ReadNextProgramByte();
        SetLoadFlag(Y);
        return 2;
    }

    private byte LDYZpg()
    {
        byte address = ReadNextProgramByte();
        Y = Read(address);
        SetLoadFlag(Y);
        return 3;
    }

    private byte LDYZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        Y = Read(address);
        SetLoadFlag(Y);
        return 4;
    }

    private byte LDYAbs()
    {
        ushort address = ReadNext16BitProgram();
        Y = Read(address);
        SetLoadFlag(Y);
        return 4;
    }

    private byte LDYAbsX()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);
        Y = Read(actualAddress);
        SetLoadFlag(Y);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    #endregion

    #endregion
}