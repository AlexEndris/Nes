// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetIncFlags(byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
    }
    
    private byte INC(Func<ushort> fetch, ushort address)
    {
        byte value = (byte) fetch();
        value++;
        SetIncFlags(value);
        Write(address, value);
        return 0;
    }

    private byte INX(Func<ushort> _, ushort __)
    {
        X++;
        SetIncFlags(X);
        return 0;
    }

    private byte INY(Func<ushort> _, ushort __) 
    {
        Y++;
        SetIncFlags(Y);
        return 0;
    }
}