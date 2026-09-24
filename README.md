# Nes
Some dabbling with C# and an NES Emulator

The solution is set up with two projects. One for the main emulation code, the other mainly for displaying something on the screen using MonoGame

## Next Steps
No Idea. About 70% of commercial games should run. Maybe make a proper window host to load the roms?

## Known Bugs
Some little flicker from IRQ timing in MMC3 games.

## Accuracy
100thCoin provided a nice ROM to check the accuracy here on Github [100thCoin/AccuracyCoin](https://github.com/100thCoin/AccuracyCoin)

Here's my result (Test 19.4 is skipped because it affects other tests for me currently):
![Accuracy Table](AccuracyCoin.png)
