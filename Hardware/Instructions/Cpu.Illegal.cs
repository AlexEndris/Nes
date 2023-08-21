namespace Hardware;

public partial class Cpu
{
    private byte DOP()
    {
        ReadNextProgramByte();

        return 3;
    }

    private byte AAC()
    {
        byte value = ReadNextProgramByte();
        A = (byte) (A & value);

        Negative = (A & 0x80) > 0;
        Zero = A == 0;
        Carry = Negative;

        return 2;
    }

    private byte ASR()
    {
        byte value = ReadNextProgramByte();
        value = (byte) (A & value);
        
        Carry = (value & 0x1) > 0;
        A = (byte) (value >> 1);

        Negative = (A & 0x80) > 0;
        Zero = A == 0;
        
        return 2;
    }

    private byte ARR()
    {
        byte value = ReadNextProgramByte();
        value = (byte) (A & value);
        A = (byte) ((value >> 1) | (Carry ? 0x80 : 0));

        Zero = A == 0;
        Negative = (A & 0x80) > 0;
        Carry = (A & 0x40) > 0;
        Overflow = ((Carry ? 0x1 : 0) ^ ((A >> 5) & 0x1)) != 0;
        
        return 2;
    }

    private byte ATX()
    {
        byte value = ReadNextProgramByte();
        A = value;
        X = value;

        Zero = A == 0;
        Negative = (A & 0x80) > 0;
        
        return 2;
    }

    private byte AXS()
    {
        byte initial = ReadNextProgramByte();
        byte value = (byte) ((A & X) - initial);

        Carry = (A & X) >= initial;

        X = value;

        Zero = X == 0;
        Negative = (X & 0x80) > 0;
        
        return 2;
    }
}