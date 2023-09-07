namespace Hardware.Audio.Filters;

public class IdentityFilter : IFilter
{
    private double Sample { get; set; }
    public void Process(double sample)
    {
        Sample = sample;
    }

    public double Output()
    {
        return Sample;
    }
}