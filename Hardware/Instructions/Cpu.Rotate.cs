// ReSharper disable once CheckNamespace

using System;

namespace Hardware;

public partial class Cpu
{
    private void SetLeftRotateFlag(ushort value)
    {
        Negative = (value & 0x80) > 0;
        Zero = (value & 0x00FF) == 0;
        Carry = (value & 0xFF00) > 0;
    }

    private void SetRightRotateFlag(byte initial, byte value)
    {
        Negative = (value & 0x80) > 0;
        Zero = value == 0;
        Carry = (initial & 0x01) > 0;
    }

    private byte ROLA(Func<ushort> _, ushort __)
    {
        ushort value = A;
        value = value.RotateLeft(Carry);
        A = (byte) value;
        SetLeftRotateFlag(value);
        return 0;
    }

    private byte ROL(Func<ushort> fetch, ushort address)
    {
        ushort value = fetch();
        value = value.RotateLeft(Carry);
        SetLeftRotateFlag(value);
        Write(address, (byte) value);
        return 0;
    }

    private byte RORA(Func<ushort> _, ushort __)
    {
        byte initial = A;
        A = A.RotateRight(Carry);
        SetRightRotateFlag(initial, A);
        return 0;
    }

    private byte ROR(Func<ushort> fetch, ushort address)
    {
        byte value = (byte) fetch();
        byte initial = value;
        value = value.RotateRight(Carry);
        SetRightRotateFlag(initial, value);
        Write(address, value);
        return 0;
    }
}