using System;
using System.Collections.Generic;
using System.Linq;
using Hardware.Audio;
using SharpDX.Direct2D1.Effects;

namespace Hardware;

public class Apu
{
    public SquarePulse[] Pulse { get; } = {new(), new()};
    public Triangle Triangle { get; } = new();
    public Noise Noise { get; } = new();

    private double[] pulseLookup;
    private double[] tndLookup;
    

    private FrameCounter frameCounter = new();
    private uint cycle;

    public Apu()
    {
        var table = new List<double>(31);
        for (int i = 1; i <= 31; i++)
            table.Add(95.52/(8128.0 / i +100));
        pulseLookup = table.ToArray();

        table = new List<double>(203);
        for (int i = 1; i <= 203; i++)
            table.Add(163.67 / (24329.0 / i + 100));
        tndLookup = table.ToArray();

        CreateHannWindow(50);
        
        // Pulse 1 is wired differently
        Pulse[0].OnesComplement = true;
    }
    
    public byte CpuRead(ushort address)
    {
        if (address != 0x4015) 
            return 0;
        
        byte status = 0;

        if (Pulse[0].Counter.Value > 0)
            status += 0b0000_0001; 
        if (Pulse[1].Counter.Value > 0)
            status += 0b0000_0010; 
        // if (Triangle.Counter.Value > 0)
        //     status += 0b0000_0100; 
        // if (Noise.Counter.Value > 0)
        //     status += 0b0000_1000; 
        // if (Dmc.Remaining > 0)
        //     status += 0b0001_0000; 
            
        if (frameCounter.Interrupt)
            status += 0b0100_0000;
        
        // if (Dmc.Interrupt)
        //     status += 0b1000_0000;
        
        
        return status;
    }
    
    public void CpuWrite(ushort address, byte value)
    {
        switch (address)
        {
            case <= 0x4003:
                WritePulse(0, (ushort)(address & 0x3), value);
                break;
            case <= 0x4007:
                WritePulse(1, (ushort)(address & 0x3), value);
                break;
            case <= 0x400B:
                WriteTriangle((ushort) (address & 0x3), value);
                break;
            case <= 0x400F:
                WriteNoise((ushort) (address & 0x3), value);
                break;
            case <= 0x4013:
                break;
            case 0x4015:
                Pulse[0].Counter.Enabled = (value & 0b0001) != 0;
                Pulse[1].Counter.Enabled = (value & 0b0010) != 0;
                Triangle.Counter.Enabled = (value & 0b0100) != 0;
                Noise.Counter.Enabled = (value & 0b1000) != 0;
                //Dmc.Enabled = (value & 0b1_0000) != 0;
                // TODO some other DMC related stuff
                //Dmc.Interrupt = false;
                break;
            case 0x4017:
                frameCounter.FiveStepMode = (value & 0x80) > 0;
                frameCounter.DisableInterrupt = (value & 0x40) > 0;
                frameCounter.ResetDelay = (byte)((cycle & 0x1) != 0 ? 3 : 4); 
                break;
        }
    }

    private void WriteNoise(ushort address, byte value)
    {
        switch (address)
        {
            case 0:
                bool halt = (value & 0x20) != 0;
                bool constant = (value & 0x10) != 0;
                byte volume = (byte) (value & 0xF);

                Noise.Counter.Halt = halt;
                Noise.Envelope.Loop = halt;
                Noise.Envelope.ConstantVolume = constant;
                Noise.Envelope.Volume = volume;
                Noise.Envelope.Start = true;
                break;
            case 1:
                // Unused
                break;
            case 2:
                bool mode = (value & 0x80) != 0;
                byte period = (byte) (value & 0xF);

                Noise.Mode = mode;
                Noise.Load(period);
                break;
            case 3:
                Noise.Counter.Load((byte) (value >> 3));
                break;
        }
    }

    private void WriteTriangle(ushort address, byte value)
    {
        switch (address)
        {
            case 0:
                bool halt = (value & 0x80) != 0;
                byte linearCounter = (byte) (value & 0x7F);

                Triangle.Counter.Halt = halt;
                Triangle.Linear.Control = halt;
                Triangle.Linear.ReloadValue = linearCounter;
                break;
            case 1:
                // Unused
                break;
            case 2:
                Triangle.PeriodReload = (ushort) ((Triangle.PeriodReload & 0x700) | value);
                break;
            case 3:
                Triangle.PeriodReload = (ushort) ((Triangle.PeriodReload & 0xFF) | ((value & 0x7) << 8));
                Triangle.Counter.Load((byte) (value >> 3));

                Triangle.Linear.Reload = true;
                break;
        }
    }

