// ReSharper disable once CheckNamespace
namespace Hardware;

public partial class Cpu
{
    private void SetFlagsForSBC(byte originalA, ushort subtractedValue, ushort sum)
    {
        Zero = (sum & 0xFF) == 0;
        Carry = (sum & 0xFF00) > 0;
        Negative = (sum & 0x80) > 0;
        Overflow = IsOverflow(originalA, subtractedValue, sum);
    }

    private byte SBCImm()
    {
        ushort value = (ushort) (ReadNextProgramByte() ^ 0x00FF) ;
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForSBC(A, value, sum);
        A = (byte)sum;
        return 2;
    }

    private byte SBCZpg()
    {
        byte address = ReadNextProgramByte();
        ushort value = (ushort)(Read(address) ^ 0x00FF);
        ushort sum = (ushort) (A + value + Carry.ToByte());
        SetFlagsForSBC(A, value, sum);

        A = (byte) sum;
        return 3;
    }

    private byte SBCZpgX()
    {
        byte address = (byte)(ReadNextProgramByte()+X);
        ushort value = (ushort)(Read(address) ^ 0x00FF);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForSBC(A, value, sum);
        A = (byte) sum;
        return 4;
    }

    private byte SBCAbs()
    {
        byte lowByte = ReadNextProgramByte();
        byte highByte = ReadNextProgramByte();
        ushort actualAddress = ByteUtil.To16Bit(highByte, lowByte);
        ushort value = (ushort)(Read(actualAddress) ^ 0x00FF);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForSBC(A, value, sum);
        A = (byte) sum;
        return 4;
    }

    private byte SBCAbsX()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);
        byte value = (byte) ~Read(actualAddress);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForSBC(A, value, sum);
        A = (byte) sum;
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    private byte SBCAbsY()
    {
ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = (byte) ~Read(actualAddress);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForSBC(A, value, sum);
        A = (byte) sum;
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);    
    }

    private byte SBCIndX()
    {
        byte zeroPageAddress = (byte)(ReadNextProgramByte()+X);
        ushort actualAddress = Read16Bit(zeroPageAddress);
        byte value = (byte) ~Read(actualAddress);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForSBC(A, value, sum);
        A = (byte) sum;
        return 6;
    }

    private byte SBCIndY()
    {
        byte zeroPageAddress = ReadNextProgramByte();
        ushort baseAddress = Read16Bit(zeroPageAddress);
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = (byte) ~Read(actualAddress);
        ushort sum = (ushort) (A + value + Carry.ToByte());

        SetFlagsForSBC(A, value, sum);
        A = (byte) sum;
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 6
            : 5);    
    }
}