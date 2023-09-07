using static System.Math;

namespace Hardware.Audio.Filters;

public class LowPassIIR : IFilter
{
    private double Alpha { get; }

    private double PreviousOutput { get; set; }

    private double Delta { get; set; }

    public LowPassIIR(double sampleRate, double cutoff)
    {
        double dt = 1.0 / sampleRate;
        double rc = 1.0 / (cutoff * 2 * PI);
        Alpha = dt / (rc + dt);
    }

    public void Process(double sample)
    {
        PreviousOutput = Output();
        Delta = sample - PreviousOutput;
    }

    public double Output()
    {
        return PreviousOutput + Alpha * Delta;
    }
}