namespace Hardware.Audio;

public class Triangle
{
    public LengthCounter Counter { get; } = new();
    public LinearCounter Linear { get; } = new();
    public Timer Timer { get; } = new();

    private byte sequence;
    
    private byte[] sequenceLookup = {
        15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
    };
    
    public ushort GetSample()
    {
        if (Timer.PeriodReload <= 2)
            return 0;

        return sequenceLookup[sequence];
    }
    
    public void Clock()
    {
        if (Counter.Value == 0
            || Linear.Value == 0)
            return;

        if (!Timer.Clock())
        {
            return;
        }

        if (sequence >= 31)
        {
            sequence = 0;
            return;
        }

        sequence += 1;
    }
}