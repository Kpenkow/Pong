using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Pong
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        bool isGameOver;

        // Ball
        Texture2D ballTexture;
        Vector2 ballPosition;
        Vector2 ballSpeedVector;
        float ballSpeed;

        // Player bats
        Texture2D batTexture;
        Vector2 pl1BatPosition;
        Vector2 pl2BatPosition;
        float batSpeed = 300f;
        const int BatWidth = 20;
        const int BatHeight = 100;

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            ballPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2,
                                       _graphics.PreferredBackBufferHeight / 2);
            ballSpeed = 200f;
            ballSpeedVector = new Vector2(1, -1);

            pl1BatPosition = new Vector2(30, _graphics.PreferredBackBufferHeight / 2);
            pl2BatPosition = new Vector2(_graphics.PreferredBackBufferWidth - 30, _graphics.PreferredBackBufferHeight / 2);

            isGameOver = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load ball texture
            ballTexture = Content.Load<Texture2D>("ball");

            // Create simple bat texture (1x1 white pixel, scaled in draw)
            batTexture = new Texture2D(GraphicsDevice, 1, 1);
            batTexture.SetData(new[] { Color.White });
        }

        private void checkBallCollision()
        {
            if (isGameOver)
                return;

            // Top and bottom wall collision
            if (ballPosition.Y > _graphics.PreferredBackBufferHeight - ballTexture.Height / 2 ||
                ballPosition.Y < ballTexture.Height / 2)
            {
                ballSpeedVector.Y = -ballSpeedVector.Y;
            }

            // Rectangle for ball
            Rectangle ballRect = new Rectangle(
                (int)(ballPosition.X - ballTexture.Width / 2),
                (int)(ballPosition.Y - ballTexture.Height / 2),
                ballTexture.Width, ballTexture.Height);

            // Player 1 bat collision
            Rectangle pl1Rect = new Rectangle(
                (int)(pl1BatPosition.X - BatWidth / 2),
                (int)(pl1BatPosition.Y - BatHeight / 2),
                BatWidth, BatHeight);

            if (ballRect.Intersects(pl1Rect))
            {
                ballSpeedVector.X = Math.Abs(ballSpeedVector.X);
            }

            // Player 2 bat collision
            Rectangle pl2Rect = new Rectangle(
                (int)(pl2BatPosition.X - BatWidth / 2),
                (int)(pl2BatPosition.Y - BatHeight / 2),
                BatWidth, BatHeight);

            if (ballRect.Intersects(pl2Rect))
            {
                ballSpeedVector.X = -Math.Abs(ballSpeedVector.X);
            }

            // Game over if ball hits left or right wall
            if (ballPosition.X < 0 || ballPosition.X > _graphics.PreferredBackBufferWidth)
            {
                isGameOver = true;
            }
        }

        private void updateBallPosition(float updatedBallSpeed)
        {
            if (isGameOver)
                return;

            float ratio = ballSpeedVector.X / ballSpeedVector.Y;
            float deltaY = updatedBallSpeed / (float)Math.Sqrt(1 + ratio * ratio);
            float deltaX = Math.Abs(ratio * deltaY);

            ballPosition.X += (ballSpeedVector.X > 0) ? deltaX : -deltaX;
            ballPosition.Y += (ballSpeedVector.Y > 0) ? deltaY : -deltaY;
        }

        private void updateBatsPositions(float deltaTime)
        {
            KeyboardState kstate = Keyboard.GetState();

            // Player 1 controls (W/S)
            if (kstate.IsKeyDown(Keys.W))
            {
                pl1BatPosition.Y -= batSpeed * deltaTime;
            }
            if (kstate.IsKeyDown(Keys.S))
            {
                pl1BatPosition.Y += batSpeed * deltaTime;
            }

            // Player 2 controls (Up/Down)
            if (kstate.IsKeyDown(Keys.Up))
            {
                pl2BatPosition.Y -= batSpeed * deltaTime;
            }
            if (kstate.IsKeyDown(Keys.Down))
            {
                pl2BatPosition.Y += batSpeed * deltaTime;
            }

            // Clamp positions
            pl1BatPosition.Y = MathHelper.Clamp(pl1BatPosition.Y, BatHeight / 2, _graphics.PreferredBackBufferHeight - BatHeight / 2);
            pl2BatPosition.Y = MathHelper.Clamp(pl2BatPosition.Y, BatHeight / 2, _graphics.PreferredBackBufferHeight - BatHeight / 2);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            checkBallCollision();
            updateBallPosition(ballSpeed * deltaTime);
            updateBatsPositions(deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw ball
            _spriteBatch.Draw(
                ballTexture,
                ballPosition,
                null,
                Color.White,
                0f,
                new Vector2(ballTexture.Width / 2, ballTexture.Height / 2),
                Vector2.One,
                SpriteEffects.None,
                0f
            );

            // Draw player 1 bat
            _spriteBatch.Draw(
                batTexture,
                new Rectangle((int)pl1BatPosition.X - BatWidth / 2, (int)pl1BatPosition.Y - BatHeight / 2, BatWidth, BatHeight),
                Color.Red
            );

            // Draw player 2 bat
            _spriteBatch.Draw(
                batTexture,
                new Rectangle((int)pl2BatPosition.X - BatWidth / 2, (int)pl2BatPosition.Y - BatHeight / 2, BatWidth, BatHeight),
                Color.Green
            );

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}