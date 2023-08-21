// ReSharper disable once CheckNamespace
namespace Hardware;

public partial class Cpu
{
    private void SetDecFlags(byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
    }
    
    private byte DECZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        value--;
        SetDecFlags(value);
        Write(address, value);
        return 5;
    }

    private byte DECZpgX() 
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        byte value = Read(address);
        value--;
        SetDecFlags(value);
        Write(address, value);
        return 6;
    }

    private byte DECAbs()
    {
        byte lowByte = ReadNextProgramByte();
        byte highByte = ReadNextProgramByte();
        ushort actualAddress = ByteUtil.To16Bit(highByte, lowByte);
        byte value = Read(actualAddress);
        value--;
        SetDecFlags(value);
        Write(actualAddress, value);
        return 6;
    }

    private byte DECAbsX() 
    {
ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);
        byte value = Read(actualAddress);
        value--;
        SetDecFlags(value);
        Write(actualAddress, value);
        return 7;
    }

    public byte DEX()
    {
        X--;
        SetDecFlags(X);
        return 2;
    }

    public byte DEY() 
    {
        Y--;
        SetDecFlags(Y);
        return 2;
    }
}