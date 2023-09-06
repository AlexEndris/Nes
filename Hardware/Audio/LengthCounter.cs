namespace Hardware.Audio;

public class LengthCounter
{
    private static byte[] lookupTable =
    {
        10, 254, 20, 2, 40, 4, 80, 6,
        160, 8, 60, 10, 14, 12, 26, 14,
        12, 16, 24, 18, 48, 20, 96, 22,
        192, 24, 72, 26, 16, 28, 32, 30
    };
    private bool enabled;

    public byte Value { get; private set; }
    public bool Halt { get; set; }

    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;

            if (!enabled)
                Value = 0;
        }
    }

    public void Load(byte value)
    {
        if (!enabled)
            return;
        
        Value = lookupTable[value];
    }
    
    public void Clock()
    {
        if (Value == 0
            || Halt) 
            return;
        
        Value--;
    }
}