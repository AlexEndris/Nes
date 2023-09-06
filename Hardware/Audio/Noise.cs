namespace Hardware.Audio;

public class Noise
{
    public Envelope Envelope { get; } = new();
    public LengthCounter Counter { get; } = new();
    public bool Mode { get; set; }
    public ushort PeriodReload { get; set; }
    
    private ushort[] periodLookup =
    {
        4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068
    };

    private ushort period;
    private ushort shift = 1;

    public void Load(byte value)
    {
        PeriodReload = periodLookup[value];
    }
    
    public ushort GetSample()
    {
        if (Counter.Value == 0)
            return 0;

        ushort sample = (ushort) (shift & 0b1);

        return (ushort) (sample * Envelope.Volume);
    }
    
    public void Clock()
    {
        if (period != 0)
        {
            period--;
            return;
        }

        period = (ushort) (PeriodReload - 1);

        int feedback = shift & 0b1;

        if (Mode)
            feedback ^= (shift >> 6) & 0b1;
        else
            feedback ^= (shift >> 1) & 0b1;

        shift >>= 1;
        shift |= (ushort) (feedback << 14);
    }
}