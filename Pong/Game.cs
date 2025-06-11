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

        // Game state
        bool isGameOver;
        string gameOverMessage = "";

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

        // Font
        SpriteFont gameFont;

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            ResetGame();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load ball texture
            ballTexture = Content.Load<Texture2D>("ball");

            // Load font
            gameFont = Content.Load<SpriteFont>("GameFont");

            // Create simple white texture
            batTexture = new Texture2D(GraphicsDevice, 1, 1);
            batTexture.SetData(new[] { Color.White });
        }

        private void ResetGame()
        {
            ballPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2,
                                       _graphics.PreferredBackBufferHeight / 2);
            ballSpeed = 200f;
            ballSpeedVector = new Vector2(1, -1);

            pl1BatPosition = new Vector2(30, _graphics.PreferredBackBufferHeight / 2);
            pl2BatPosition = new Vector2(_graphics.PreferredBackBufferWidth - 30, _graphics.PreferredBackBufferHeight / 2);

            isGameOver = false;
            gameOverMessage = "";
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

            Rectangle ballRect = new Rectangle(
                (int)(ballPosition.X - ballTexture.Width / 2),
                (int)(ballPosition.Y - ballTexture.Height / 2),
                ballTexture.Width, ballTexture.Height);

            Rectangle pl1Rect = new Rectangle(
                (int)(pl1BatPosition.X - BatWidth / 2),
                (int)(pl1BatPosition.Y - BatHeight / 2),
                BatWidth, BatHeight);

            if (ballRect.Intersects(pl1Rect))
            {
                ballSpeedVector.X = Math.Abs(ballSpeedVector.X);
            }

            Rectangle pl2Rect = new Rectangle(
                (int)(pl2BatPosition.X - BatWidth / 2),
                (int)(pl2BatPosition.Y - BatHeight / 2),
                BatWidth, BatHeight);

            if (ballRect.Intersects(pl2Rect))
            {
                ballSpeedVector.X = -Math.Abs(ballSpeedVector.X);
            }

            // Check for game over
            if (ballPosition.X < 0)
            {
                isGameOver = true;
                gameOverMessage = "Game Over - Player 2 Wins!";
            }
            else if (ballPosition.X > _graphics.PreferredBackBufferWidth)
            {
                isGameOver = true;
                gameOverMessage = "Game Over - Player 1 Wins!";
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

            // Player 1: W / S
            if (kstate.IsKeyDown(Keys.W))
                pl1BatPosition.Y -= batSpeed * deltaTime;
            if (kstate.IsKeyDown(Keys.S))
                pl1BatPosition.Y += batSpeed * deltaTime;

            // Player 2: Up / Down
            if (kstate.IsKeyDown(Keys.Up))
                pl2BatPosition.Y -= batSpeed * deltaTime;
            if (kstate.IsKeyDown(Keys.Down))
                pl2BatPosition.Y += batSpeed * deltaTime;

            // Clamp
            pl1BatPosition.Y = MathHelper.Clamp(pl1BatPosition.Y, BatHeight / 2, _graphics.PreferredBackBufferHeight - BatHeight / 2);
            pl2BatPosition.Y = MathHelper.Clamp(pl2BatPosition.Y, BatHeight / 2, _graphics.PreferredBackBufferHeight - BatHeight / 2);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState kstate = Keyboard.GetState();

            // Restart with R
            if (isGameOver && kstate.IsKeyDown(Keys.R))
            {
                ResetGame();
            }

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

            // Player 1 bat
            _spriteBatch.Draw(
                batTexture,
                new Rectangle((int)pl1BatPosition.X - BatWidth / 2, (int)pl1BatPosition.Y - BatHeight / 2, BatWidth, BatHeight),
                Color.Red
            );

            // Player 2 bat
            _spriteBatch.Draw(
                batTexture,
                new Rectangle((int)pl2BatPosition.X - BatWidth / 2, (int)pl2BatPosition.Y - BatHeight / 2, BatWidth, BatHeight),
                Color.Green
            );

            // Game Over message + restart instruction
            if (isGameOver)
            {
                Vector2 textSize = gameFont.MeasureString(gameOverMessage);
                Vector2 textPosition = new Vector2(
                    (_graphics.PreferredBackBufferWidth - textSize.X) / 2,
                    (_graphics.PreferredBackBufferHeight - textSize.Y) / 2 - 20
                );

                _spriteBatch.DrawString(gameFont, gameOverMessage, textPosition, Color.Yellow);

                string restartMessage = "Press R to restart";
                Vector2 restartTextSize = gameFont.MeasureString(restartMessage);
                Vector2 restartTextPosition = new Vector2(
                    (_graphics.PreferredBackBufferWidth - restartTextSize.X) / 2,
                    textPosition.Y + textSize.Y + 10
                );

                _spriteBatch.DrawString(gameFont, restartMessage, restartTextPosition, Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}