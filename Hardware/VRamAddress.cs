namespace Hardware;

public struct VRamAddress {
    public ushort Raw;

    public byte CoarseX
    {
        get => (byte) (Raw & 0x1F);
        set => Raw = (ushort) (Raw & 0x7FE0 | value & 0x1F);
    }

    public byte CoarseY
    {
        get => (byte) (Raw >> 5 & 0x1F);
        set => Raw = (ushort) (Raw & 0x7C1F | (value & 0x1F) << 5);
    }

    public byte NametableX
    {
        get => (byte) ((Raw >> 10) & 0x1);
        set => Raw = (ushort) (Raw & 0x7BFF | (value << 10));
    }
    
    public byte NametableY
    {
        get => (byte) ((Raw >> 11) & 0x1);
        set => Raw = (ushort) (Raw & 0x77FF | (value << 11));
    }
    
    public byte FineY
    {
        get => (byte) (Raw >> 12 & 0x7);
        set => Raw = (ushort) (Raw & 0xFFF | (value & 0x7) << 12);
    }

    public static implicit operator ushort(VRamAddress address)
    {
        return address.Raw;
    }    
    
    public static implicit operator VRamAddress(ushort address)
    {
        return new VRamAddress {Raw = address};
    }
}