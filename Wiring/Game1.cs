using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wiring.Wiring;
using Wiring.UI;

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
        UIList<UIList<Button>> firstBar, secondBar;
        TextureButton[] MiscButtons; // boutons qu'on ne peut pas mettre sur les barres (pour l'instant seulement paramètres)

        Texture2D buttonsBar; // textures pour les barres de boutons
        SpriteFont font;
        StringBuilder builder; // Permet de construire des strings à partir de l'input de l'utilisateur (voir Window_TextInput)
        bool isListeningToInputText;
        string savePath; // Chemin sur le disque dans lequel on enregistre le schematic courant

        // "alias" pour accéder aux groupes de boutons plus facilement
        UIList<Button> SchematicPath => secondBar[2];
        StringButton LastSchemPathButton => (StringButton)SchematicPath[SchematicPath.Count - 1];

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Window.TextInput += Window_TextInput; // évenements de textInput pour le renommage
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
            //TargetElapsedTime = TimeSpan.FromSeconds(0.25);
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
            MiscButtons = new TextureButton[1];
            MiscButtons[0] = new TextureButton(new Point(0, 38)); // paramètres

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
            UIObject.LoadContent(Content);
            Settings.LoadContent(Content);

            buttonsBar = Content.Load<Texture2D>("Button/buttonsBar");
            firstBar = getFirstBar(Content);
            secondBar = getSecondBar(Content);
            MiscButtons[0].SetTexture(Content.Load<Texture2D>("Button/Settings"));

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
                Point newPosition = new Point(LastSchemPathButton.Bounds.Right, LastSchemPathButton.Bounds.Top) + new Point(8, 0);
                LastSchemPathButton.toolTip.Text = "";
                secondBar[2].Add(new StringButton(newPosition, editor.mainSchem.Name, "Renommer"));
                secondBar[2].SetSize();
                savePath = "";
            }

            if (Inpm.OnPressed(Keys.Escape) && !isListeningToInputText)
            {
                // Retour avec Echap
                Back();
            }

            UpdateButtons();
            

            if (Inpm.Control && !Inpm.Alt && !Inpm.Shift && Inpm.OnPressed(Keys.R))
            {
                // Ctrl+R : renommer le schematic actuel
                isListeningToInputText = true;
                LastSchemPathButton.toggle = true;
                builder = new StringBuilder(LastSchemPathButton.text);
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
            spriteBatch.Draw(buttonsBar, new Rectangle(0, 0, Window.ClientBounds.Width, buttonsBar.Height), Color.White);
            spriteBatch.Draw(buttonsBar, new Rectangle(0, buttonsBar.Height, Window.ClientBounds.Width, buttonsBar.Height), Color.White);
            secondBar.Draw(spriteBatch);
            firstBar.Draw(spriteBatch);

            if (isListeningToInputText)
            {
                LastSchemPathButton.DrawEditCursor(spriteBatch, gameTime);
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

            // tooltip des barres
            foreach (UIList<Button> l in firstBar.list)
                foreach (Button b in l.list)
                    b.DrawToolTip(spriteBatch);
            foreach (UIList<Button> l in secondBar.list)
                foreach (Button b in l.list)
                    b.DrawToolTip(spriteBatch);

            // Blackbox tooltip
            if (editor.tool == Editor.Tool.Edit || editor.tool == Editor.Tool.Wire)
            {
                if (editor.currentHoveredComponent() is BlackBox bb)
                {
                    ToolTip.DrawToolTip(spriteBatch, bb.schem.Name, Inpm.MsState.Position + new Point(0, 25));
                }
            }

            // Menu de paramètres
            if (MiscButtons[0].toggle) Settings.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Mise à jour les barres de boutons et les autres boutons
        /// </summary>
        void UpdateButtons()
        {
            bool declic = Inpm.leftClic == InputManager.ClicState.Declic;
            // Organisation des boutons pour faciliter les boucles
            Button[] AddButtons = { firstBar[1][1], firstBar[1][2], firstBar[1][3], firstBar[1][4], firstBar[1][5] };
            Button[] ToolButtons = { firstBar[0][0], firstBar[1][0], firstBar[2][0], firstBar[2][1] };
            Button[] FileButtons = secondBar[0].list.ToArray();
            Button Back = secondBar[1][0];

            // Boutons de création de composants
            for (int i = 0; i < AddButtons.Length; i++)
            {
                if (AddButtons[i].toggle && (declic || editor.tool != Editor.Tool.Move))
                {
                    // Détoggle dans le cas où on clique n'importe où ou si l'outil n'est pas le déplacement
                    AddButtons[i].toggle = false;
                }
                if (AddButtons[i].Contains(Inpm.MsState.Position) && declic && AddButtons[i].Contains(Inpm.mousePositionOnClic.ToPoint()))
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
                if (ToolButtons[i].Contains(Inpm.MsState.Position) && declic)
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
                if (FileButtons[i].Contains(Inpm.MsState.Position) && declic)
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
            if (Back.Contains(Inpm.MsState.Position) && declic)
            {
                this.Back();
            }

            // Gestion boutons de navigation
            for (int i = 0; i < SchematicPath.Count; i++)
            {
                if (SchematicPath[i].Contains(Inpm.MsState.Position) && Inpm.leftClic == InputManager.ClicState.Declic)
                {
                    if (i == SchematicPath.Count - 1)
                    {
                        // Schematic courant => renommage
                        isListeningToInputText = true;
                        SchematicPath[i].toggle = true;
                        builder = new StringBuilder(((StringButton)SchematicPath[i]).text);
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
                LastSchemPathButton.toggle = false;
                if (LastSchemPathButton.text == "")
                    // Chaîne vide => on garde le nom précedent
                    LastSchemPathButton.SetText(editor.mainSchem.Name);
                else
                    // Sinon on change le nom
                    editor.mainSchem.Name = LastSchemPathButton.text;
            }

            // Bouton paramètres
            if (declic)
                if (MiscButtons[0].Contains(Inpm.MsState.Position))
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
                    if (LastSchemPathButton.text.Length == 0)
                        LastSchemPathButton.SetText(editor.mainSchem.Name);
                    else
                        editor.mainSchem.Name = LastSchemPathButton.text;
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
                        LastSchemPathButton.SetText(builder.ToString());
                    } catch (ArgumentException) 
                    {
                        // caractère non valide : ctrl + qqch, echap...
                        builder.Remove(builder.Length - 1, 1);
                        LastSchemPathButton.SetText(builder.ToString());
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
            string lastBlackBoxName = editor.schemPile[id + 1].Name;
            editor.schemPile.RemoveRange(id + 1, editor.schemPile.Count - id - 1);
            SchematicPath.list.RemoveRange(id + 1, SchematicPath.Count - id - 1);
            // Set tooltips
            LastSchemPathButton.toolTip.Text = "Renommer";
            // Reload plugs, selection and wires
            foreach (Component c in editor.mainSchem.components)
                if (c is BlackBox bb && bb.schem.Name == lastBlackBoxName)
                {
                    bb.ReloadPlugsFromSchematic(true);
                    bb.Update();
                }
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
                // Demander la création d'un schematic parent
                string[] buttons = { "Ok", "Annuler" };
                int? result = MessageBox.Show("Créer une boîte noire ?", "Voulez-vous encapsuler le schematic actuel ?", buttons).Result;
                if (result == 0)
                {
                    Console.WriteLine("Création d'un nouveau schematic...");
                    Schematic old = editor.mainSchem;
                    editor.mainSchem = new Schematic("Schematic");
                    editor.mainSchem.AddComponent(new BlackBox(old, new Vector2(50, 50)));
                    LastSchemPathButton.SetText(editor.mainSchem.Name);
                }
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
                SchemWriter.write(savePath, editor.mainSchem, !Settings.OptimizeFileSize, Settings.DontSaveBlackBoxContent);
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
                SchemWriter.write(fs, editor.mainSchem, !Settings.OptimizeFileSize, Settings.DontSaveBlackBoxContent);
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
            openFileDialog1.InitialDirectory = savePath == "" ? AppDomain.CurrentDomain.BaseDirectory + "Schematics" : savePath; // positionner au chemin de l'appication + "Schematics"
            openFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (openFileDialog1.FileName != "")
            {
                // Bouton utilisé pour les messages d'erreur
                string[] buttons = { "Ok" };
                // Open the File via a FileStream created by the OpenFile method.
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                try
                {
                    Schematic newSchem = SchemReader.Read(fs, Settings.IgnoreWarnings);
                    newSchem.Initialize(true);
                    if (Settings.OpenInNewBlackBox)
                    {
                        BlackBox newBB = new BlackBox(newSchem, Vector2.Zero);
                        editor.AddComponent(newBB);
                    }
                    else
                    {
                        editor.mainSchem.Name = newSchem.Name;
                        editor.mainSchem.components = newSchem.components;
                        editor.mainSchem.Initialize(true);
                        LastSchemPathButton.SetText(editor.mainSchem.Name);
                    }
                    savePath = fs.Name;
                }
                // Si on trouve une exception, on affiche une fenêtre d'erreur avec Windows.Forms
                catch (System.IO.FileNotFoundException fnf)
                {
                    MessageBox.Show("Erreur : Fichier non trouvé", fnf.Message, buttons);
                }
                catch (SchemReader.SyntaxException sy)
                {
                    MessageBox.Show("Erreur : Syntaxe invalide", sy.Message, buttons);
                }
                catch (SchemReader.StructureException stc)
                {
                    MessageBox.Show("Erreur : structure de schematic invalide", stc.Message, buttons);
                }
                catch (SchemReader.MissingFieldException mf)
                {
                    MessageBox.Show("Erreur : Champ manquant", mf.Message, buttons);
                }
                catch (SchemReader.InvalidValueException iv)
                {
                    MessageBox.Show("Erreur : Valeur invalide", iv.Message, buttons);
                }
                catch (SchemReader.UndefinedWordWarning uw)
                {
                    MessageBox.Show("Avertissement : Mot-clé non défini", uw.Message, buttons);
                }
                catch (SchemReader.InvalidTypeWarning it)
                {
                    MessageBox.Show("Avertissement : Type invalide", it.Message, buttons);
                }
                catch (SchemReader.InvalidListSizeWarning ils)
                {
                    MessageBox.Show("Avertissement : Taille de liste invalide", ils.Message, buttons);
                }

                fs.Close();
            }
        }

        public UIList<UIList<Button>> getFirstBar(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            UIList<UIList<Button>> bar = new UIList<UIList<Button>>(new Point(4, 2), Separator.Line);
            bar.Add(new UIList<Button>());
            bar[0].Add(new TextureButton(null, Content.Load<Texture2D>("Button/arrow"), "Sélectionner/Modifier (S)"));
            bar.Add(new UIList<Button>());
            bar[1].Add(new TextureButton(null, Content.Load<Texture2D>("Button/addWireButton"), "Ajouter une Connexion (C)"));
            bar[1].Add(new TextureButton(null, Content.Load<Texture2D>("Button/addInputButton"), "Ajouter une Entrée"));
            bar[1].Add(new TextureButton(null, Content.Load<Texture2D>("Button/addOutputButton"), "Ajouter une Sortie"));
            bar[1].Add(new TextureButton(null, Content.Load<Texture2D>("Button/addNotButton"), "Ajouter un Inverseur"));
            bar[1].Add(new TextureButton(null, Content.Load<Texture2D>("Button/addDiodeButton"), "Ajouter une Diode"));
            bar[1].Add(new TextureButton(null, Content.Load<Texture2D>("Button/addBlackBoxButton"), "Ajouter une Boîte Noire"));
            bar.Add(new UIList<Button>());
            bar[2].Add(new TextureButton(null, Content.Load<Texture2D>("Button/hand"), "Panoramique (H)"));
            bar[2].Add(new TextureButton(null, Content.Load<Texture2D>("Button/zoom"), "Zoom (Z)"));

            bar.SetSize();
            return bar;
        }
        public UIList<UIList<Button>> getSecondBar(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            UIList<UIList<Button>> bar = new UIList<UIList<Button>>(new Point(4, buttonsBar.Height + 2), Separator.Line);
            bar.Add(new UIList<Button>());
            bar[0].Add(new TextureButton(null, Content.Load<Texture2D>("Button/open"), "Ouvrir (Ctrl + O)"));
            bar[0].Add(new TextureButton(null, Content.Load<Texture2D>("Button/save"), "Enregistrer (Ctrl + S)"));
            bar[0].Add(new TextureButton(null, Content.Load<Texture2D>("Button/saveAs"), "Enregistrer Sous (Ctrl + Maj + S)"));
            bar.Add(new UIList<Button>());
            bar[1].Add(new TextureButton(null, Content.Load<Texture2D>("Button/arrowUp"), "Retour (Echap)"));
            bar.Add(new UIList<Button>(null, Separator.Arrow));
            bar[2].Add(new StringButton(null, editor.mainSchem.Name, "Renommer"));

            bar.SetSize();
            return bar;
        }
    }
}
