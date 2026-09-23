namespace Hardware.Audio;

using System;

public class Dmc
{
    private ushort[] rateLookupNtsc =
    {
        428, 380, 340, 320, 286, 254, 226, 214, 190, 160, 142, 128, 106, 84, 72, 54
    };

    private ushort[] rateLookupPal =
    {
        398, 354, 316, 298, 276, 236, 210, 198, 176, 148, 132, 118, 98, 78, 66, 50
    };

    public Timer Timer { get; } = new();

    public bool IrqEnabled { get; set; }

    public bool Irq { get; set; }

    public ushort SampleAddress { get; set; }

    public ushort SampleLength { get; set; }

    public bool Loop { get; set; }

    private byte SampleBuffer { get; set; }

    private bool SampleBufferEmpty { get; set; } = true;

    // Reader
    private ushort CurrentAddress { get; set; }

    private ushort BytesRemaining { get; set; }

    // Output
    public byte OutputLevel { get; set; }

    private bool Silence { get; set; }

    private byte Shift { get; set; }

    private byte BitsRemaining { get; set; } = 8;

    public void SetState(bool enabled)
    {
        if (!enabled)
        {
            BytesRemaining = 0;
            return;
        }

        if (BytesRemaining == 0)
        {
            ResetSample();
        }
    }

    public void SetRate(byte value)
    {
        Timer.PeriodReload = (ushort)(rateLookupNtsc[value] - 1);
    }
    
    private void ResetSample()
    {
        CurrentAddress = SampleAddress;
        BytesRemaining = SampleLength;
    }

    public void Clock(Func<ushort, byte> reader)
    {
        ProcessOutput();
        ProcessReader(reader);
    }

    private void ProcessOutput()
    {
        if (!Timer.Clock())
        {
            return;
        }

        if (!Silence)
        {
            if ((Shift & 0x1) > 0)
            {
                if (OutputLevel <= 125)
                {
                    OutputLevel += 2;
                }
            }
            else
            {
                if (OutputLevel >= 2)
                {
                    OutputLevel -= 2;
                }
            }
        }

        Shift >>= 1;
        BitsRemaining--;

        if (BitsRemaining != 0)
        {
            return;
        }
        
        BitsRemaining = 8;

        if (SampleBufferEmpty)
        {
            Silence = true;
        }
        else
        {
            Silence = false;
            Shift = SampleBuffer;
            SampleBufferEmpty = true;
        }
    }

    private void ProcessReader(Func<ushort, byte> reader)
    {
        if (!SampleBufferEmpty 
            || BytesRemaining == 0)
        {
            return;
        }
        
        SampleBuffer = reader(CurrentAddress);
        SampleBufferEmpty = false;

        if (CurrentAddress == 0xFFFF)
        {
            CurrentAddress = 0x8000;
        }
        else
        {
            CurrentAddress++;
        }

        BytesRemaining--;

        if (BytesRemaining != 0)
        {
            return;
        }
        
        if (Loop)
        {
            ResetSample();
        }
        else if (IrqEnabled)
        {
            Irq = true;
        }
    }

    public ushort GetSample()
    {
        return OutputLevel;
    }
}
