// ReSharper disable once CheckNamespace

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

    private byte ASLAcc()
    {
        ushort value = (ushort) (A << 1);
        A = (byte)value;
        SetLeftShiftFlag(value);
        return 2;
    }

    private byte ASLZpg()
    {
        byte address = ReadNextProgramByte();
        ushort value = Read(address);
        value <<= 1;
        SetLeftShiftFlag(value);
        Write(address, (byte)value);
        return 5;
    }

    private byte ASLZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        ushort value = Read(address);
        value <<= 1;
        SetLeftShiftFlag(value);
        Write(address, (byte)value);
        return 6;
    }

    private byte ASLAbs()
    {
        ushort address = ReadNext16BitProgram();
        ushort value = Read(address);
        value <<= 1;
        SetLeftShiftFlag(value);
        Write(address, (byte)value);
        return 6;
    }

    private byte ASLAbsX()
    {
        ushort address = (ushort) (ReadNext16BitProgram() + X);
        ushort value = Read(address);
        value <<= 1;
        SetLeftShiftFlag(value);
        Write(address, (byte)value);
        return 7;
    }

    private byte LSRAcc()
    {
        byte initial = A;
        A >>= 1;
        SetRightShiftFlag(initial, A);
        return 2;
    }

    private byte LSRZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        byte initial = value;
        value >>= 1;
        SetRightShiftFlag(initial, value);
        Write(address, value);
        return 5;
    }

    private byte LSRZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        byte value = Read(address);
        byte initial = value;
        value >>= 1;
        SetRightShiftFlag(initial, value);
        Write(address, value);
        return 6;
    }

    private byte LSRAbs()
    {
        ushort address = ReadNext16BitProgram();
        byte value = Read(address);
        byte initial = value;
        value >>= 1;
        SetRightShiftFlag(initial, value);
        Write(address, value);
        return 6;
    }

    private byte LSRAbsX()
    {
        ushort address = (ushort) (ReadNext16BitProgram() + X);
        byte value = Read(address);
        byte initial = value;
        value >>= 1;
        SetRightShiftFlag(initial, value);
        Write(address, value);
        return 7;
    }
}