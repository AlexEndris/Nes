namespace Hardware.Audio;

public class Timer
{
    public ushort PeriodReload { get; set; }
    
    private ushort period;

    public bool Clock()
    {
        if (period != 0)
        {
            period--;
            return false;
        }

        period = PeriodReload;
        return true;
    }
}
