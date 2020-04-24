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
        Matrix editorCamera; // transformations (panoramique et zoom) de la fenêtre de l'éditeur
        InputManager Inpm;
        ButtonsBar firstBar, secondBar;
        TextureButton[] MiscButtons; // boutons qu'on ne peut pas mettre sur les barres (pour l'instant seulement paramètres)
        List<StringButton> SchematicPath; // chemin vers le schematic courant (boutons vers tout les schematics)

        Texture2D buttonsBar, /*separator,*/ pathSeparator; // textures pour les boutons et barres de boutons => à suppr ?
        SpriteFont font;
        StringBuilder builder; // Permet de construire des strings à partir de l'input de l'utilisateur (voir Window_TextInput)
        bool isListeningToInputText;
        string savePath; // Chemin sur le disque dans lequel on enregistre le schematic courant

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
            editorCamera = Matrix.CreateScale(4) * Matrix.CreateTranslation(0, 36 * 2, 0);
            Inpm = new InputManager();
            // Les buttonsBar sont initialisées dans LoadContent
            SchematicPath = new List<StringButton>();
            MiscButtons = new TextureButton[1];
            MiscButtons[0] = new TextureButton(new Vector2(0, 38)); // paramètres

            builder = new StringBuilder();
            isListeningToInputText = false;
            savePath = "";
            Settings.Default();

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
            Settings.LoadContent(Content);
            ButtonsBar.LoadContent(Content);

            buttonsBar = Content.Load<Texture2D>("Button/buttonsBar");
            //separator = Content.Load<Texture2D>("Button/separator");
            pathSeparator = Content.Load<Texture2D>("Button/pathSeparator");
            firstBar = ButtonsBar.firstBar(Content);
            secondBar = ButtonsBar.secondBar(Content);
            MiscButtons[0].setTexture(Content.Load<Texture2D>("Button/Settings"));
            SchematicPath.Add(new StringButton(new Vector2(10 + 36 * 4, 41), editor.mainSchem.Name, "Renommer")); // A besoin de Button.LoadContent pour mesurer la taille du string

            font = Content.Load<SpriteFont>("Arial");

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
            // La fenêtre de l'éditeur correspond à la zone dans laquelle on peut utiliser les outils de l'éditeur
            // C'est à dire toute la fenêtre sauf les barres de boutons (donnée en argument de Editor.Update())
            Rectangle EditorWindow = new Rectangle(0, 36*2, Window.ClientBounds.Width, Window.ClientBounds.Height - 36*3);

            // Menu de paramètres, si il est ouvert
            if (MiscButtons[0].toggle)
                Settings.Update(new Vector2(MiscButtons[0].Bounds.Right + 4, MiscButtons[0].Bounds.Bottom + 2));

            // Ne pas update l'éditeur pendant le renommage permet de ne pas écouter les raccourcis claviers,
            // mais ça arrête aussi le temps (donc le délai des diodes)
            if (!isListeningToInputText)
                editor.Update(gameTime, ref editorCamera, EditorWindow);

            Inpm.Update();
            if (Inpm.leftClic == InputManager.ClicState.Clic)
            {
                Inpm.SaveClic(gameTime);
            }

            if (editor.schemPile.Count > SchematicPath.Count)
            {
                // On est entré dans une blackbox à partir de l'éditeur : il faut donc rajouter un bouton dans SchematicPath
                Vector2 newPosition = new Vector2(SchematicPath[SchematicPath.Count - 1].Bounds.Right, SchematicPath[SchematicPath.Count - 1].Bounds.Top) + new Vector2(8, 0);
                SchematicPath[SchematicPath.Count - 1].ToolTip = "";
                SchematicPath.Add(new StringButton(newPosition, editor.mainSchem.Name, "Renommer"));
                savePath = "";
            }

            if (Inpm.OnPressed(Keys.Escape) && !isListeningToInputText)
            {
                // Retour avec Echap
                Back();
            }

            UpdateButtons();

            // Gestion boutons de navigation => à mettre dans une fonction/une classe ?
            for (int i = 0; i < SchematicPath.Count; i++)
            {
                if (SchematicPath[i].hover(Inpm.MsPosition()) && Inpm.leftClic == InputManager.ClicState.Declic)
                {
                    if (i == SchematicPath.Count - 1)
                    {
                        // Schematic courant => renommage
                        isListeningToInputText = true;
                        SchematicPath[i].toggle = true;
                        builder = new StringBuilder(SchematicPath[i].text);
                    }
                    else
                    {
                        // Autre schematic => navigation
                        schematicNav(i);
                    }
                }
            }
            if (isListeningToInputText && Inpm.leftClic == InputManager.ClicState.Clic)
            {
                // Fin de renommage
                isListeningToInputText = false;
                SchematicPath[SchematicPath.Count - 1].toggle = false;
                if (SchematicPath[SchematicPath.Count - 1].text == "")
                    // Chaîne vide => on garde le nom précedent
                    SchematicPath[SchematicPath.Count - 1].setText(editor.mainSchem.Name);
                else
                    // Sinon on change le nom
                    editor.mainSchem.Name = SchematicPath[SchematicPath.Count - 1].text;
            }

            if (Inpm.Control && !Inpm.Alt && !Inpm.Shift && Inpm.OnPressed(Keys.R))
            {
                // Ctrl+R : renommer le schematic actuel
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

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // spriteBatch de l'éditeur
            spriteBatch.Begin(SpriteSortMode.Immediate, transformMatrix: editorCamera);
            GraphicsDevice.Clear(Color.LightGray); // TODO : rajouter une texture pour le fond (texture loop)
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            // Dessin de l'éditeur
            editor.Draw(spriteBatch, new Rectangle(0, 36 * 2, Window.ClientBounds.Width, Window.ClientBounds.Height - 36 * 3));

            spriteBatch.End();

            // spriteBatch des barres de boutons
            spriteBatch.Begin();
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            // Dessin des barres de boutons
            secondBar.Draw(spriteBatch, Window.ClientBounds.Width);
            firstBar.Draw(spriteBatch, Window.ClientBounds.Width);
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
            // Barre d'informations et de debug
            spriteBatch.Draw(buttonsBar, new Rectangle(0, Window.ClientBounds.Height - buttonsBar.Height, Window.ClientBounds.Width, buttonsBar.Height), Color.White);
            spriteBatch.DrawString(font, editor.GetInfos() + "  FrameRate : " + (1/gameTime.ElapsedGameTime.TotalSeconds).ToString("0.00") + " fps", new Vector2(36, Window.ClientBounds.Height -  buttonsBar.Height*3/4), StringButton.textColor);
            // Boutons qui ne sont pas stockées dans les barres
            foreach (Button b in MiscButtons)
            {
                b.Draw(spriteBatch);
                b.DrawToolTip(spriteBatch);
            }

            // Blackbox tooltip
            if (editor.tool == Editor.Tool.Edit || editor.tool == Editor.Tool.Wire)
            {
                if (editor.currentHoveredComponent() is BlackBox bb)
                {
                    Button.DrawToolTip(spriteBatch, bb.schem.Name, Inpm.MsPosition() + new Vector2(0, 25));
                }
            }

            // Menu de paramètres
            if (MiscButtons[0].toggle) Settings.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Mise à jour les barres de boutons et les autres boutons (sauf SchematicPath !)
        /// </summary>
        void UpdateButtons()
        {
            bool declic = Inpm.leftClic == InputManager.ClicState.Declic;
            // Organisation des boutons pour faciliter les boucles
            Button[] AddButtons = { firstBar[1, 1], firstBar[1, 2], firstBar[1, 3], firstBar[1, 4], firstBar[1, 5] };
            Button[] ToolButtons = { firstBar[0, 0], firstBar[1, 0], firstBar[2, 0], firstBar[2, 1] };
            Button[] FileButtons = secondBar[0].ToArray();
            Button Back = secondBar[1][0];

            // Boutons de création de composants
            for (int i = 0; i < AddButtons.Length; i++)
            {
                if (AddButtons[i].toggle && (declic || editor.tool != Editor.Tool.Move) && i != 0)
                {
                    // Détoggle dans le cas où on clique n'importe où ou si l'outil n'est pas le déplacement
                    AddButtons[i].toggle = false;
                }
                if (AddButtons[i].hover(Inpm.MsPosition()) && declic && AddButtons[i].hover(Inpm.mousePositionOnClic))
                {
                    // Si ce bouton est cliqué
                    AddButtons[i].toggle = true;
                    // Création d'un composant
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

            // Boutons de choix d'outils
            for (int i = 0; i < ToolButtons.Length; i++)
            {
                if (ToolButtons[i].hover(Inpm.MsPosition()) && declic)
                {
                    // Si ce bouton est cliqué
                    // Choix de l'outil
                    switch (i)
                    {
                        case 0:
                            editor.tool = Editor.Tool.Edit;
                            break;
                        case 1:
                            editor.tool = Editor.Tool.Wire;
                            break;
                        case 2:
                            editor.tool = Editor.Tool.Pan;
                            break;
                        case 3:
                            editor.tool = Editor.Tool.Zoom;
                            break;
                    }
                }
            }
            // Set tool buttons toggle
            ToolButtons[0].toggle = editor.tool == Editor.Tool.Edit || editor.tool == Editor.Tool.Move || editor.tool == Editor.Tool.Select;
            ToolButtons[1].toggle = editor.tool == Editor.Tool.Wire;
            ToolButtons[2].toggle = editor.tool == Editor.Tool.Pan;
            ToolButtons[3].toggle = editor.tool == Editor.Tool.Zoom;

            // Boutons "fichier" : Enregistrement et Ouverture
            for (int i = 0; i < FileButtons.Length; i++)
            {
                if (FileButtons[i].hover(Inpm.MsPosition()) && declic)
                {
                    // Si ce bouton est cliqué
                    switch (i) {
                        case 0:
                            // Bouton ouvrir un schematic
                            Open();
                            break;
                        case 1:
                            // Bouton sauvegarder sous le nom actuel
                            Save();
                            break;
                        case 2:
                            // Bouton sauvegarder sous...
                            SaveAs();
                            break;
                    }
                }
            }

            // Bouton retour vers le schematic parent
            if (Back.hover(Inpm.MsPosition()) && declic)
            {
                this.Back();
            }

            // Bouton paramètres
            if (declic)
                if (MiscButtons[0].hover(Inpm.MsPosition()))
                    // Toggle si on clique dessus
                    MiscButtons[0].toggle = !MiscButtons[0].toggle;
                else
                    // Détoggle sinon
                    MiscButtons[0].toggle = false;
            // Position de bouton paramètres (à droite de la fenêtre)
            MiscButtons[0].Bounds.X = Window.ClientBounds.Width - MiscButtons[0].Bounds.Width - 4;
        }


        /// <summary>
        /// Permet d'écrire dans builder que que l'utilisateur tape sur son clavier, pour le stocker dans SchematicPath[SchematicPath.Count - 1]
        /// </summary>
        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            if (isListeningToInputText)
            {
                if (e.Key == Keys.Enter)
                {
                    // Enter : fin de renommage
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
                            // Backspace : suppression du dernier caractère
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
            savePath = ""; // reset
            // Supprime le dernier element de editor.schemPile et SchematicPath
            editor.schemPile.RemoveRange(id + 1, editor.schemPile.Count - id - 1);
            SchematicPath.RemoveRange(id + 1, SchematicPath.Count - id - 1);
            // Set tooltips
            SchematicPath[SchematicPath.Count - 1].ToolTip = "Renommer";
            // Reload plugs, selection and wires
            foreach (Component c in editor.mainSchem.components)
                if (c is BlackBox bb)
                    bb.ReloadPlugsFromInOut(true);
            editor.clearSelection();
            editor.mainSchem.ReloadWiresFromComponents();
        }

        /// <summary>
        /// Navigue vers le schematic précedent, propose à l'utilisateur d'en créer un nouveau si on est à la racine
        /// </summary>
        private void Back()
        {
            if (editor.schemPile.Count > 1)
                // navigation vers le schematic parent
                schematicNav(editor.schemPile.Count - 2);
            else
            {
                // TODO : demander la création d'un schematic parent
            }
        }
        /// <summary>
        /// Sauvegarde sous le nom actuel si il est déjà enregistré. Sinon, ouvre la fenêtre d'enregistrement sous...
        /// </summary>
        private void Save()
        {
            if (savePath == "")
                // Pas de chemin enregistré => on ouvre la fenêtre Enregistrer sous
                SaveAs();
            else
            {
                Console.WriteLine("saving at path : " + savePath);
                SchemWriter.write(savePath, editor.mainSchem, !Settings.OptimizeFileSize);
            }
        }
        /// <summary>
        /// Ouvre la fenêtre d'enregistrement sous... de windows
        /// </summary>
        private void SaveAs()
        {
            // Création d'une fenêtre "Sauvegarder un Schematic" avec Windows.Forms
            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog1.Filter = "Fichier Schematic|*.schem"; // Extensions acceptées
            saveFileDialog1.Title = "Sauvegarder un Schematic";
            saveFileDialog1.FileName = editor.mainSchem.Name + ".schem";
            saveFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Schematics"; // positionner au chemin de l'appication + "Schematics"
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the File via a FileStream created by the OpenFile method.
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                SchemWriter.write(fs, editor.mainSchem, !Settings.OptimizeFileSize);
                savePath = fs.Name;
                fs.Close();
            }
        }
        /// <summary>
        /// Ouvre la fenêtre d'ouverture de fichier de windows
        /// </summary>
        private void Open()
        {
            // Création d'une fenêtre "Ouvrir un Schematic" avec Windows.Forms
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "Fichier Schematic|*.schem"; // Extensions acceptées
            openFileDialog1.Title = "Ouvrir un Schematic";
            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Schematics"; // positionner au chemin de l'appication + "Schematics"
            openFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (openFileDialog1.FileName != "")
            {
                // Open the File via a FileStream created by the OpenFile method.
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                try
                {
                    Schematic newSchem = SchemReader.Read(fs, Settings.IgnoreWarnings);
                    editor.mainSchem.Name = newSchem.Name;
                    Console.WriteLine("opened " + editor.mainSchem.Name);
                    editor.mainSchem.components = newSchem.components;
                    editor.mainSchem.Initialize(true);
                    SchematicPath[SchematicPath.Count - 1].setText(editor.mainSchem.Name);
                    savePath = fs.Name;
                }
                // Si on trouve une exception, on affiche une fenêtre d'erreur avec Windows.Forms
                catch (System.IO.FileNotFoundException fnf)
                {
                    System.Windows.Forms.MessageBox.Show(fnf.Message, "Erreur : Fichier non trouvé");
                }
                catch (SchemReader.SyntaxException sy)
                {
                    System.Windows.Forms.MessageBox.Show(sy.Message, "Erreur : Syntaxe invalide");
                }
                catch (SchemReader.StructureException stc)
                {
                    System.Windows.Forms.MessageBox.Show(stc.Message, "Erreur : structure de schematic invalide");
                }
                catch (SchemReader.MissingFieldException mf)
                {
                    System.Windows.Forms.MessageBox.Show(mf.Message, "Erreur : Champ manquant");
                }
                catch (SchemReader.InvalidValueException iv)
                {
                    System.Windows.Forms.MessageBox.Show(iv.Message, "Erreur : Valeur invalide");
                }
                catch (SchemReader.UndefinedWordWarning uw)
                {
                    System.Windows.Forms.MessageBox.Show(uw.Message, "Avertissement : Mot-clé non défini");
                }
                catch (SchemReader.InvalidTypeWarning it)
                {
                    System.Windows.Forms.MessageBox.Show(it.Message, "Avertissement : Type invalide");
                }
                catch (SchemReader.InvalidListSizeWarning ils)
                {
                    System.Windows.Forms.MessageBox.Show(ils.Message, "Avertissement : Taille de liste invalide");
                }

                fs.Close();
            }
        }
    }
}
