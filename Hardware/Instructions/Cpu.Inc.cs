// ReSharper disable once CheckNamespace
namespace Hardware;

public partial class Cpu
{
    private void SetIncFlags(byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
    }
    
    private byte INC(ushort data, ushort address)
    {
        byte value = (byte) data;
        value++;
        SetIncFlags(value);
        Write(address, value);
        return 5;
    }

    private byte INX(ushort _, ushort __)
    {
        X++;
        SetIncFlags(X);
        return 0;
    }

    private byte INY(ushort _, ushort __) 
    {
        Y++;
        SetIncFlags(Y);
        return 0;
    }
}