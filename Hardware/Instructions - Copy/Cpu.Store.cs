// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private byte STA(ushort _, ushort address)
    {
        Write(address, A);
        return 0;
    }

    private byte STX(ushort _, ushort address)
    {
        Write(address, X);
        return 0;
    }

    private byte STY(ushort _, ushort address)
    {
        Write(address, Y);
        return 0;
    }
}