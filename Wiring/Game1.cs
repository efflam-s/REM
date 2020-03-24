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
        Editor editor;
        Matrix Camera;
        Button[] AddButtons;
        Texture2D toolBar;
        MouseState prevMsState;
        SpriteFont font;

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
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
            editor = new Editor();
            editor.Initialize();
            AddButtons = new Button[5];
            AddButtons[0] = new Button(new Vector2(36, 18), "Ajouter une Connection (C)");
            AddButtons[1] = new Button(new Vector2(36*2, 18), "Ajouter une Entrée");
            AddButtons[2] = new Button(new Vector2(36*3, 18), "Ajouter une Sortie");
            AddButtons[3] = new Button(new Vector2(36*4, 18), "Ajouter un Inverseur");
            AddButtons[4] = new Button(new Vector2(36*5, 18), "Ajouter une Diode");
            prevMsState = Mouse.GetState();
            Camera = Matrix.CreateScale(4) * Matrix.CreateTranslation(0, 36, 0);

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
            Editor.LoadContent(Content);
            Button.LoadContent(Content);
            toolBar = Content.Load<Texture2D>("Button/toolBar");
            AddButtons[0].setTexture(Content.Load<Texture2D>("Button/addWireButton"));
            AddButtons[1].setTexture(Content.Load<Texture2D>("Button/addInputButton"));
            AddButtons[2].setTexture(Content.Load<Texture2D>("Button/addOutputButton"));
            AddButtons[3].setTexture(Content.Load<Texture2D>("Button/addNotButton"));
            AddButtons[4].setTexture(Content.Load<Texture2D>("Button/addDiodeButton"));
            font = Content.Load<SpriteFont>("Arial");
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
            //Window.ClientBounds => rectangle de la fenêtre
            Rectangle EditorWindow = new Rectangle(0, 36, Window.ClientBounds.Width, Window.ClientBounds.Height - 36*2);
            editor.Update(gameTime, ref Camera, EditorWindow);
            for (int i=0; i<AddButtons.Length; i++)
            {
                bool declic = Mouse.GetState().LeftButton == ButtonState.Released && prevMsState.LeftButton == ButtonState.Pressed;
                if (AddButtons[i].toggle && declic)
                {
                    // détoggle dans le cas où on clique n'importe où
                    AddButtons[i].toggle = false;
                }
                if (AddButtons[i].hover(Mouse.GetState().Position.ToVector2()) && declic)
                {
                    AddButtons[i].toggle = true;
                    //AddButtons[i].toggle = !AddButtons[i].toggle;
                    // création d'un composant
                    switch (i)
                    {
                        case 0:
                            editor.tool = Editor.Tool.Wire;
                            break;
                        case 1:
                            editor.AddComponent(new Input(new Wire(), Mouse.GetState().Position.ToVector2()));
                            break;
                        case 2:
                            editor.AddComponent(new Output(new Wire(), Mouse.GetState().Position.ToVector2()));
                            break;
                        case 3:
                            editor.AddComponent(new Not(new Wire(), new Wire(), Mouse.GetState().Position.ToVector2()));
                            break;
                        case 4:
                            editor.AddComponent(new Diode(new Wire(), new Wire(), Mouse.GetState().Position.ToVector2()));
                            break;
                    }
                }
            }
            AddButtons[0].toggle = (editor.tool == Editor.Tool.Wire);
            prevMsState = Mouse.GetState();
            base.Update(gameTime);
            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, transformMatrix: Camera);
            GraphicsDevice.Clear(Color.LightGray);
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            editor.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            spriteBatch.Draw(toolBar, new Rectangle(0, 0, Window.ClientBounds.Width, toolBar.Height), Color.White);
            foreach (Button b in AddButtons)
            {
                b.Draw(spriteBatch);
            }
            spriteBatch.Draw(toolBar, new Rectangle(0, Window.ClientBounds.Height - toolBar.Height, Window.ClientBounds.Width, toolBar.Height), Color.White);
            spriteBatch.DrawString(font, editor.GetInfos() + "  Position Camera : ("+Camera.M41+", "+Camera.M42+")  Zoom : "+Camera.M11, new Vector2(36, Window.ClientBounds.Height -  toolBar.Height*3/4), Color.Black);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
