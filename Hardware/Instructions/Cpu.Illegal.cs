using System;

namespace Hardware;

public partial class Cpu
{
    private const byte MAGIC = 0xFF;
    
    private byte DOP(Func<ushort> _, ushort __)
    {
        return 0;
    }
    
    private byte NOP(Func<ushort> _, ushort __)
    {
        return 0;
    }

    private byte ANC(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        A = (byte) (A & value);

        Negative = (A & 0x80) > 0;
        Zero = A == 0;
        Carry = Negative;

        return 2;
    }

    private byte ALR(Func<ushort> fetch, ushort _)
    {
        byte value = (byte) fetch();
        value = (byte) (A & value);
        
        Carry = (value & 0x1) > 0;
        A = (byte) (value >> 1);

        Negative = (A & 0x80) > 0;
        Zero = A == 0;
        
        return 0;
    }

    private byte ANE(Func<ushort> fetch, ushort _)
    {
        byte value = (byte)fetch();
        byte result = (byte)((A | MAGIC) & X & value);

        A = result;
            
        Zero = A == 0;
        Negative = (A & 0x80) > 0;
        
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

    private byte LXA(Func<ushort> fetch, ushort __)
    {
        byte value = (byte)fetch();
        byte result = (byte)((A | MAGIC) & value); 
        
        A = result;
        X = result;

        Zero = A == 0;
        Negative = (A & 0x80) > 0;
        
        return 0;
    }

    private byte SBX(Func<ushort> fetch, ushort __)
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