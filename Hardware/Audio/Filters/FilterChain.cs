using System.Collections.Generic;
using System.Linq;

namespace Hardware.Audio.Filters;

public class FilterChain
{
    private List<SamplingFilter> Filters { get; } = new();

    private double DeltaTime { get; }
    
    public FilterChain(double clockRate)
    {
        DeltaTime = 1.0 / clockRate;
    }

    public void Add(IFilter filter, double sampleRate)
    {
        Filters.Add(new SamplingFilter(filter, 1.0 / sampleRate));
    }
    
    public double Process(double sample)
    {
        var previousSample = sample;
        foreach (var filter in Filters)
        {
            previousSample = Process(previousSample, filter);
        }
        return previousSample;
    }

    private double Process(double previous, SamplingFilter filter)
    {
        filter.SamplingCounter += DeltaTime;
        while (filter.SamplingCounter >= filter.SamplingPeriod)
        {
            filter.SamplingCounter -= filter.SamplingPeriod;

            filter.Process(previous);
        }
        
        return filter.Output();
    }
}