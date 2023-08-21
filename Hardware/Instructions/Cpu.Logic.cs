// ReSharper disable once CheckNamespace

namespace Hardware;

public partial class Cpu
{
    private void SetBitwiseFlags(byte value)
    {
        Zero = value == 0;
        Negative = (value & 0x80) > 0;
    }

    private byte ANDImm()
    {
        byte value = ReadNextProgramByte();
        A &= value;
        SetBitwiseFlags(A);
        return 2;
    }

    private byte ANDZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        A &= value;
        SetBitwiseFlags(A);
        return 3;
    }

    private byte ANDZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        byte value = Read(address);
        A &= value;
        SetBitwiseFlags(A);
        return 4;
    }

    private byte ANDAbs()
    {
        ushort address = ReadNext16BitProgram();
        byte value = Read(address);
        A &= value;
        SetBitwiseFlags(A);
        return 4;
    }

    private byte ANDAbsX()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);
        byte value = Read(actualAddress);
        A &= value;
        SetBitwiseFlags(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    private byte ANDAbsY()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = Read(actualAddress);
        A &= value;
        SetBitwiseFlags(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 5
            : 4);
    }

    private byte ANDIndX()
    {
        byte zeroPageAddress = (byte) (ReadNextProgramByte() + X);
        ushort actualAddress = Read16Bit(zeroPageAddress);
        byte value = Read(actualAddress);
        A &= value;
        SetBitwiseFlags(A);
        return 6;
    }

    private byte ANDIndY()
    {
        byte zeroPageAddress = ReadNextProgramByte();
        ushort baseAddress = Read16Bit(zeroPageAddress);
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = Read(actualAddress);
        A &= value;
        SetBitwiseFlags(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 6
            : 5);
    }

    private byte ORAImm()
    {
        byte value = ReadNextProgramByte();
        A |= value;
        SetBitwiseFlags(A);
        return 2;
    }

    private byte ORAZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        A |= value;
        SetBitwiseFlags(A);
        return 3;
    }

    private byte ORAZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        byte value = Read(address);
        A |= value;
        SetBitwiseFlags(A);
        return 4;
    }

    private byte ORAAbs()
    {
        ushort address = ReadNext16BitProgram();
        byte value = Read(address);
        A |= value;
        SetBitwiseFlags(A);
        return 4;
    }

    private byte ORAAbsX()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);
        byte value = Read(actualAddress);
        A |= value;
        SetBitwiseFlags(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress) ? 5 : 4);
    }

    private byte ORAAbsY()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = Read(actualAddress);
        A |= value;
        SetBitwiseFlags(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress) ? 5 : 4);
    }

    private byte ORAIndX()
    {
        byte zeroPageAddress = (byte) (ReadNextProgramByte() + X);
        ushort actualAddress = Read16Bit(zeroPageAddress);
        byte value = Read(actualAddress);
        A |= value;
        SetBitwiseFlags(A);
        return 6;
    }

    private byte ORAIndY()
    {
        byte zeroPageAddress = ReadNextProgramByte();
        ushort baseAddress = Read16Bit(zeroPageAddress);
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = Read(actualAddress);
        A |= value;
        SetBitwiseFlags(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress)
            ? 6
            : 5);
    }

    private byte EORImm()
    {
        byte value = ReadNextProgramByte();
        A ^= value;
        SetBitwiseFlags(A);
        return 2;
    }

    private byte EORZpg()
    {
        byte address = ReadNextProgramByte();
        byte value = Read(address);
        A ^= value;
        SetBitwiseFlags(A);
        return 3;
    }

    private byte EORZpgX()
    {
        byte address = (byte) (ReadNextProgramByte() + X);
        byte value = Read(address);
        A ^= value;
        SetBitwiseFlags(A);
        return 4;
    }

    private byte EORAbs()
    {
        var address = ReadNext16BitProgram();
        byte value = Read(address);
        A ^= value;
        SetBitwiseFlags(A);
        return 4;
    }

    private byte EORAbsX()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + X);
        byte value = Read(actualAddress);
        A ^= value;
        SetBitwiseFlags(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress) 
            ? 5 
            : 4);
    }

    private byte EORAbsY()
    {
        ushort baseAddress = ReadNext16BitProgram();
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = Read(actualAddress);
        A ^= value;
        SetBitwiseFlags(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress) 
            ? 5 
            : 4);
    }

    private byte EORIndX()
    {
        byte zeroPageAddress = (byte) (ReadNextProgramByte() + X);
        ushort actualAddress = Read16Bit(zeroPageAddress);
        byte value = Read(actualAddress);
        A ^= value;
        SetBitwiseFlags(A);
        return 6;
    }

    private byte EORIndY()
    {
        byte zeroPageAddress = ReadNextProgramByte();
        ushort baseAddress = Read16Bit(zeroPageAddress);
        ushort actualAddress = (ushort) (baseAddress + Y);
        byte value = Read(actualAddress);
        A ^= value;
        SetBitwiseFlags(A);
        return (byte) (Memory.CrossesPageBoundary(baseAddress, actualAddress) 
            ? 6 
            : 5);
    }

    private void SetBITFlags(byte value)
    {
        byte result = (byte) (A & value);

        Zero = result == 0;
        Negative = (value & 0x80) > 0;
        Overflow = (value & 0x40) > 0;
    }
    
    private byte BITZpg()
    {
        byte zeroPageAddress = ReadNextProgramByte();
        byte value = Read(zeroPageAddress);
        SetBITFlags(value);
        return 3;
    }

    private byte BITAbs()
    {
        ushort address = ReadNext16BitProgram();
        byte value = Read(address);
        SetBITFlags(value);
        return 4;
    }
}