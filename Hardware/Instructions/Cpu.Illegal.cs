namespace Hardware;

public partial class Cpu
{
    private byte DOP(ushort _, ushort __)
    {
        return 0;
    }

    private byte AAC(ushort data, ushort _)
    {
        byte value = (byte) data;
        A = (byte) (A & value);

        Negative = (A & 0x80) > 0;
        Zero = A == 0;
        Carry = Negative;

        return 2;
    }

    private byte ASR(ushort data, ushort _)
    {
        byte value = (byte) data;
        value = (byte) (A & value);
        
        Carry = (value & 0x1) > 0;
        A = (byte) (value >> 1);

        Negative = (A & 0x80) > 0;
        Zero = A == 0;
        
        return 0;
    }

    private byte ARR(ushort data, ushort _)
    {
        byte value = (byte) data;
        value = (byte) (A & value);
        A = (byte) ((value >> 1) | (Carry ? 0x80 : 0));

        Zero = A == 0;
        Negative = (A & 0x80) > 0;
        Carry = (A & 0x40) > 0;
        Overflow = ((Carry ? 0x1 : 0) ^ ((A >> 5) & 0x1)) != 0;
        
        return 0;
    }

    private byte ATX(ushort data, ushort __)
    {
        byte value = (byte)data;
        A = value;
        X = value;

        Zero = A == 0;
        Negative = (A & 0x80) > 0;
        
        return 0;
    }

    private byte AXS(ushort data, ushort __)
    {
        byte value = (byte) ((A & X) - (byte) data);

        Carry = (A & X) >= data;

        X = value;

        Zero = X == 0;
        Negative = (X & 0x80) > 0;
        
        return 0;
    }
}