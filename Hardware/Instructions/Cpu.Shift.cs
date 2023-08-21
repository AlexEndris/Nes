// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetLeftShiftFlag(ushort value)
    {
        Negative = (value & 0x80) > 0;
        Zero = (value & 0x00FF) == 0;
        Carry = (value & 0xFF00) > 0;
    }

    private void SetRightShiftFlag(byte initial, byte value)
    {
        Negative = false;
        Zero = value == 0;
        Carry = (initial & 0x01) > 0;
    }

    private byte ASLA(Func<ushort> _, ushort __)
    {
        ushort value = (ushort) (A << 1);
        A = (byte)value;
        SetLeftShiftFlag(value);
        return 0;
    }

    private byte ASL(Func<ushort> fetch, ushort address)
    {
        ushort value = fetch();
        value <<= 1;
        SetLeftShiftFlag(value);
        Write(address, (byte)value);
        return 0;
    }

    private byte LSRA(Func<ushort> _, ushort __)
    {
        byte initial = A;
        A >>= 1;
        SetRightShiftFlag(initial, A);
        return 0;
    }

    private byte LSR(Func<ushort> fetch, ushort address)
    {
        byte data = (byte)fetch();
        byte value = data; 
        value >>= 1;
        SetRightShiftFlag(data, value);
        Write(address, value);
        return 0;
    }
}