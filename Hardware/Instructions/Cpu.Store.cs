// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private byte STA(Func<ushort> _, ushort address)
    {
        Write(address, A);
        return 0;
    }

    private byte STX(Func<ushort> _, ushort address)
    {
        Write(address, X);
        return 0;
    }

    private byte STY(Func<ushort> _, ushort address)
    {
        Write(address, Y);
        return 0;
    }
}