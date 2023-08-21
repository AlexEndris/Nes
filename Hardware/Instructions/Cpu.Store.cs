// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    #region Accumulator Store Operations (STA)

    private byte STAZpg()
    {
        byte address = ReadNextProgramByte();
        Write(address, A);
        return 3;
    }

    private byte STAZpgX()
    {
        byte address = ReadNextProgramByte();
        Write((byte) (address + X), A);
        return 4;
    }

    private byte STAAbs()
    {
        var data = ReadNext16BitProgram();
        Write(data, A);
        return 4;
    }

    private byte STAAbsX()
    {
        ushort address = ReadNext16BitProgram();
        Write((ushort) (address + X), A);
        return 5;
    }

    private byte STAAbsY()
    {
        ushort address = ReadNext16BitProgram();
        Write((ushort) (address + Y), A);
        return 5;
    }

    private byte STAIndX()
    {
        byte zeroPageAddress = (byte) (ReadNextProgramByte() + X);
        ushort actualAddress = Read16BitZpg(zeroPageAddress);
        Write(actualAddress, A);
        return 6;
    }

    private byte STAIndY()
    {
        byte zeroPageAddress = ReadNextProgramByte();
        ushort actualAddress = (ushort) (Read16BitZpg(zeroPageAddress) + Y);
        Write(actualAddress, A);
        return 6;
    }

    #endregion

    #region X Register Store Operations (STX)

    private byte STXZpg()
    {
        byte address = ReadNextProgramByte();
        Write(address, X);
        return 3;
    }

    private byte STXZpgY()
    {
        byte address = ReadNextProgramByte();
        Write((byte) (address + Y), X);
        return 4;
    }

    private byte STXAbs()
    {
        byte lowByte = ReadNextProgramByte();
        byte highByte = ReadNextProgramByte();
        Write(ByteUtil.To16Bit(highByte, lowByte), X);
        return 4;
    }

    #endregion

    #region Y Register Store Operations (STY)

    private byte STYZpg()
    {
        byte address = ReadNextProgramByte();
        Write(address, Y);
        return 3;
    }

    private byte STYZpgX()
    {
        byte address = ReadNextProgramByte();
        Write((byte) (address + X), Y);
        return 4;
    }

    private byte STYAbs()
    {
        byte lowByte = ReadNextProgramByte();
        byte highByte = ReadNextProgramByte();
        Write(ByteUtil.To16Bit(highByte, lowByte), Y);
        return 4;
    }

    #endregion
}