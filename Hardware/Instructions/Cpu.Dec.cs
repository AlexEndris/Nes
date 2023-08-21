// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetDecFlags(byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
    }
    
    private byte DEC(Func<ushort> fetch, ushort address)
    {
        byte value = (byte) fetch();
        value--;
        SetDecFlags(value);
        Write(address, value);
        return 0;
    }

    private byte DEX(Func<ushort> _, ushort __)
    {
        X--;
        SetDecFlags(X);
        return 0;
    }

    private byte DEY(Func<ushort> _, ushort __) 
    {
        Y--;
        SetDecFlags(Y);
        return 0;
    }
}