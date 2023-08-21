// ReSharper disable once CheckNamespace
namespace Hardware;

public partial class Cpu
{
    private void SetIncFlags(byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
    }
    
    private byte INCZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        value++;
        SetIncFlags(value);
        Write(address, value);
        return 5;
    }

    private byte INCZpgX() 
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        byte value = Read(address);
        value++;
        SetIncFlags(value);
        Write(address, value);
        return 6;
    }

    private byte INCAbs()
    {
        ushort actualAddress = ReadNext16BitProgram();
        byte value = Read(actualAddress);
        value++;
        SetIncFlags(value);
        Write(actualAddress, value);
        return 6;
    }

    private byte INCAbsX() 
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);
        byte value = Read(actualAddress);
        value++;
        SetIncFlags(value);
        Write(actualAddress, value);
        return 7;
    }

    private byte INX()
    {
        X++;
        SetIncFlags(X);
        return 2;
    }

    private byte INY() 
    {
        Y++;
        SetIncFlags(Y);
        return 2;
    }
}