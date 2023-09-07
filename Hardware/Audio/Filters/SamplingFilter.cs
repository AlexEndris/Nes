namespace Hardware.Audio.Filters;

public class SamplingFilter : IFilter
{
    public IFilter Filter { get; }
    public double SamplingPeriod { get; }

    public double SamplingCounter { get; set; }

    public SamplingFilter(IFilter filter, double samplingPeriod)
    {
        Filter = filter;
        SamplingPeriod = samplingPeriod;
    }
    public void Process(double sample) => Filter.Process(sample);
    public double Output() => Filter.Output();
}