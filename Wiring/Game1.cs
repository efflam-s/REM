using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wiring
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Schematic schem;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            schem = new Schematic("main");
            schem.wires.Add(new Wire());
            schem.wires.Add(new Wire());
            schem.wires.Add(new Wire());
            schem.wires.Add(new Wire());
            schem.AddComponent(new Input(schem.wires[0], new Vector2(32, 32)));
            schem.AddComponent(new Input(schem.wires[1], new Vector2(32, 96)));
            schem.AddComponent(new Not(schem.wires[0], schem.wires[2], new Vector2(64, 32)));
            schem.AddComponent(new Not(schem.wires[1], schem.wires[2], new Vector2(64, 96)));
            schem.AddComponent(new Not(schem.wires[2], schem.wires[3], new Vector2(96, 64)));
            schem.AddComponent(new Output(schem.wires[3], new Vector2(128, 64)));
            schem.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Schematic.LoadContent(Content);
            Component.LoadContent(Content);
            Wire.LoadContent(Content);
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Press N to set inputs[0] to true
            if (Keyboard.GetState().IsKeyDown(Keys.N))
                schem.inputs[0].value = true;
            else
                schem.inputs[0].value = false;
            schem.inputs[0].Update();

            // Press B to set inputs[1] to true
            if (Keyboard.GetState().IsKeyDown(Keys.B))
                schem.inputs[1].value = true;
            else
                schem.inputs[1].value = false;
            schem.inputs[1].Update();

            //Console.WriteLine(schem.outputs[0].GetValue());

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, transformMatrix:Matrix.CreateScale(4));
            GraphicsDevice.Clear(Color.LightGray);
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            schem.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
