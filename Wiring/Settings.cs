using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Wiring.UI;

namespace Wiring
{
    /// <summary>
    /// Permet de gérer les paramètres (le menu "paramètres" et bientôt le fichier)
    /// </summary>
    internal static class Settings
    {
        static InputManager Inpm = new InputManager();
        static Texture2D CheckBox, CheckOn;
        public static UIList<UIList<Button>> Menu;
        public static bool IgnoreWarnings, OpenInNewBlackBox, DontSaveBlackBoxContent, OptimizeFileSize;
        // public static Color textColor, toolTipColor;

        /// <summary>
        /// Rétabis les paramètres par défaut
        /// </summary>
        public static void Default()
        {
            IgnoreWarnings = false;
            OpenInNewBlackBox = false; // pas encore implémenté
            DontSaveBlackBoxContent = false; // pas encore implémenté
            OptimizeFileSize = false;
        }
        public static void LoadContent(ContentManager Content)
        {
            CheckBox = Content.Load<Texture2D>("CheckBox");
            CheckOn = Content.Load<Texture2D>("CheckOn");

            Texture2D help = Content.Load<Texture2D>("Button/help");

            Menu = new UIList<UIList<Button>>(new Point(-250, 0), vertical: true);
            Menu.Add(new UIList<Button>());
            Menu[0].Add(new StringButton(null, "Paramètres d'ouverture :", "", false));
            Menu.Add(new UIList<Button>());
            Menu[1].Add(new TextureButton(null, CheckBox, ""));
            Menu[1].Add(new StringButton(null, "Ingorer les avertissements", "", false));
            Menu[1].Add(new TextureButton(null, help, "Améliore les performances durant l'ouverture, déconseillé si vous éditez le code de vos schematics", false));
            Menu.Add(new UIList<Button>());
            Menu[2].Add(new TextureButton(null, CheckBox, ""));
            Menu[2].Add(new StringButton(null, "Ouvrir dans une nouvelle boîte noire", "", false));
            Menu.Add(new UIList<Button>());
            Menu[3].Add(new StringButton(null, "Paramètres d'enregistrement", "", false));
            Menu.Add(new UIList<Button>());
            Menu[4].Add(new TextureButton(null, CheckBox, ""));
            Menu[4].Add(new StringButton(null, "Ne pas enregistrer le contenu des boîtes noires", "", false));
            Menu[4].Add(new TextureButton(null, help, "Il faudra les enregistrer séparément, sous leur nom actuel", false));
            Menu.Add(new UIList<Button>());
            Menu[5].Add(new TextureButton(null, CheckBox, ""));
            Menu[5].Add(new StringButton(null, "Optimiser la taille du fichier", "", false));
            Menu[5].Add(new TextureButton(null, help, "La lisibilité du code en sera réduite", false));

            Menu.SetSize();
        }
        /// <summary>
        /// Update les boutons du menu. A n'éxecuter que si celui-ci est ouvert
        /// </summary>
        public static void Update(Vector2 TopRightPosition)
        {
            Inpm.Update();
            Menu.SetPosition(new Point((int)TopRightPosition.X - Menu.Bounds.Width, (int)TopRightPosition.Y));

            Menu[1][2].toolTip.AlignRight();
            Menu[2][1].toolTip.AlignRight();
            Menu[4][2].toolTip.AlignRight();
            Menu[5][2].toolTip.AlignRight();

            if (Menu.Contains(Inpm.MsState.Position) && Inpm.leftClic == InputManager.ClicState.Clic)
            {
                for (int i = 0; i < Menu.Count; i++)
                {
                    if (Menu[i][0].Contains(Inpm.MsState.Position))
                    {
                        switch (i) {
                            case 1:
                                IgnoreWarnings = !IgnoreWarnings;
                                ((TextureButton)Menu[i][0]).SetTexture(IgnoreWarnings ? CheckOn : CheckBox);
                                break;
                            case 2:
                                OpenInNewBlackBox = !OpenInNewBlackBox;
                                ((TextureButton)Menu[i][0]).SetTexture(OpenInNewBlackBox ? CheckOn : CheckBox);
                                break;
                            case 4:
                                DontSaveBlackBoxContent = !DontSaveBlackBoxContent;
                                ((TextureButton)Menu[i][0]).SetTexture(DontSaveBlackBoxContent ? CheckOn : CheckBox);
                                break;
                            case 5:
                                OptimizeFileSize = !OptimizeFileSize;
                                ((TextureButton)Menu[i][0]).SetTexture(OptimizeFileSize ? CheckOn : CheckBox);
                                break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Dessine le menu des paramètres. A n'éxecuter que si celui-ci est ouvert
        /// </summary>
        /// <param name="spriteBatch"></param>
        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ToolTip.tipBorder, Menu.Bounds, Color.White);
            spriteBatch.Draw(ToolTip.tipBox, new Rectangle(Menu.Bounds.X + 1, Menu.Bounds.Y + 1, Menu.Bounds.Width - 2, Menu.Bounds.Height - 2), Color.White);

            Menu.Draw(spriteBatch);

            foreach (UIList<Button> l in Menu.list)
                foreach (Button b in l.list)
                    b.DrawToolTip(spriteBatch);
        }
    }
}
