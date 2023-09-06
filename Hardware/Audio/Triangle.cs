namespace Hardware.Audio;

public class Triangle
{
    public LengthCounter Counter { get; } = new();
    public LinearCounter Linear { get; } = new();

    public ushort PeriodReload { get; set; }
    
    private ushort period;
    private byte sequence;
    
    private byte[] sequenceLookup = {
        15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
    };
    
    public ushort GetSample()
    {
        if (PeriodReload <= 2)
            return 7;

        return sequenceLookup[sequence];
    }
    
    public void Clock()
    {
        if (Counter.Value == 0
            || Linear.Value == 0)
            return;

        if (period != 0)
        {
            period--;
            return;
        }

        period = PeriodReload;

        if (sequence >= 31)
        {
            sequence = 0;
            return;
        }

        sequence += 1;
    }
}