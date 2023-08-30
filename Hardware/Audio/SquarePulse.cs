using static System.Math;

namespace Hardware.Audio;

public class SquarePulse : ISampler
{
    public double Frequency { get; set; }
    public double DutyCycle { get; set; }
    public double Amplitude { get; set; }
    public double Harmonics { get; set; } = 20;
    
    public double Sample(double time)
    {
        return SquareWave(Amplitude, Frequency, Harmonics, DutyCycle, time);
    }
    
    private static double SquareWave(double amplitude, double frequency, double harmonics, double dutyCycle, double time)
    {
        double y1 = 0;
        double y2 = 0;
        double p = dutyCycle * 2 * PI;

        for (int n = 1; n < harmonics; n++)
        {
            double c = n * frequency * 2 * PI * time;
            y1 += -FastSin(c) / n;
            y2 += -FastSin(c - p * n) / n;
        }

        return 2.0 * amplitude / PI * (y1 - y2);
    }
    
    private static double FastSin(double x)
    {
        double j = x * 0.15915;
        j = j - (int) j;
        return 20.785 * j * (j - 0.5) * (j - 1.0);
    }
}