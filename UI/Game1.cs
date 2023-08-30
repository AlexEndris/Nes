using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Hardware;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.Double;

namespace UI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Nes nes;

        private DynamicSoundEffectInstance sound;

        private Texture2D nesScreen;

        private Texture2D pattern1;
        private Texture2D pattern2;
        private Texture2D oam1;

        private Texture2D palette1;
        private Texture2D palette2;
        private Texture2D palette3;
        private Texture2D palette4;
        private TimeSpan _timePerFrame;

        public static double FastSin(double x)
        {
            double j = x * 0.15915;
            j = j - (int) j;
            return 20.785 * j * (j - 0.5) * (j - 1.0);
        }

        private double SquareWave(double amplitude, double frequency, double harmonics, double dutyCycle, double time)
        {
            double y1 = 0;
            double y2 = 0;
            double p = dutyCycle * 2 * Pi;

            for (int n = 1; n < harmonics; n++)
            {
                double c = n * frequency * 2 * Pi * time;
                y1 += -FastSin(c) / n;
                y2 += -FastSin(c - p * n) / n;
            }

            return (2.0 * amplitude / Pi) * (y1 - y2);
        }

        private double TriangleWave(double amplitude, double frequency, double time)
        {
            return Math.Abs(2 * (time * frequency - Math.Floor(time * frequency + 0.5))) * amplitude * 2 - amplitude;
        }
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            //IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromTicks((long) (TimeSpan.TicksPerSecond / 60.1));
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            nes = new Nes();

            gameTime = new GameTime();

            buffer = new byte[1024 * 2];
            workingBuffer = new float[1024];
            
            sound = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);
            //sound.BufferNeeded += SoundOnBufferNeeded;
            sound.Play();

            nesScreen = new Texture2D(GraphicsDevice, Nes.Width, Nes.Height);
            pattern1 = new Texture2D(GraphicsDevice, 256, 256);
            pattern2 = new Texture2D(GraphicsDevice, 256, 256);
            oam1 = new Texture2D(GraphicsDevice, 64, 64);
            palette1 = new Texture2D(GraphicsDevice, 4, 1);
            palette2 = new Texture2D(GraphicsDevice, 4, 1);
            palette3 = new Texture2D(GraphicsDevice, 4, 1);
            palette4 = new Texture2D(GraphicsDevice, 4, 1);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("fonts/Cascadia");

            Cartridge cart;
            cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\nestest.nes");
            cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\mario.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\donkey.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\icarus.nes");
            //cart = TestRoms();

            nes.Insert(cart);
        }

        private static Cartridge TestRoms()
        {
            Cartridge cart;
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\01-basics.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\02-implied.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\03-immediate.nes");
            cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\04-zero_page.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\05-zp_xy.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\06-absolute.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\07-abs_xy.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\08-ind_x.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\09-ind_y.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\10-branches.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\11-stack.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\12-jmp_jsr.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\13-rts.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\14-rti.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\15-brk.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\rom_singles\16-special.nes");
            return cart;
        }

        private bool pause = false;
        private double _framesPerSecond;
        private KeyboardState previousState;
        private KeyboardState currentState;
        private bool advanceFrame;
        private bool advanceScanline;
        private bool advanceCycle;
        private GameTime gameTime;

        private double time = 0;

        protected override void Update(GameTime gameTime)
        {
            currentState = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                currentState.IsKeyDown(Keys.Escape))
                Exit();

            this.gameTime = gameTime;

            // while (sound.PendingBufferCount < 2)
            //     SubmitBuffer();

            EmulationInput();
            UpdateInput();
            
            var start = DateTime.Now;
            
            nes.Update(pause, advanceFrame, advanceScanline, advanceCycle);
            // update nes screen
            nesScreen.SetData(nes.Pixels);
            
            var end = DateTime.Now;
            _timePerFrame = (end - start);
            _framesPerSecond = 1.0 / _timePerFrame.TotalSeconds;
            var frameTime = _timePerFrame.TotalMilliseconds;
            
            UpdateDebug();
            
            previousState = Keyboard.GetState();
            advanceScanline = false;
            advanceFrame = false;
            advanceCycle = false;
            base.Update(gameTime);
        }

        private float[] workingBuffer;
        private byte[] buffer;
    

        private void SubmitBuffer()
        {
            FillWorkBuffer();
            ConvertBuffer(workingBuffer, buffer);
            sound.SubmitBuffer(buffer);
        }

        private void ConvertBuffer(float[] from, byte[] to)
        {
            const int bytesPerSample = 2;
            int bufferSize = from.Length;

            for (int i = 0; i < bufferSize; i++)
            {
                float floatSample = Math.Clamp(from[i], -1.0f, 1.0f);

                short shortSample =
                    (short) (floatSample >= 0 ? floatSample * short.MaxValue : floatSample * short.MinValue * -1);

                int index = i * bytesPerSample;

                if (!BitConverter.IsLittleEndian)
                {
                    to[index] = (byte)(shortSample >> 8);
                    to[index + 1] = (byte)shortSample;
                }
                else
                {
                    to[index] = (byte)shortSample;
                    to[index + 1] = (byte)(shortSample >> 8);
                }
            }
        }
        
        private void FillWorkBuffer()
        {
            for (int i = 0; i < 1024; i++)
            {
                //workingBuffer[i] = (float) (Math.Sin(440 * time * 2 * Pi) * 0.2);
                //workingBuffer[i] = (float) (SquareWave(.1, 440, 20, .5, time));
                workingBuffer[i] = (float) (TriangleWave(.1, 110, time));

                time += 1.0 / 44100;
            }
        }

        private void EmulationInput()
        {
            if (IsKeyPressed(Keys.Space))
                pause = !pause;

            if (IsKeyPressed(Keys.Back))
                nes.Reset();

            if (pause
                && currentState.IsKeyDown(Keys.Enter))
                advanceFrame = true;

            if (pause
                && currentState.IsKeyDown(Keys.Add))
                advanceScanline = true;
            
            if (pause
                && currentState.IsKeyDown(Keys.Subtract))
                advanceCycle = true;
        }

        private bool IsKeyPressed(Keys key)
        {
            return currentState.IsKeyUp(key)
                   && previousState.IsKeyDown(key);
        }

        private void UpdateDebug()
        {
            // update pattern
            var data = nes.Ppu.GetOamTable(0);
            oam1.SetData(data);
            data = nes.Ppu.GetPatternTable(0, 0);
            pattern1.SetData(data);
            data = nes.Ppu.GetPatternTable(1, 0);
            pattern2.SetData(data);

            // update palette
            data = nes.Ppu.GetPalette(0);
            palette1.SetData(data);
            data = nes.Ppu.GetPalette(1);
            palette2.SetData(data);
            data = nes.Ppu.GetPalette(2);
            palette3.SetData(data);
            data = nes.Ppu.GetPalette(3);
            palette4.SetData(data);
        }

        private void UpdateInput()
        {
            var keyboardState = Keyboard.GetState();
            nes.Controllers[0] = 0x0;
            nes.Controllers[0] |= (byte)(keyboardState.IsKeyDown(Keys.A) ? 0x80 : 0x0);
            nes.Controllers[0] |= (byte)(keyboardState.IsKeyDown(Keys.R) ? 0x40 : 0x0);
            nes.Controllers[0] |= (byte)(keyboardState.IsKeyDown(Keys.Z) ? 0x20 : 0x0);
            nes.Controllers[0] |= (byte)(keyboardState.IsKeyDown(Keys.X) ? 0x10 : 0x0);
            nes.Controllers[0] |= (byte)(keyboardState.IsKeyDown(Keys.Up) ? 0x08 : 0x0);
            nes.Controllers[0] |= (byte)(keyboardState.IsKeyDown(Keys.Down) ? 0x04 : 0x0);
            nes.Controllers[0] |= (byte)(keyboardState.IsKeyDown(Keys.Left) ? 0x02 : 0x0);
            nes.Controllers[0] |= (byte)(keyboardState.IsKeyDown(Keys.Right) ? 0x01 : 0x0);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // NES Screen
            _spriteBatch.Draw(nesScreen, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 1);
            
            DrawDebug();

            _spriteBatch.End();
            
            base.Draw(gameTime);
        }

        private void DrawDebug()
        {
            var offset=  DrawCpu();
            DrawText(offset);
            //DrawTemp(offset);
            DrawPpu();
        }

        private void DrawText(int offsetY)
        {
            var screenOffsetX = nesScreen.Width * 4 + 5;
            var textHeight = (int) _font.MeasureString("RAM").Y;
            offsetY += textHeight;

            _spriteBatch.DrawString(_font, $"Cycle: {nes.Ppu.Cycle,-3} -- Scanline: {nes.Ppu.Scanline,-3} -- Frames: {nes.Ppu.FrameCount:N0}",
                new Vector2(screenOffsetX, offsetY), Color.White);            
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"Pause: {pause}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"VRAM: {nes.Ppu.VRam}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"NametableX: {nes.Ppu.VRam.NametableX} - NametableY: {nes.Ppu.VRam.NametableY}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"CoarseX: {nes.Ppu.VRam.CoarseX}, FineX: {nes.Ppu.FineX} - CoarseY: {nes.Ppu.VRam.CoarseY}, FineY: {nes.Ppu.VRam.FineY}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"TRAM: {nes.Ppu.TRam}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"NametableX: {nes.Ppu.TRam.NametableX} - NametableY: {nes.Ppu.TRam.NametableY}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"CoarseX: {nes.Ppu.TRam.CoarseX}, FineX: -- - CoarseY: {nes.Ppu.TRam.CoarseY}, FineY: {nes.Ppu.TRam.FineY}",
                new Vector2(screenOffsetX, offsetY), Color.White);
        }

        private int DrawCpu()
        {
            var screenOffsetX = nesScreen.Width * 4 + 5;
            var offsetY = 0;
            var textHeight = (int)_font.MeasureString("Status").Y;
            // Status
            DrawStatus(screenOffsetX, offsetY);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"PC: ${nes.Cpu.PC:X4}", new Vector2(screenOffsetX, offsetY),
                Color.White);            
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"A: ${nes.Cpu.A:X2} [{nes.Cpu.A}]", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"X: ${nes.Cpu.X:X2} [{nes.Cpu.X}]", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"Y: ${nes.Cpu.Y:X2} [{nes.Cpu.Y}]", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"Stack: ${nes.Cpu.SP:X2} [{nes.Cpu.SP}]", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"NES Frame Time: {_timePerFrame.TotalMilliseconds:00.0}ms -- FPS: {_framesPerSecond:000}fps", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"Cycles: CPU: {nes.Cpu.CycleCount,-13:N0}-- PPU: {nes.Ppu.CycleCount:N0}", new Vector2(screenOffsetX, offsetY),
                Color.White);

            return offsetY;
        }

        private void DrawStatus(int screenOffsetX, int offsetY)
        {
            var status = nes.Cpu.Status;
            var offsetX = 0;
            _spriteBatch.DrawString(_font, "Status: ", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetX += (int) _font.MeasureString("Status: ").X;
            _spriteBatch.DrawString(_font, "N ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.Negative)? Color.Green : Color.DarkRed);
            var twoLetters = (int) _font.MeasureString("V ").X;
            offsetX += twoLetters;
            _spriteBatch.DrawString(_font, "V ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.Overflow)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            _spriteBatch.DrawString(_font, "- ", new Vector2(screenOffsetX+offsetX, offsetY), Color.Gray);
            offsetX += twoLetters;
            _spriteBatch.DrawString(_font, "B ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.BreakCommand)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            _spriteBatch.DrawString(_font, "D ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.DecimalMode)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            _spriteBatch.DrawString(_font, "I ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.InterruptDisable)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            _spriteBatch.DrawString(_font, "Z ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.Zero)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            _spriteBatch.DrawString(_font, "C ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.Carry)? Color.Green : Color.DarkRed);
        }

        private void DrawPpu()
        {
            var screenOffsetX = nesScreen.Width * 4 + 5;
            var screenOffsetY = nesScreen.Height * 4 - 277;
            var patternOffsetX = pattern1.Width + 5;
            var patternOffsetY = palette1.Height * 16 + 5;

            // Palette
            _spriteBatch.Draw(palette1, new Vector2(screenOffsetX, screenOffsetY), null,
                Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 1);

            _spriteBatch.Draw(palette2, new Vector2(screenOffsetX + 1 * 4 * 16 + 24, screenOffsetY), null,
                Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 1);

            _spriteBatch.Draw(palette3, new Vector2(screenOffsetX + 2 * 4 * 16 + 2 * 24, screenOffsetY), null,
                Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 1);

            _spriteBatch.Draw(palette4, new Vector2(screenOffsetX + 3 * 4 * 16 + 3 * 24, screenOffsetY), null,
                Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 1);


            // PatternTable
            _spriteBatch.Draw(pattern1, new Vector2(screenOffsetX, screenOffsetY + patternOffsetY), null,
                Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 1);

            _spriteBatch.Draw(pattern2, new Vector2(screenOffsetX + patternOffsetX, screenOffsetY + patternOffsetY), null,
                Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 1);
            
            _spriteBatch.Draw(oam1, new Vector2(screenOffsetX + patternOffsetX*2, screenOffsetY + patternOffsetY), null,
                Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 1);
        }
    }
}