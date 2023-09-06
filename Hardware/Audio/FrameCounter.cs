using System;

namespace Hardware.Audio;

public class FrameCounter
{
    private uint counter;
    public bool FiveStepMode { get; set; }
    public bool DisableInterrupt { get; set; }
    public bool Interrupt { get; set; }
    public byte ResetDelay { get; set; }

    public void Clock(Action quarterFrame, Action halfFrame)
    {
        ProcessReset(quarterFrame, halfFrame);
        
        if (FiveStepMode)
            FiveStep(quarterFrame, halfFrame);
        else
            FourStep(quarterFrame, halfFrame);
        
        counter++;
    }

    private void ProcessReset(Action quarterFrame, Action halfFrame)
    {
        if (ResetDelay == 0)
            return;

        ResetDelay--;
        if (ResetDelay != 0)
            return;

        counter = 0;

        if (!FiveStepMode) 
            return;
        
        quarterFrame();
        halfFrame();
    }

    private void FourStep(Action quarterFrame, Action halfFrame)
    {
        bool interrupt = false;
        switch (counter)
        {
            case 7457:
                quarterFrame();
                break;
            case 14913:
                quarterFrame();
                halfFrame();
                break;
            case 22371:
                quarterFrame();
                break;
            case 29828:
                Interrupt = !DisableInterrupt;
                break;
            case 29829:
                Interrupt = !DisableInterrupt;
                quarterFrame();
                halfFrame();
                break;
            case 29830:
                Interrupt = !DisableInterrupt;
                counter = 0;
                break;
        }
    }

    private void FiveStep(Action quarterFrame, Action halfFrame)
    {
        switch (counter)
        {
            case 7457:
                quarterFrame();
                break;
            case 14913:
                quarterFrame();
                halfFrame();
                break;
            case 22371:
                quarterFrame();
                break;
            case 37281:
                quarterFrame();
                halfFrame();
                break;
            case 37282:
                counter = 0;
                break;
        }
    }
}