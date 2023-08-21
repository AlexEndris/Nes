using System;

namespace Hardware;

public partial class Cpu
{
    private byte DOP(Func<ushort> _, ushort __)
    {
        return 0;
    }

    private byte AAC(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        A = (byte) (A & value);

        Negative = (A & 0x80) > 0;
        Zero = A == 0;
        Carry = Negative;

        return 2;
    }

    private byte ASR(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        value = (byte) (A & value);
        
        Carry = (value & 0x1) > 0;
        A = (byte) (value >> 1);

        Negative = (A & 0x80) > 0;
        Zero = A == 0;
        
        return 0;
    }

    private byte ARR(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        value = (byte) (A & value);
        A = (byte) ((value >> 1) | (Carry ? 0x80 : 0));

        Zero = A == 0;
        Negative = (A & 0x80) > 0;
        Carry = (A & 0x40) > 0;
        Overflow = ((Carry ? 0x1 : 0) ^ ((A >> 5) & 0x1)) != 0;
        
        return 0;
    }

    private byte ATX(Func<ushort> fetch, ushort __)
    {
        byte value = (byte)fetch();
        A = value;
        X = value;

        Zero = A == 0;
        Negative = (A & 0x80) > 0;
        
        return 0;
    }

    private byte AXS(Func<ushort> fetch, ushort __)
    {
        byte data = (byte)fetch();
        byte value = (byte) ((A & X) - data);

        Carry = (A & X) >= data;

        X = value;

        Zero = X == 0;
        Negative = (X & 0x80) > 0;
        
        return 0;
    }
}