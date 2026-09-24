using System;

using Hardware;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UI
{
    public class Emulator : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
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
        private TimeSpan timePerFrame;

        public Emulator()
        {
            graphics = new GraphicsDeviceManager(this);
            TargetElapsedTime = TimeSpan.FromTicks((long) (TimeSpan.TicksPerSecond / 60.0988118623484));
            Content.RootDirectory = "Content";
            IsDebugEnabled = false;
        }

        public bool IsDebugEnabled { get; set; }

        protected override void Initialize()
        {
            nes = new Nes();

            gameTime = new GameTime();

            InitializeSound();
            InitializeTextures();
            InitialiseWindow();
            
            base.Initialize();
        }

        private void InitialiseWindow()
        {
            if (!IsDebugEnabled)
            {
                graphics.PreferredBackBufferWidth = Nes.Width * 4;
                graphics.PreferredBackBufferHeight = Nes.Height * 4;
                graphics.ApplyChanges();
                return;
            }
            
            graphics.PreferredBackBufferWidth = 1807;
            graphics.PreferredBackBufferHeight = 960;
            graphics.ApplyChanges();
        }

        private void InitializeTextures()
        {

            nesScreen = new Texture2D(GraphicsDevice, Nes.Width, Nes.Height);
            pattern1 = new Texture2D(GraphicsDevice, 256, 256);
            pattern2 = new Texture2D(GraphicsDevice, 256, 256);
            oam1 = new Texture2D(GraphicsDevice, 64, 64);
            palette1 = new Texture2D(GraphicsDevice, 4, 1);
            palette2 = new Texture2D(GraphicsDevice, 4, 1);
            palette3 = new Texture2D(GraphicsDevice, 4, 1);
            palette4 = new Texture2D(GraphicsDevice, 4, 1);
        }

        private void InitializeSound()
        {
            try
            {
                sound = new DynamicSoundEffectInstance(48000, AudioChannels.Mono);
            }
            catch (Exception e)
            {
                sound = null;
            }
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("fonts/Cascadia");

            Cartridge cart;
            cart = Loader.LoadFromFile(@"..\..\..\mario.nes");
            cart = Loader.LoadFromFile(@"..\..\..\AccuracyCoin.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\donkey.nes");
            
            //cart = Loader.LoadFromFile(@"..\..\..\contra.nes"); // UxROM
            //cart = Loader.LoadFromFile(@"..\..\..\megaman.nes"); // UxROM
            //cart = Loader.LoadFromFile(@"..\..\..\castlevania.nes"); // UxROM
            
            cart = Loader.LoadFromFile(@"..\..\..\metroid.nes"); // MMC1
            //cart = Loader.LoadFromFile(@"..\..\..\icarus.nes"); // MMC1
            //cart = Loader.LoadFromFile(@"..\..\..\megaman2.nes"); // MMC1
            
            //cart = CpuTestRoms();
            //cart = PpuTestRoms();

            nes.Insert(cart);
        }

        private static Cartridge PpuTestRoms()
        {
            Cartridge cart;
            //cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\01-vbl_basics.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\02-vbl_set_time.nes");
            cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\03-vbl_clear_time.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\04-nmi_control.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\05-nmi_timing.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\06-suppression.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\07-nmi_on_timing.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\08-nmi_off_timing.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\09-even_odd_frames.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\ppu_tests\10-even_odd_timing.nes");

            return cart;
        }
        
        private static Cartridge CpuTestRoms()
        {
            Cartridge cart;
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\01-basics.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\02-implied.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\03-immediate.nes");
            cart = Loader.LoadFromFile(@"..\..\..\rom_singles\04-zero_page.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\05-zp_xy.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\06-absolute.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\07-abs_xy.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\08-ind_x.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\09-ind_y.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\10-branches.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\11-stack.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\12-jmp_jsr.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\13-rts.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\14-rti.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\15-brk.nes");
            //cart = Loader.LoadFromFile(@"..\..\..\rom_singles\16-special.nes");
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
            if (currentState.IsKeyDown(Keys.Escape))
                Exit();

            this.gameTime = gameTime;

            UpdateSound();
            EmulationInput();
            UpdateInput();
            
            var start = DateTime.Now;
            
            nes.Update(pause, advanceFrame, advanceScanline, advanceCycle);
            // update nes screen
            nesScreen.SetData(nes.Pixels);
            
            var end = DateTime.Now;
            timePerFrame = (end - start);
            _framesPerSecond = 1.0 / timePerFrame.TotalSeconds;
            var frameTime = timePerFrame.TotalMilliseconds;
            
            UpdateDebug();
            
            previousState = Keyboard.GetState();
            advanceScanline = false;
            advanceFrame = false;
            advanceCycle = false;
            base.Update(gameTime);
        }

        private void UpdateSound()
        {
            if (sound is null)
                return;

            if (nes.Apu.HasSamples())
            {
                SubmitBuffer();
                if (sound.State != SoundState.Playing)
                    sound.Play();
            }
        }

        private void SubmitBuffer()
        {
            var workingBuffer = nes.Apu.GetOutputBuffer();
            var buffer = ConvertBuffer(workingBuffer);
            sound.SubmitBuffer(buffer);
        }

        private byte[] ConvertBuffer(double[] from)
        {
            const int channels = 1;
            const int bytesPerSample = 2;
            int bufferSize = from.Length;
            byte[] to = new byte[bufferSize * channels * 2];
            
            for (int i = 0; i < bufferSize; i++)
                for (int c = 0; c < channels; c++)
                {
                    var fromIndex = i * channels + c;
                    double clampedSample = Math.Clamp(from[fromIndex], -1.0f, 1.0f);

                    short shortSample =
                        (short) (clampedSample >= 0 ? clampedSample * short.MaxValue : clampedSample * short.MinValue * -1);

                    int index = i * channels * bytesPerSample + c * bytesPerSample;

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

            return to;
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
            if (!IsDebugEnabled)
                return;
            
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
            nes.Controllers[0] = 0x0;
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
            
            var gamepad = GamePad.GetState(0);
            nes.Controllers[0] |= (byte)(gamepad.IsButtonDown(Buttons.A) ? 0x80 : 0x0);
            nes.Controllers[0] |= (byte)(gamepad.IsButtonDown(Buttons.X) ? 0x40 : 0x0);
            nes.Controllers[0] |= (byte)(gamepad.IsButtonDown(Buttons.Back) ? 0x20 : 0x0);
            nes.Controllers[0] |= (byte)(gamepad.IsButtonDown(Buttons.Start) ? 0x10 : 0x0);
            nes.Controllers[0] |= (byte)(gamepad.IsButtonDown(Buttons.DPadUp) ? 0x08 : 0x0);
            nes.Controllers[0] |= (byte)(gamepad.IsButtonDown(Buttons.DPadDown) ? 0x04 : 0x0);
            nes.Controllers[0] |= (byte)(gamepad.IsButtonDown(Buttons.DPadLeft) ? 0x02 : 0x0);
            nes.Controllers[0] |= (byte)(gamepad.IsButtonDown(Buttons.DPadRight) ? 0x01 : 0x0);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // NES Screen
            spriteBatch.Draw(nesScreen, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 1);
            
            DrawDebug();

            spriteBatch.End();
            
            base.Draw(gameTime);
        }

        private void DrawDebug()
        {
            if (!IsDebugEnabled)
                return;
            
            var offset=  DrawCpu();
            DrawText(offset);
            DrawPpu();
        }

        private void DrawText(int offsetY)
        {
            var screenOffsetX = nesScreen.Width * 4 + 5;
            var textHeight = (int) font.MeasureString("RAM").Y;
            offsetY += textHeight;

            spriteBatch.DrawString(font, $"Cycle: {nes.Ppu.Cycle,-3} -- Scanline: {nes.Ppu.Scanline,-3} -- Frames: {nes.Ppu.FrameCount:N0}",
                new Vector2(screenOffsetX, offsetY), Color.White);            
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"Pause: {pause}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"VRAM: {nes.Ppu.VRam.Raw}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"NametableX: {nes.Ppu.VRam.NametableX} - NametableY: {nes.Ppu.VRam.NametableY}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"CoarseX: {nes.Ppu.VRam.CoarseX}, FineX: {nes.Ppu.FineX} - CoarseY: {nes.Ppu.VRam.CoarseY}, FineY: {nes.Ppu.VRam.FineY}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"TRAM: {nes.Ppu.TRam.Raw}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"NametableX: {nes.Ppu.TRam.NametableX} - NametableY: {nes.Ppu.TRam.NametableY}",
                new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"CoarseX: {nes.Ppu.TRam.CoarseX}, FineX: -- - CoarseY: {nes.Ppu.TRam.CoarseY}, FineY: {nes.Ppu.TRam.FineY}",
                new Vector2(screenOffsetX, offsetY), Color.White);
        }

        private int DrawCpu()
        {
            var screenOffsetX = nesScreen.Width * 4 + 5;
            var offsetY = 0;
            var textHeight = (int)font.MeasureString("Status").Y;
            // Status
            DrawStatus(screenOffsetX, offsetY);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"PC: ${nes.Cpu.PC:X4}", new Vector2(screenOffsetX, offsetY),
                Color.White);            
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"A: ${nes.Cpu.A:X2} [{nes.Cpu.A}]", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"X: ${nes.Cpu.X:X2} [{nes.Cpu.X}]", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"Y: ${nes.Cpu.Y:X2} [{nes.Cpu.Y}]", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"Stack: ${nes.Cpu.SP:X2} [{nes.Cpu.SP}]", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"NES Frame Time: {timePerFrame.TotalMilliseconds:00.0}ms -- FPS: {_framesPerSecond:000}fps", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            spriteBatch.DrawString(font, $"Cycles: CPU: {nes.Cpu.CycleCount,-13:N0}-- PPU: {nes.Ppu.CycleCount:N0}", new Vector2(screenOffsetX, offsetY),
                Color.White);

            return offsetY;
        }

        private void DrawStatus(int screenOffsetX, int offsetY)
        {
            var status = nes.Cpu.Status;
            var offsetX = 0;
            spriteBatch.DrawString(font, "Status: ", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetX += (int) font.MeasureString("Status: ").X;
            spriteBatch.DrawString(font, "N ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.Negative)? Color.Green : Color.DarkRed);
            var twoLetters = (int) font.MeasureString("V ").X;
            offsetX += twoLetters;
            spriteBatch.DrawString(font, "V ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.Overflow)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            spriteBatch.DrawString(font, "- ", new Vector2(screenOffsetX+offsetX, offsetY), Color.Gray);
            offsetX += twoLetters;
            spriteBatch.DrawString(font, "B ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.BreakCommand)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            spriteBatch.DrawString(font, "D ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.DecimalMode)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            spriteBatch.DrawString(font, "I ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.InterruptDisable)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            spriteBatch.DrawString(font, "Z ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.Zero)? Color.Green : Color.DarkRed);
            offsetX += twoLetters;
            spriteBatch.DrawString(font, "C ", new Vector2(screenOffsetX+offsetX, offsetY), status.IsSet(CpuFlags.Carry)? Color.Green : Color.DarkRed);
        }

        private void DrawPpu()
        {
            var screenOffsetX = nesScreen.Width * 4 + 5;
            var screenOffsetY = nesScreen.Height * 4 - 277;
            var patternOffsetX = pattern1.Width + 5;
            var patternOffsetY = palette1.Height * 16 + 5;

            // Palette
            spriteBatch.Draw(palette1, new Vector2(screenOffsetX, screenOffsetY), null,
                Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 1);

            spriteBatch.Draw(palette2, new Vector2(screenOffsetX + 1 * 4 * 16 + 24, screenOffsetY), null,
                Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 1);

            spriteBatch.Draw(palette3, new Vector2(screenOffsetX + 2 * 4 * 16 + 2 * 24, screenOffsetY), null,
                Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 1);

            spriteBatch.Draw(palette4, new Vector2(screenOffsetX + 3 * 4 * 16 + 3 * 24, screenOffsetY), null,
                Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 1);


            // PatternTable
            spriteBatch.Draw(pattern1, new Vector2(screenOffsetX, screenOffsetY + patternOffsetY), null,
                Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 1);

            spriteBatch.Draw(pattern2, new Vector2(screenOffsetX + patternOffsetX, screenOffsetY + patternOffsetY), null,
                Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 1);
            
            spriteBatch.Draw(oam1, new Vector2(screenOffsetX + patternOffsetX*2, screenOffsetY + patternOffsetY), null,
                Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 1);
        }
    }
}