    private void WritePulse(int i, ushort address, byte value)
    {
        var pulse = Pulse[i];

        switch (address)
        {
            case 0:
            {
                byte duty = (byte) (value >> 6);
                bool halt = (value & 0x20) > 0;
                bool constant = (value & 0x10) > 0;
                byte volume = (byte) (value & 0xF);

                pulse.DutyCycleIndex = duty;
                pulse.Envelope.Loop = halt;
                pulse.Counter.Halt = halt;
                pulse.Envelope.ConstantVolume = constant;
                pulse.Envelope.Volume = volume;
                pulse.Envelope.Start = true;
                break;
            }
            case 1:
                bool enabled = (value & 0x80) != 0;
                byte dividerPeriod = (byte) ((value >> 4) & 0x7);
                bool negate = (value & 0x8) != 0;
                byte shift = (byte) (value & 0x7);

                pulse.SweepEnabled = enabled;
                pulse.SweepDividerPeriod = dividerPeriod;
                pulse.SweepNegate = negate;
                pulse.SweepShift = shift;
                
                pulse.SweepReload = true;
                break;
            case 2:
                pulse.PeriodReload = (ushort) ((pulse.PeriodReload & 0x700) | value);
                break;
            case 3:
                pulse.PeriodReload = (ushort) ((pulse.PeriodReload & 0xFF) | ((value & 0x7) << 8));
                pulse.Counter.Load((byte) (value >> 3));
                break;
        }
    }

    public void Clock()
    {
        frameCounter.Clock(QuarterFrame, HalfFrame);
        // TODO clock channels
        
        if ((cycle & 0x1) == 0)
        {
            Pulse[0].Clock();
            Pulse[1].Clock();
        }

        Triangle.Clock();
        Noise.Clock();
        
        double output = MixSamples();
        Downsample(output);
        
        cycle++;
    }

    private void Downsample(double sample)
    {
        rawSampleBuffer.Add(sample);
        if (cycle < nextSampleAt)
            return;

        ApplyHann();
        
        var downSampled = rawSampleBuffer.Average();
        var filtered = highPass(downSampled, 37.0);
        
        rawSampleBuffer.Clear();
        sampleBuffer.Add(filtered);
        nextSampleAt = (uint) ((generatedSamples + 1) * ((float)cpuClock / sampleRate));
        generatedSamples++;
    }

    private double[] hannWindow;

    private void CreateHannWindow(int size)
    {
        hannWindow = new double[size];
        for(int i = 0; i < size; i++)
            hannWindow[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (size - 1)));
    }
    
    private void ApplyHann()
    {
        for (int i = 0; i < rawSampleBuffer.Count; i++)
            rawSampleBuffer[i] *= hannWindow[i];
    }

    private List<double> rawSampleBuffer = new(50);
    private List<double> sampleBuffer = new(800);
    
    public bool HasSamples()
    {
        return sampleBuffer.Count >= 64;
    }

    private uint cpuClock = 1789773;
    private uint sampleRate = 48000;
    private uint nextSampleAt = 0;
    private uint generatedSamples = 0;
    
    public double[] GetOutputBuffer()
    {
        var buffer = sampleBuffer.ToArray();

        sampleBuffer.Clear();
        
        return buffer;
    }
    
    private double MixSamples()
    {
        ushort pulse1 = Pulse[0].GetSample();
        ushort pulse2 = Pulse[1].GetSample();
        ushort triangle = Triangle.GetSample(); 
        ushort noise = Noise.GetSample(); 
        ushort dmc = 0;

        int combinedPulse = pulse1 + pulse2;
        double pulseOut = 0.00752 * combinedPulse;
        double tndOut = 0.00851 * triangle + 0.00494 * noise + 0.00335 * dmc;

        double output = pulseOut + tndOut;

        return output;
    }

    private double lastSample = 0;
    private double highPass(double sample, double cutoff)
    {
        double dt = 1.0 / sampleRate;
        double rc = 1.0 / cutoff;
        double alpha = rc / (rc + dt);
        double filteredSample = alpha * (lastSample + sample - lastSample);
        lastSample = filteredSample;
        return filteredSample;
    }

    private double lowPassSample = 0;
    private double lowPass(double sample, double cutoff){
        double dt = 1.0 / sampleRate;
        double rc = 1.0 / (cutoff * 2 * Math.PI);
        double alpha = dt / (rc + dt);
        lowPassSample += (sample - lowPassSample) * alpha;
        return lowPassSample;
    }
    
    private void QuarterFrame()
    {
        Pulse[0].Envelope.Clock();
        Pulse[1].Envelope.Clock();
        Triangle.Linear.Clock();
        Noise.Envelope.Clock();
    }

    private void HalfFrame()
    {
        Pulse[0].UpdateSweep();
        Pulse[1].UpdateSweep();
        
        Pulse[0].Counter.Clock();
        Pulse[1].Counter.Clock();
        Triangle.Counter.Clock();
        Noise.Counter.Clock();
    }
}