namespace Hardware.Audio.Filters;

public interface IFilter
{
    public void Process(double sample);
    public double Output();
}