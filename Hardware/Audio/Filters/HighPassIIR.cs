namespace Hardware.Audio.Filters;

public class HighPassIIR : IFilter
{
    private double Alpha { get; }
    private double Delta { get; set; }
    private double PreviousInput { get; set; }
    private double PreviousOutput { get; set; }

    public HighPassIIR(double sampleRate, double cutoff)
    {
        double dt = 1.0 / sampleRate;
        double rc = 1.0 / cutoff;
        Alpha = rc / (rc + dt);
    }
    
    public void Process(double sample)
    {
        PreviousOutput = Output();
        Delta = sample - PreviousInput;
        PreviousInput = sample;
    }

    public double Output()
    {
        return Alpha * PreviousOutput + Alpha * Delta;
    }
}