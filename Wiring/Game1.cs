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
        TextureButton[] AddButtons, MiscButtons;
        List<StringButton> SchematicPath;
        Texture2D buttonsBar, separator, pathSeparator;
        InputManager Inpm;
        SpriteFont font;
        StringBuilder builder;
        bool isListeningToInputText;
        string savePath;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Window.TextInput += Window_TextInput; // évenements de textInput pour le renommage
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            editor = new Editor();
            editor.Initialize();
            AddButtons = new TextureButton[5];
            AddButtons[0] = new TextureButton(new Vector2(7+36*2, 2), "Ajouter une Entrée");
            AddButtons[1] = new TextureButton(new Vector2(7+36*3, 2), "Ajouter une Sortie");
            AddButtons[2] = new TextureButton(new Vector2(7+36*4, 2), "Ajouter un Inverseur");
            AddButtons[3] = new TextureButton(new Vector2(7+36*5, 2), "Ajouter une Diode");
            AddButtons[4] = new TextureButton(new Vector2(7+36*6, 2), "Ajouter une Boîte Noire");
            MiscButtons = new TextureButton[8];
            MiscButtons[0] = new TextureButton(new Vector2(7+36*3, 38), "Retour (Echap)");
            MiscButtons[1] = new TextureButton(new Vector2(4, 2), "Selectionner/Modifier (S)");
            MiscButtons[2] = new TextureButton(new Vector2(43, 2), "Ajouter une Connexion (C)");
            MiscButtons[3] = new TextureButton(new Vector2(10+36*7, 2), "Panoramique (H)");
            MiscButtons[4] = new TextureButton(new Vector2(10+36*8, 2), "Zoom (Z)");
            MiscButtons[5] = new TextureButton(new Vector2(4, 38), "Ouvrir (Ctrl + O)");
            MiscButtons[6] = new TextureButton(new Vector2(4+36, 38), "Enregistrer (Ctrl + S)");
            MiscButtons[7] = new TextureButton(new Vector2(4+36*2, 38), "Enregistrer Sous (Ctrl + Maj + S)");
            SchematicPath = new List<StringButton>();
            Inpm = new InputManager();
            Camera = Matrix.CreateScale(4) * Matrix.CreateTranslation(0, 36*2, 0);
            builder = new StringBuilder();
            isListeningToInputText = false;
            savePath = "";
            // seed random : DateTime.Now.Millisecond;

            //editor.mainSchem = SchemReader.Read("Schematics/exampleSchem.schem");
            //editor.mainSchem.Initialize();
            //SchemWriter.write("Schematics/testWrite.schem", editor.mainSchem, true);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Wire.LoadContent(Content);
            Component.LoadContent(Content);
            Schematic.LoadContent(Content);
            Editor.LoadContent(Content);
            Button.LoadContent(Content);

            buttonsBar = Content.Load<Texture2D>("Button/buttonsBar");
            separator = Content.Load<Texture2D>("Button/separator");
            pathSeparator = Content.Load<Texture2D>("Button/pathSeparator");
            AddButtons[0].setTexture(Content.Load<Texture2D>("Button/addInputButton"));
            AddButtons[1].setTexture(Content.Load<Texture2D>("Button/addOutputButton"));
            AddButtons[2].setTexture(Content.Load<Texture2D>("Button/addNotButton"));
            AddButtons[3].setTexture(Content.Load<Texture2D>("Button/addDiodeButton"));
            AddButtons[4].setTexture(Content.Load<Texture2D>("Button/addBlackBoxButton"));
            MiscButtons[0].setTexture(Content.Load<Texture2D>("Button/arrowUp"));
            MiscButtons[1].setTexture(Content.Load<Texture2D>("Button/arrow"));
            MiscButtons[2].setTexture(Content.Load<Texture2D>("Button/addWireButton"));
            MiscButtons[3].setTexture(Content.Load<Texture2D>("Button/hand"));
            MiscButtons[4].setTexture(Content.Load<Texture2D>("Button/zoom"));
            MiscButtons[5].setTexture(Content.Load<Texture2D>("Button/Open"));
            MiscButtons[6].setTexture(Content.Load<Texture2D>("Button/Save"));
            MiscButtons[7].setTexture(Content.Load<Texture2D>("Button/SaveAs"));
            font = Content.Load<SpriteFont>("Arial");

            SchematicPath.Add(new StringButton(new Vector2(10+36*4, 41), editor.mainSchem.Name, "Renommer"));
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
            Rectangle EditorWindow = new Rectangle(0, 36*2, Window.ClientBounds.Width, Window.ClientBounds.Height - 36*3);
            
            if (!isListeningToInputText)
                editor.Update(gameTime, ref Camera, EditorWindow);

            Inpm.Update();

            if (editor.schemPile.Count > SchematicPath.Count)
            {
                Vector2 newPosition = new Vector2(SchematicPath[SchematicPath.Count - 1].Bounds.Right, SchematicPath[SchematicPath.Count - 1].Bounds.Top) + new Vector2(8, 0);
                SchematicPath[SchematicPath.Count - 1].ToolTip = "";
                SchematicPath.Add(new StringButton(newPosition, editor.mainSchem.Name, "Renommer"));
            }

            if (Inpm.OnPressed(Keys.Escape) && !isListeningToInputText)
            {
                if (editor.schemPile.Count > 1)
                {
                    // navigation vers le schematic parent
                    schematicNav(editor.schemPile.Count - 2);
                }
            }

            if (Inpm.leftClic == InputManager.ClicState.Clic)
            {
                Inpm.SaveClic(gameTime);
            }
            bool declic = Inpm.leftClic == InputManager.ClicState.Declic;
            for (int i = 0; i < AddButtons.Length; i++)
            {
                if (AddButtons[i].toggle && declic)
                {
                    // détoggle dans le cas où on clique n'importe où
                    AddButtons[i].toggle = false;
                }
                if (AddButtons[i].hover(Inpm.MsPosition()) && declic && AddButtons[i].hover(Inpm.mousePositionOnClic))
                {
                    AddButtons[i].toggle = true;
                    //AddButtons[i].toggle = !AddButtons[i].toggle;
                    // création d'un composant
                    switch (i)
                    {
                        case 0:
                            editor.AddComponent(new Input(new Wire(), Inpm.MsPosition()));
                            break;
                        case 1:
                            editor.AddComponent(new Output(new Wire(), Inpm.MsPosition()));
                            break;
                        case 2:
                            editor.AddComponent(new Not(new Wire(), new Wire(), Inpm.MsPosition()));
                            break;
                        case 3:
                            editor.AddComponent(new Diode(new Wire(), new Wire(), Inpm.MsPosition()));
                            break;
                        case 4:
                            editor.AddComponent(BlackBox.Default(Inpm.MsPosition()));
                            break;
                    }
                }
            }

            for (int i = 0; i < MiscButtons.Length; i++)
            {
                if (MiscButtons[i].hover(Inpm.MsPosition()) && declic)
                {
                    switch (i)
                    {
                        case 0:
                            if (editor.schemPile.Count > 1)
                            {
                                // navigation vers le schematic parent
                                schematicNav(editor.schemPile.Count - 2);
                            }
                            break;
                        case 1:
                            editor.tool = Editor.Tool.Edit;
                            break;
                        case 2:
                            editor.tool = Editor.Tool.Wire;
                            break;
                        case 3:
                            editor.tool = Editor.Tool.Pan;
                            break;
                        case 4:
                            editor.tool = Editor.Tool.Zoom;
                            break;
                        case 5:
                            // Bouton ouvrir un schematic
                            Open();
                            break;
                        case 6:
                            // Bouton sauvegarder sous le nom actuel
                            Save();
                            break;
                        case 7:
                            // Bouton sauvegarder sous...
                            SaveAs();
                            break;
                    }
                }
            }

            for (int i = 0; i < SchematicPath.Count; i++)
            {
                if (SchematicPath[i].hover(Inpm.MsPosition()) && declic)
                {
                    if (i == SchematicPath.Count - 1)
                    {
                        // schematic courant => rename
                        isListeningToInputText = true;
                        SchematicPath[i].toggle = true;
                        builder = new StringBuilder(SchematicPath[i].text);
                    }
                    else
                    {
                        // autre schematic => navigation
                        schematicNav(i);
                    }
                }
            }
            if (isListeningToInputText && Inpm.leftClic == InputManager.ClicState.Clic)
            {
                SchematicPath[SchematicPath.Count - 1].toggle = false;
                // fin de renommage
                isListeningToInputText = false;
                if (SchematicPath[SchematicPath.Count - 1].text.Length == 0)
                    SchematicPath[SchematicPath.Count - 1].setText(editor.mainSchem.Name);
                else
                    editor.mainSchem.Name = SchematicPath[SchematicPath.Count - 1].text;
            }

            // set buttons toggle
            MiscButtons[1].toggle = editor.tool == Editor.Tool.Edit || editor.tool == Editor.Tool.Move || editor.tool == Editor.Tool.Select;
            MiscButtons[2].toggle = editor.tool == Editor.Tool.Wire;
            MiscButtons[3].toggle = editor.tool == Editor.Tool.Pan;
            MiscButtons[4].toggle = editor.tool == Editor.Tool.Zoom;

            if (Inpm.Control && !Inpm.Alt && !Inpm.Shift && Inpm.OnPressed(Keys.R))
            {
                // Rename
                isListeningToInputText = true;
                SchematicPath[SchematicPath.Count-1].toggle = true;
                builder = new StringBuilder(SchematicPath[SchematicPath.Count - 1].text);
            }
            if (Inpm.leftClic == InputManager.ClicState.Up && Inpm.Control && !Inpm.Alt && !Inpm.Shift && Inpm.OnPressed(Keys.S))
                // Ctrl+S : sauvegarder sous le nom actuel
                Save();
            if (Inpm.leftClic == InputManager.ClicState.Up && Inpm.Control && !Inpm.Alt && Inpm.Shift && Inpm.OnPressed(Keys.S))
                // Ctrl+Shift+S : sauvegarder sous...
                SaveAs();
            if (Inpm.leftClic == InputManager.ClicState.Up && Inpm.Control && !Inpm.Alt && !Inpm.Shift && Inpm.OnPressed(Keys.O))
                // Ctrl+O : Ouvrir un schematic
                Open();

            //Console.WriteLine(SchematicPath[0].Bounds);

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
            editor.Draw(spriteBatch, new Rectangle(0, 36 * 2, Window.ClientBounds.Width, Window.ClientBounds.Height - 36 * 3));
            spriteBatch.End();

            spriteBatch.Begin();
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            // schematic bar
            spriteBatch.Draw(buttonsBar, new Rectangle(0, buttonsBar.Height, Window.ClientBounds.Width, buttonsBar.Height), Color.White);
            spriteBatch.Draw(separator, new Vector2(1+36*3, 36), Color.White);
            spriteBatch.Draw(separator, new Vector2(4+36*4, 36), Color.White);
            foreach (StringButton b in SchematicPath)
            {
                b.Draw(spriteBatch);
                if (b != SchematicPath[SchematicPath.Count-1])
                {
                    spriteBatch.Draw(pathSeparator, new Vector2(b.Bounds.Right, b.Bounds.Top), Color.White);
                }
                else if (isListeningToInputText)
                {
                    b.DrawEditCursor(spriteBatch, gameTime);
                }
            }
            // tool bar
            spriteBatch.Draw(buttonsBar, new Rectangle(0, 0, Window.ClientBounds.Width, buttonsBar.Height), Color.White);
            spriteBatch.Draw(separator, new Vector2(1+36, 0), Color.White);
            spriteBatch.Draw(separator, new Vector2(4+36*7, 0), Color.White);
            foreach (Button b in AddButtons)
                b.Draw(spriteBatch);
            // debug bar
            spriteBatch.Draw(buttonsBar, new Rectangle(0, Window.ClientBounds.Height - buttonsBar.Height, Window.ClientBounds.Width, buttonsBar.Height), Color.White);
            spriteBatch.DrawString(font, editor.GetInfos() + "  FrameRate : " + (1/gameTime.ElapsedGameTime.TotalSeconds).ToString("0.00") + " fps", new Vector2(36, Window.ClientBounds.Height -  buttonsBar.Height*3/4), StringButton.textColor);
            // other buttons
            foreach (Button b in MiscButtons)
                b.Draw(spriteBatch);

            // blackbox tooltip
            if (editor.tool == Editor.Tool.Edit || editor.tool == Editor.Tool.Wire)
            {
                if (editor.currentHoveredComponent() is BlackBox bb)
                {
                    Button.DrawToolTip(spriteBatch, bb.schem.Name, Inpm.MsPosition() + new Vector2(0, 25));
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            if (isListeningToInputText)
            {
                if (e.Key == Keys.Enter)
                {
                    // fin de renommage
                    isListeningToInputText = false;
                    if (SchematicPath[SchematicPath.Count - 1].text.Length == 0)
                        SchematicPath[SchematicPath.Count - 1].setText(editor.mainSchem.Name);
                    else
                        editor.mainSchem.Name = SchematicPath[SchematicPath.Count - 1].text;
                    SchematicPath[SchematicPath.Count - 1].toggle = false;
                    return;
                } else {
                    try
                    {
                        if (e.Key == Keys.Back)
                        {
                            if (builder.Length > 0)
                                builder.Remove(builder.Length - 1, 1);
                        }
                        else
                            builder.Append(e.Character);
                        SchematicPath[SchematicPath.Count - 1].setText(builder.ToString());
                    } catch (ArgumentException) 
                    {
                        // caractère non valide : ctrl + qqch, echap...
                        builder.Remove(builder.Length - 1, 1);
                        SchematicPath[SchematicPath.Count - 1].setText(builder.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Permet de naviguer vers le schematic n°id
        /// </summary>
        private void schematicNav(int id)
        {
            savePath = "";
            editor.schemPile.RemoveRange(id + 1, editor.schemPile.Count - id - 1);
            SchematicPath.RemoveRange(id + 1, SchematicPath.Count - id - 1);
            SchematicPath[SchematicPath.Count - 1].ToolTip = "Renommer";
            foreach (Component c in editor.mainSchem.components)
                if (c is BlackBox bb)
                    bb.ReloadPlugsFromInOut(true);
            editor.clearSelection();
            editor.mainSchem.ReloadWiresFromComponents();
        }

        /// <summary>
        /// Sauvegarde sous le nom actuel si il est déjà enregistré. Sinon, ouvre la fenêtre d'enregistrement sous...
        /// </summary>
        private void Save()
        {
            if (savePath == "")
                SaveAs();
            else
            {
                Console.WriteLine("saving at path : " + savePath);
                SchemWriter.write(savePath, editor.mainSchem, true);
            }
        }
        /// <summary>
        /// Ouvre la fenêtre d'enregistrement sous... de windows
        /// </summary>
        private void SaveAs()
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog1.Filter = "Fichier Schematic|*.schem";
            saveFileDialog1.Title = "Sauvegarder un Schematic";
            saveFileDialog1.FileName = editor.mainSchem.Name + ".schem";
            saveFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Schematics"; // positionner au chemin de l'appication + "Schematics"
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the File via a FileStream created by the OpenFile method.
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                SchemWriter.write(fs, editor.mainSchem, true);
                savePath = fs.Name;
                fs.Close();
            }
        }
        /// <summary>
        /// Ouvre la fenêtre d'ouverture de fichier de windows
        /// </summary>
        private void Open()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "Fichier Schematic|*.schem";
            openFileDialog1.Title = "Ouvrir un Schematic";
            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Schematics"; // positionner au chemin de l'appication + "Schematics"
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                Schematic newSchem = SchemReader.Read(fs);
                editor.mainSchem.Name = newSchem.Name;
                Console.WriteLine("opened " + editor.mainSchem.Name);
                editor.mainSchem.components = newSchem.components;
                editor.mainSchem.Initialize(true);
                SchematicPath[SchematicPath.Count - 1].setText(editor.mainSchem.Name);
                savePath = fs.Name;
                fs.Close();
            }
        }
    }
}
