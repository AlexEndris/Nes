using static System.Math;

namespace Hardware.Audio;

public class SquarePulse
{
    private byte[] dutyCycleLookup = 
    {
        0b1000_0000,
        0b1100_0000,
        0b1111_0000,
        0b0011_1111
    };
    
    public byte DutyCycleIndex { get; set; }

    public LengthCounter Counter { get; } = new();
    public Envelope Envelope { get; } = new();
    public Timer Timer { get; } = new();

    private byte sequence;
    
    public ushort GetSample()
    {
        if (Counter.Value == 0)
            return 0;

        ushort target = TargetPeriod;

        if (target > 0x7FF || Timer.PeriodReload < 8)
            return 0;
        
        ushort sample = (ushort) ((dutyCycleLookup[DutyCycleIndex] >> sequence) & 0x1);

        return (ushort) (sample * Envelope.Volume);
    }

    public void Clock()
    {
        if (!Timer.Clock())
        {
            return;
        }
        
        if (sequence != 0)
        {
            sequence--;
            return;
        }

        sequence = 7;
    }

    #region Sweep
    
    public bool SweepEnabled { get; set; }
    public byte SweepDividerPeriod { get; set; }
    public bool SweepNegate { get; set; }
    public byte SweepShift { get; set; }
    public bool OnesComplement { get; set; }
    public bool SweepReload { get; set; }
 
    private byte divider;
   
    public ushort TargetPeriod
    {
        get
        {
            ushort change = (ushort) (Timer.PeriodReload >> SweepShift);

            if (!SweepNegate)
                return (ushort) (Timer.PeriodReload + change);

            if (!OnesComplement)
                return (ushort) (Timer.PeriodReload - change);

            if (SweepShift == 0 || Timer.PeriodReload == 0)
                return 0 ;

            return (ushort) (Timer.PeriodReload - change - 1);
        }
    }

    public void UpdateSweep()
    {
        ushort target = TargetPeriod;

        if (SweepEnabled
            && divider == 0
            && SweepShift != 0
            && target <= 0x7FF
            && Timer.PeriodReload >= 8)
            Timer.PeriodReload = target;

        if (divider != 0
            && !SweepReload)
        {
            divider--;
            return;
        }

        divider = SweepDividerPeriod;
        SweepReload = false;
    }
    

    #endregion
}