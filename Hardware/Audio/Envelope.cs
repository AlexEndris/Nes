namespace Hardware.Audio;

public class Envelope
{
    private byte volume;
    private byte divider;
    private byte decay;

    public bool Start { get; set; }
    public bool ConstantVolume { get; set; }
    public bool Loop { get; set; }

    public byte Volume
    {
        get
        {
            if (ConstantVolume)
                return volume;

            return decay;
        }
        set => volume = value;
    }
   
    public void Clock()
    {
        if (Start)
        {
            Start = false;
            decay = 15;
            divider = volume;
            return;
        }

        if (divider != 0)
        {
            divider--;
            return;
        }

        divider = volume;

        if (decay > 0)
        {
            decay--;
            return;
        }

        if (!Loop) 
            return;
        
        decay = 15;
    }
}