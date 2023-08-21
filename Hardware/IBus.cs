namespace Hardware;

public interface IBus
{
    byte Read(ushort address);
    ushort Read16Bit(ushort address);
    void Write(ushort address, byte value);
}