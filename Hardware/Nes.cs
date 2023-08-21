using System;
using Microsoft.Xna.Framework;

namespace Hardware;

public class Nes : IResetable, IInsertable, IPixelBuffer
{
    private PpuBus PpuBus { get; }
    public Ppu Ppu { get; }
    public CpuBus CpuBus { get; }
    public Cpu Cpu { get; }
    private Cartridge? Cartridge { get; set; }

    public byte[] Controllers => CpuBus.controllers;
    
    public Nes()
    {
        PpuBus = new PpuBus();
        Ppu = new Ppu(PpuBus, this);
        CpuBus = new CpuBus(Ppu);
        screen = new uint[Width * Height];
        Cpu = new Cpu(CpuBus);
    }
    
    public const int Width = 256;
    public const int Height = 240;

    private Memory<uint> screen;

    public uint[] Pixels => screen.ToArray();

    public void SetPixel(ushort x, ushort y, uint colour)
    {
        screen.Span[y * Width + x] = colour;
    }

    private uint systemClock = 0;

    private double residualTime = 0;

    
    
    public void Update(GameTime gameTime, bool pause)
    {
        if (Cartridge == null)
            return;

        if (pause)
            return;
        
        while (!Ppu.FrameComplete)
        {
            Clock();
        }

        Ppu.FrameComplete = false;
    }

    private void Clock()
    {
        Ppu.Cycle();
    
        if (systemClock % 3 == 0)
        {
            HandleCPU();
        }

        if (Ppu.NonMaskableInterrupt)
        {
            Ppu.ResetNonMaskableInterrupt();
            Cpu.TriggerNonMaskableInterrupt();
        }

        systemClock++;
    }

    private void HandleCPU()
    {
        if (!CpuBus.DmaTransfer)
        {
            Cpu.Cycle();
            return;
        }

        if (CpuBus.DmaDummy && systemClock % 2 == 1)
        {
            CpuBus.DmaDummy = false;
            return;
        }

        if (systemClock % 2 == 0)
        {
            CpuBus.DmaData = CpuBus.Read((ushort) (CpuBus.DmaPage << 8 | CpuBus.DmaAddress));
            return;
        }

        Ppu.SetOamByte(CpuBus.DmaAddress, CpuBus.DmaData);
        CpuBus.DmaAddress++;

        if (CpuBus.DmaAddress != 0x00)
            return;

        CpuBus.DmaTransfer = false;
        CpuBus.DmaDummy = true;
    }

    public void Insert(Cartridge cart)
    {
        Cartridge = cart;
        PpuBus.Insert(cart);
        CpuBus.Insert(cart);
        Reset();
    }

    public void Reset()
    {
        systemClock = 0;
        Ppu.Reset();
        Cpu.Reset();
    }
}