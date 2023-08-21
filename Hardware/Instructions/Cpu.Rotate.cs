// ReSharper disable once CheckNamespace

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

    private byte ROLAcc()
    {
        ushort value = A;
        value = value.RotateLeft(Carry);
        A = (byte) value;
        SetLeftRotateFlag(value);
        return 2;
    }

    private byte ROLZpg()
    {
        byte address = ReadNextProgramByte();
        ushort value = Read(address);
        value = value.RotateLeft(Carry);
        SetLeftRotateFlag(value);
        Write(address, (byte) value);
        return 5;
    }

    private byte ROLZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        ushort value = Read(address);
        value = value.RotateLeft(Carry);
        SetLeftRotateFlag(value);
        Write(address, (byte)value);
        return 6;
    }

    private byte ROLAbs()
    {
        ushort address = ReadNext16BitProgram();
        ushort value = Read(address);
        value = value.RotateLeft(Carry);
        SetLeftRotateFlag(value);
        Write(address, (byte)value);
        return 6;
    }

    private byte ROLAbsX()
    {
        ushort address = ReadNext16BitProgram();
        ushort value = Read(address);
        value = value.RotateLeft(Carry);
        SetLeftRotateFlag(value);
        Write(address, (byte) value);
        return 7;
    }

    private byte RORAcc()
    {
        byte initial = A;
        A = A.RotateRight(Carry);
        SetRightRotateFlag(initial, A);
        return 2;
    }

    private byte RORZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        byte initial = value;
        value = value.RotateRight(Carry);
        SetRightRotateFlag(initial, value);
        Write(address, value);
        return 5;
    }

    private byte RORZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        byte value = Read(address);
        byte initial = value;
        value = value.RotateRight(Carry);
        SetRightRotateFlag(initial, value);
        Write(address, value);
        return 6;
    }

    private byte RORAbs()
    {
        ushort address = ReadNext16BitProgram();
        byte value = Read(address);
        byte initial = value;
        value = value.RotateRight(Carry);
        SetRightRotateFlag(initial, value);
        Write(address, value);
        return 6;
    }

    private byte RORAbsX()
    {
        ushort address = (ushort) (ReadNext16BitProgram() + X);
        byte value = Read(address);
        byte initial = value;
        value = value.RotateRight(Carry);
        SetRightRotateFlag(initial, value);
        Write(address, value);
        return 7;
    }
}