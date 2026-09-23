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
    public bool Enabled { get; set; }
    public bool IrqEnabled { get; set; }
    public bool Irq { get; set; }
    
    public ushort SampleAddress { get; set; }
    public ushort SampleLength { get; set; }
    public byte RateIndex { get; set; }
    public byte OutputLevel { get; set; }
    
    private byte SampleBuffer { get; set; }
    private bool Silence { get; set; }
    
    
    
    public void Clock(Func<ushort, byte> reader)
    {
        if (SampleBuffer == 0)
        {
            SampleBuffer = reader(SampleAddress);
        }
    }

    public ushort GetSample()
    {
        return 0;
    }
}