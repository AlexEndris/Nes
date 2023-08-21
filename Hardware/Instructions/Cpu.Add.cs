// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private void SetFlagsForADC(byte originalA, byte addedValue, ushort sum)
    {
        Zero = (byte)sum == 0;
        Carry = sum > 0xff;
        Negative = (sum & 0x80) > 0;
        Overflow = IsOverflow(originalA, addedValue, sum);
    }

    private byte ADCImm()
    {
        byte value = ReadNextProgramByte();
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForADC(A, value, sum);
        A = (byte) sum;
        return 2;
    }

    private byte ADCZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForADC(A, value, sum);

        A = (byte) sum;
        return 3;
    }

    private byte ADCZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        byte value = Read(address);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForADC(A, value, sum);

        A = (byte) sum;
        return 4;
    }

    private byte ADCAbs()
    {
        ushort address = ReadNext16BitProgram();

        byte value = Read(address);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForADC(A, value, sum);

        A = (byte) sum;
        return 4;
    }

    private byte ADCAbsX()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);

        byte value = Read(actualAddress);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForADC(A, value, sum);

        A = (byte) sum;
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    private byte ADCAbsY()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + Y);

        byte value = Read(actualAddress);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForADC(A, value, sum);

        A = (byte) sum;
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    private byte ADCIndX()
    {
        byte zeroPageAddress = (byte) (ReadNextProgramByte() + X);
        ushort actualAddress = Read16Bit(zeroPageAddress);
        byte value = Read(actualAddress);

        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForADC(A, value, sum);

        A = (byte) sum;
        return 6;
    }

    private byte ADCIndY()
    {
        byte zeroPageAddress = ReadNextProgramByte();
        ushort baseAddress = Read16Bit(zeroPageAddress);
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = Read(actualAddress);

        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForADC(A, value, sum);

        A = (byte) sum;
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 6
            : 5);
    }
}