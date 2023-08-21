namespace Hardware;

public struct PpuCtrl
{
    public byte Raw;

    public byte Nametable
    {
        get => (byte) (Raw & 0x3);
        set => Raw = (byte) ((Raw & 0xFC) | value & 0x3);
    }

    public bool SpriteSize => ((Raw & 0x20) > 0); 
    public byte BackgroundTableBase => (byte) ((Raw >> 4) & 0x1);
    public byte SpriteTableBase => (byte) ((Raw >> 3) & 0x1);
    public byte VramIncrement => (byte) ((Raw & 0x4) == 0 ? 1 : 32);
    
    public bool EnableNmi => Raw >> 7 == 1;


    public static implicit operator byte(PpuCtrl address)
    {
        return address.Raw;
    }    
    
    public static implicit operator PpuCtrl(byte address)
    {
        return new PpuCtrl {Raw = address};
    }
}