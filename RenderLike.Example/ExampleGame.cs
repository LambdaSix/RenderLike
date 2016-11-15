using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RenderLike.Example
{
    public class ExampleGame : Game
    {
        public GameInfo Info { get; set; }
        private RLConsole _console;
        private Surface _buffer;
        private SpriteBatch _spriteBatcher;

        public Font CurrentFont { get; set; }

        public GraphicsDeviceManager Graphics { get; set; }
        public static readonly FrameCounter FrameCounter = new FrameCounter();

        public int ConWidth => ScreenSize.Width;
        public int ConHeight => ScreenSize.Height;
        public Rectangle ScreenSize{ get; }

        public ExampleGame(GameInfo info)
        {
            Info = info;
            ScreenSize = new Rectangle(0, 0, info.Width, info.Height);
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public void Start()
        {
            Run();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatcher = new SpriteBatch(GraphicsDevice);

            Texture2D fontTexture = Content.Load<Texture2D>(Info.Font);
            CurrentFont = Font.CreateFromTexture(fontTexture, Info.FontLayout, Info.FontType);

            _console = new RLConsole(GraphicsDevice, CurrentFont, ConWidth, ConHeight);

            Graphics.PreferredBackBufferWidth = ConWidth * CurrentFont.CharacterWidth;
            Graphics.PreferredBackBufferHeight = ConHeight * CurrentFont.CharacterHeight;
            Graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = true;
            Graphics.ApplyChanges();

            _buffer = _console.CreateSurface(ConWidth, ConHeight);

            Console.Clear();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            FrameCounter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            var text0 = $"fps: {FrameCounter.AverageFramesPerSecond}";
            var text1 = $"tps: {FrameCounter.TotalFrames}";

            _buffer.PrintString(ConWidth - text0.Length - 5, 0, text0, Color.Green);
            _buffer.PrintString(ConWidth - text1.Length - 5, 1, text1, Color.Orange);

            _buffer.PrintChar(39, 10, '@');
        }

        protected override void Draw(GameTime gameTime)
        {
            Console.Clear();

            _console.Blit(_buffer, ScreenSize, 0, 0);

            Draw();

            base.Draw(gameTime);
        }

        private void Draw()
        {
            var render = _console.Flush();
            _spriteBatcher.Begin();
            {
                _spriteBatcher.Draw(render, Vector2.Zero, Color.White);
            }
            _spriteBatcher.End();
        }
    }
}