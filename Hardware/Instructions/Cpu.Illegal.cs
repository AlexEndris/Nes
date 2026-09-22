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

    private byte MemNOP(Func<ushort> fetch, ushort __)
    {
        fetch();
        return 1;
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
        A = (byte) (value >> 1 | (Carry ? 0x80 : 0));

        Zero = A == 0;
        Negative = (A & 0x80) > 0;
        Carry = (A & 0x40) > 0;
        Overflow = ((Carry ? 0x1 : 0) ^ A >> 5 & 0x1) != 0;
        
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

    private byte SLO(Func<ushort> fetch, ushort address)
    {
        ushort value = fetch();
        value <<= 1;
        
        Carry = (value & 0xFF00) > 0;
        Write(address, (byte)value);

        A |= (byte)value;
        
        SetBitwiseFlags(A);
        
        return 0;
    }

    private byte RLA(Func<ushort> fetch, ushort address)
    {
        ushort value = fetch();
        value = value.RotateLeft(Carry);

        Carry = (value & 0xFF00) > 0;
        Write(address, (byte) value);

        A &= (byte)value;
        SetBitwiseFlags(A);

        return 0;
    }

    private byte SRE(Func<ushort> fetch, ushort address)
    {
        byte data = (byte)fetch();
        byte value = data; 
        value >>= 1;
        
        Carry = (data & 0x01) > 0;
        Write(address, value);
        
        A ^= value;
        SetBitwiseFlags(A);

        return 0;
    }

    private byte RRA(Func<ushort> fetch, ushort address)
    {
        byte value = (byte)fetch();
        byte initial = value;
        value = value.RotateRight(Carry);
        Carry = (initial & 0x01) > 0;
        Write(address, value);

        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForAdc(A, value, sum);
        A = (byte) sum;

        return 0;
    }

    private byte SAX(Func<ushort> fetch, ushort address)
    {
        byte result = (byte)(A & X);
        Write(address, result);
        
        return 0;
    }

    private byte LAX(Func<ushort> fetch, ushort address)
    {
        X = A = (byte) fetch();
        SetLoadFlag(X);
        
        return 1;
    }

    private byte DCP(Func<ushort> fetch, ushort address)
    {
        byte value = (byte) fetch();
        value--;
        Write(address, value);
        
        SetFlagsForCmp(A, value);
        
        return 0;
    }
    
    private byte ISC(Func<ushort> fetch, ushort address)
    {
        byte value = (byte) fetch();
        value++;
        Write(address, value);
        
        ushort negValue = (ushort) (value ^ 0x00FF) ;
        ushort sum = (ushort) (A + negValue + Carry.ToByte());
        SetFlagsForSBC(A, negValue, sum);
        A = (byte)sum;
        
        return 0;
    }

    private byte SHA(Func<ushort> fetch, ushort address)
    {
        ushort baseAddress = (ushort)(address - Y);
        byte value = (byte)(A & X & (baseAddress >> 8) + 1);

        ushort destination = Memory.CrossesPageBoundary(baseAddress, address)
            ? (ushort)(value << 8 | address & 0x00FF)
            : address;
        
        Write(destination, value);
        
        return 0;
    }

    private byte SHS(Func<ushort> fetch, ushort address)
    {
        SP = (byte)(A & X);
        
        ushort baseAddress = (ushort)(address - Y);
        byte value = (byte)(A & X & (baseAddress >> 8) + 1);

        ushort destination = Memory.CrossesPageBoundary(baseAddress, address)
            ? (ushort)(value << 8 | address & 0x00FF)
            : address;
        
        Write(destination, value);
        
        return 0;
    }

    private byte SHY(Func<ushort> fetch, ushort address)
    {
        ushort baseAddress = (ushort)(address - X);
        byte value = (byte)(Y & (baseAddress >> 8) + 1);

        ushort destination = Memory.CrossesPageBoundary(baseAddress, address)
            ? (ushort)(value << 8 | address & 0x00FF)
            : address;
        
        Write(destination, value);
        
        return 0;
    }

    private byte SHX(Func<ushort> fetch, ushort address)
    {
        ushort baseAddress = (ushort)(address - Y);
        byte value = (byte)(X & (baseAddress >> 8) + 1);

        ushort destination = Memory.CrossesPageBoundary(baseAddress, address)
            ? (ushort)(value << 8 | address & 0x00FF)
            : address;
        
        Write(destination, value);
        
        return 0;
    }

    private byte LAE(Func<ushort> fetch, ushort address)
    { 
        byte value =  (byte) fetch();
        SP = A = X = (byte)(value & SP);
        SetLoadFlag(A);
        
        return 1;   
    }
}