namespace Hardware.Headers;

public enum TimingMode
{
    RP2C02 = 0, // NTSC NES
    RP2C07 = 1, // PAL NES
    MultiRegion = 2,
    UA6538 = 3 // Dendy
}