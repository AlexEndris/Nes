using System.Runtime.InteropServices;

namespace Hardware;

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct ObjectAttributeEntry
{
    public byte Y = 0xFF;
    public byte Id = 0xFF;
    public byte Attribute = 0xFF;
    public byte X = 0xFF;

    // Index only for 8x16
    public ushort Bank8x16 => (ushort) (Id & 0x1 << 12);
    public byte TileId8x16 => (byte) (Id & 0xFE);
    
    // Attribute
    public byte Palette => (byte) (Attribute & 0x3);
    public bool Priority => (Attribute & 0x20) > 0;
    public bool FlipHorizontal => (Attribute & 0x40) > 0;
    public bool FlipVertical => (Attribute & 0x80) > 0;
    
    public ObjectAttributeEntry()
    {
    }
}