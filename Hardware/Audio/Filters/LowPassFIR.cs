using System;
using System.Collections.Generic;
using System.Linq;

namespace Hardware.Audio.Filters;

public class LowPassFIR : IFilter
{
    private int WindowSize { get; }
    //private Queue<double> Samples { get; }
    private double[] Samples { get; }
    private int samplesIndex = 0;
    private double[] Coefficients { get; }

    public LowPassFIR(double sampleRate, double cutoff, int windowSize)
    {
        WindowSize = windowSize;
        Samples = new double[windowSize];
        Coefficients = new double[windowSize];

        double normalizedCutoff = cutoff / sampleRate;

        for (int i = 0; i < windowSize; i++)
        {
            Coefficients[i] = Sinc(i, normalizedCutoff, windowSize);

            Coefficients[i] *= Hanning(i, windowSize);
        }
    }

    private static double Hanning(int index, int windowSize)
    {
        return 0.5 * (1 - Math.Cos(2 * Math.PI * index / windowSize));
    }

    private static double Sinc(int index, double normalizedCutoff, int windowSize)
    {
        if (index == windowSize / 2)
            return 2 * Math.PI * normalizedCutoff;

        return Math.Sin(2 * Math.PI * normalizedCutoff * (index - windowSize / 2.0)) /
               (index - windowSize / 2.0);
    }

    public void Process(double sample)
    {
        Samples[samplesIndex] = sample;
        samplesIndex = (samplesIndex + 1) % Samples.Length;
    }

    public double Output()
    {
        double output = 0;
        for (var i = 0; i < WindowSize; i++)
        {
            int index = (samplesIndex + i) % Samples.Length;
            output += Coefficients[i] * Samples[index];
        }
        return output;
    }
}