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

    private ushort period;
    private byte sequence;
    
    public ushort PeriodReload { get; set; }
    
    public ushort GetSample()
    {
        if (Counter.Value == 0)
            return 0;

        ushort target = TargetPeriod;

        if (target > 0x7FF || PeriodReload < 8)
            return 0;
        
        ushort sample = (ushort) ((dutyCycleLookup[DutyCycleIndex] >> sequence) & 0x1);

        return (ushort) (sample * Envelope.Volume);
    }

    public void Clock()
    {
        if (period != 0)
        {
            period--;
            return;
        }
        
        period = PeriodReload;
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
 
    public byte divider;
   
    public ushort TargetPeriod
    {
        get
        {
            ushort change = (ushort) (PeriodReload >> SweepShift);

            if (!SweepNegate)
                return (ushort) (PeriodReload + change);

            if (!OnesComplement)
                return (ushort) (PeriodReload - change);

            if (SweepShift == 0 || PeriodReload == 0)
                return 0 ;

            return (ushort) (PeriodReload - change - 1);
        }
    }

    public void UpdateSweep()
    {
        ushort target = TargetPeriod;

        if (SweepEnabled
            && divider == 0
            && SweepShift != 0
            && target <= 0x7FF
            && PeriodReload >= 8)
            PeriodReload = target;

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