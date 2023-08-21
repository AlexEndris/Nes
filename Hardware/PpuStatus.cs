namespace Hardware;

public struct PpuStatus
{
    public byte Raw;
    
    public bool VerticalBlank
    {
        get => Raw >> 7 == 1;
        set => Raw = (byte) (Raw & 0x7F | (value ? 0x80 : 0));
    }

    public bool SpriteZeroHit
    {
        get => (Raw & 0x40) > 0;
        set => Raw = (byte) (Raw & 0xBF | (value ? 0x40 : 0));
    }
    
    public bool SpriteOverflow
    {
        get => (Raw & 0x20) > 0;
        set => Raw = (byte) (Raw & 0xDF | (value ? 0x20 : 0));
    }
    
    
    public static implicit operator byte(PpuStatus address)
    {
        return address.Raw;
    }    
    
    public static implicit operator PpuStatus(byte address)
    {
        return new PpuStatus {Raw = address};
    }
}