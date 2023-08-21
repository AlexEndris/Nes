// ReSharper disable once CheckNamespace
namespace Hardware;

public partial class Cpu
{
    private void SetDecFlags(byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
    }
    
    private byte DEC(ushort data, ushort address)
    {
        byte value = (byte) data;
        value--;
        SetDecFlags(value);
        Write(address, value);
        return 0;
    }

    private byte DEX(ushort _, ushort __)
    {
        X--;
        SetDecFlags(X);
        return 0;
    }

    private byte DEY(ushort _, ushort __) 
    {
        Y--;
        SetDecFlags(Y);
        return 0;
    }
}