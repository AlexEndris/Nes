namespace Hardware.Audio;

public class LinearCounter
{
    public byte Value { get; private set; }
    public bool Reload { get; set; }
    public byte ReloadValue { get; set; }
    public bool Control { get; set; }

    public void Clock()
    {
        if (Reload)
        {
            Value = ReloadValue;
        }
        else if (Value > 0)
        {
            Value--;
        }

        if (Control)
            return;

        Reload = false;
    }
}