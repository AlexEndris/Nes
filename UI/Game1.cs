using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Hardware;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Nes nes;
        
        private Texture2D nesScreen;
        
        private Texture2D pattern1;
        private Texture2D pattern2;
        
        private Texture2D palette1;
        private Texture2D palette2;
        private Texture2D palette3;
        private Texture2D palette4;
        private TimeSpan _timePerFrame;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 60.1));
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            nes = new Nes();
            nesScreen = new Texture2D(GraphicsDevice, Nes.Width, Nes.Height);
            pattern1 = new Texture2D(GraphicsDevice, 256, 256);
            pattern2 = new Texture2D(GraphicsDevice, 256, 256);
            palette1 = new Texture2D(GraphicsDevice, 4, 1);
            palette2 = new Texture2D(GraphicsDevice, 4, 1);
            palette3 = new Texture2D(GraphicsDevice, 4, 1);
            palette4 = new Texture2D(GraphicsDevice, 4, 1);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("fonts/Arial");

            Cartridge cart;
            cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\nestest.nes");
            //cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\mario.nes");
            cart = Loader.LoadFromFile(@"D:\Development\Programming\c#\NES\UI\donkey.nes");
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

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                pause = !pause;
            
            if (Keyboard.GetState().IsKeyDown(Keys.Back))
                nes.Reset();
            
            UpdateInput();
            
            var start = DateTime.Now;

            nes.Update(gameTime, pause);
            // update nes screen
            nesScreen.SetData(nes.Pixels);

            var end = DateTime.Now;
            _timePerFrame = (end - start);
            _framesPerSecond = 1.0 / _timePerFrame.TotalSeconds;
            var frameTime = _timePerFrame.TotalMilliseconds;

            UpdateDebug();

            base.Update(gameTime);
        }

        private void UpdateDebug()
        {
            // update pattern
            // var data = nes.Ppu.GetOamTable(0);
            // pattern2.SetData(data);
            var data = nes.Ppu.GetPatternTable(0, 0);
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
            //DrawText(offset);
            //DrawTemp(offset);
            DrawPpu();
        }

        private void DrawText(int offset)
        {
            var screenOffsetX = nesScreen.Width * 4 + 5;
            var textHeight = (int)_font.MeasureString("RAM").Y;
            offset += textHeight;

            ushort position = 0x6004 & 0x1FFF;

            List<byte> text = new List<byte>();
            
            while (nes.CpuBus.Temp.Span[position] != 0)
            {
                text.Add(nes.CpuBus.Temp.Span[position]);
                position++;
            }
            text.Add(0);
            var encoded = Encoding.Default.GetString(text.ToArray());
        }

        private void DrawTemp(int offsetY)
        {
            var screenOffsetX = nesScreen.Width * 4 + 5;
            var textHeight = (int)_font.MeasureString("RAM").Y;

            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"RAM:", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"{nes.CpuBus.Temp.Span[0]:X2} {nes.CpuBus.Temp.Span[1]:X2} {nes.CpuBus.Temp.Span[2]:X2} {nes.CpuBus.Temp.Span[3]:X2}", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"{nes.CpuBus.Temp.Span[4]:X2} {nes.CpuBus.Temp.Span[5]:X2} {nes.CpuBus.Temp.Span[6]:X2} {nes.CpuBus.Temp.Span[7]:X2}", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"{nes.CpuBus.Temp.Span[8]:X2} {nes.CpuBus.Temp.Span[9]:X2} {nes.CpuBus.Temp.Span[10]:X2} {nes.CpuBus.Temp.Span[11]:X2}", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"{nes.CpuBus.Temp.Span[12]:X2} {nes.CpuBus.Temp.Span[13]:X2} {nes.CpuBus.Temp.Span[14]:X2} {nes.CpuBus.Temp.Span[15]:X2}", new Vector2(screenOffsetX, offsetY), Color.White);        }
        
        private void DrawRam(int offsetY)
        {
            var screenOffsetX = nesScreen.Width * 4 + 5;
            var textHeight = (int)_font.MeasureString("RAM").Y;

            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"RAM:", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"{nes.CpuBus.Ram.Span[0x02D0]:X2} {nes.CpuBus.Ram.Span[0x02D1]:X2} {nes.CpuBus.Ram.Span[0x02D2]:X2} {nes.CpuBus.Ram.Span[0x02D3]:X2}", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"{nes.CpuBus.Ram.Span[0x02D4]:X2} {nes.CpuBus.Ram.Span[0x02D5]:X2} {nes.CpuBus.Ram.Span[0x02D6]:X2} {nes.CpuBus.Ram.Span[0x02D7]:X2}", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"{nes.CpuBus.Ram.Span[0x02D8]:X2} {nes.CpuBus.Ram.Span[0x02D9]:X2} {nes.CpuBus.Ram.Span[0x02DA]:X2} {nes.CpuBus.Ram.Span[0x02DB]:X2}", new Vector2(screenOffsetX, offsetY), Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"{nes.CpuBus.Ram.Span[0x02DC]:X2} {nes.CpuBus.Ram.Span[0x02DD]:X2} {nes.CpuBus.Ram.Span[0x02DE]:X2} {nes.CpuBus.Ram.Span[0x02DF]:X2}", new Vector2(screenOffsetX, offsetY), Color.White);
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
            _spriteBatch.DrawString(_font, $"Frame Time: {_timePerFrame.TotalMilliseconds:##.#}ms", new Vector2(screenOffsetX, offsetY),
                Color.White);
            offsetY += textHeight;
            _spriteBatch.DrawString(_font, $"FPS: {_framesPerSecond:###}fps", new Vector2(screenOffsetX, offsetY),
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
        }
    }
}