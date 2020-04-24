using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring
{
    public static class Settings
    {
        static Rectangle Menu;
        static InputManager Inpm = new InputManager();
        static Texture2D CheckBox, CheckOn;
        public static bool IgnoreWarnings, OpenInNewBlackBox, SaveBlackBoxesInSeparateFiles, OptimizeFileSize;
        public static void Default()
        {
            IgnoreWarnings = false;
            OpenInNewBlackBox = false; // pas encore implémenté
            SaveBlackBoxesInSeparateFiles = false; // pas encore implémenté
            OptimizeFileSize = false;
        }
        public static void LoadContent(ContentManager Content)
        {
            CheckBox = Content.Load<Texture2D>("CheckBox");
            CheckOn = Content.Load<Texture2D>("CheckOn");
        }
        public static void Update(Vector2 TopRightPosition)
        {
            Inpm.Update();
            Menu = new Rectangle((int)TopRightPosition.X - 250, (int)TopRightPosition.Y, 250, 217);
            if (Menu.Contains(Inpm.MsPosition()) && Inpm.leftClic == InputManager.ClicState.Declic)
            {
                float Y = Inpm.MsPosition().Y;
                if (Menu.Y + 36 < Y && Y < Menu.Y + 36 * 2)
                {
                    IgnoreWarnings = !IgnoreWarnings;
                }
                else if (Menu.Y + 36 * 2 < Y && Y < Menu.Y + 12 + 36 * 3)
                {
                    OpenInNewBlackBox = !OpenInNewBlackBox;
                }
                else if (Menu.Y + 36 * 4 < Y && Y < Menu.Y + 36 * 5)
                {
                    SaveBlackBoxesInSeparateFiles = !SaveBlackBoxesInSeparateFiles;
                }
                else if (Menu.Y + 36 * 5 < Y && Y < Menu.Y + 36 * 6)
                {
                    OptimizeFileSize = !OptimizeFileSize;
                }
            }
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Button.tipBorder, Menu, Color.White);
            spriteBatch.Draw(Button.tipBox, new Rectangle(Menu.X + 1, Menu.Y + 1, Menu.Width - 2, Menu.Height - 2), Color.White);
            spriteBatch.DrawString(Button.font, "Paramètres d'ouverture :", new Vector2(Menu.X + 12, Menu.Y + 12), StringButton.textColor);
            spriteBatch.Draw(IgnoreWarnings ? CheckOn : CheckBox, new Vector2(Menu.X + 4, Menu.Y + 36), Color.White);
            spriteBatch.DrawString(Button.font, "Ingorer les avertissements", new Vector2(Menu.X + 30, Menu.Y + 12+36), StringButton.textColor);
            spriteBatch.Draw(OpenInNewBlackBox ? CheckOn : CheckBox, new Vector2(Menu.X + 4, Menu.Y + 36*2), Color.White);
            spriteBatch.DrawString(Button.font, "Ouvrir dans une nouvelle boîte", new Vector2(Menu.X + 30, Menu.Y + 12+36*2), StringButton.textColor);
            spriteBatch.DrawString(Button.font, "Paramètres d'enregistrement", new Vector2(Menu.X + 12, Menu.Y + 12+36*3), StringButton.textColor);
            spriteBatch.Draw(SaveBlackBoxesInSeparateFiles ? CheckOn : CheckBox, new Vector2(Menu.X + 4, Menu.Y + 36*4), Color.White);
            spriteBatch.DrawString(Button.font, "Enregister les boîtes séparement", new Vector2(Menu.X + 30, Menu.Y + 12+36*4), StringButton.textColor);
            spriteBatch.Draw(OptimizeFileSize ? CheckOn : CheckBox, new Vector2(Menu.X + 4, Menu.Y + 36*5), Color.White);
            spriteBatch.DrawString(Button.font, "Optimiser la taille du fichier", new Vector2(Menu.X + 30, Menu.Y + 12+36*5), StringButton.textColor);

            if (Menu.Contains(Inpm.MsPosition()))
            {
                float Y = Inpm.MsPosition().Y;
                if (Menu.Y + 36 < Y && Y < Menu.Y + 36*2)
                {
                    spriteBatch.Draw(Button.hoverTex, new Rectangle(Menu.X, Menu.Y + 36, Menu.Width, 36), Color.White);
                } else if (Menu.Y + 36*2 < Y && Y < Menu.Y + 12+36*3)
                {
                    spriteBatch.Draw(Button.hoverTex, new Rectangle(Menu.X, Menu.Y + 36*2, Menu.Width, 36), Color.White);
                }
                else if (Menu.Y + 36*4 < Y && Y < Menu.Y + 36*5)
                {
                    spriteBatch.Draw(Button.hoverTex, new Rectangle(Menu.X, Menu.Y + 36*4, Menu.Width, 36), Color.White);
                }
                else if (Menu.Y + 36*5 < Y && Y < Menu.Y + 36*6)
                {
                    spriteBatch.Draw(Button.hoverTex, new Rectangle(Menu.X, Menu.Y + 36*5, Menu.Width, 36), Color.White);
                }
            }
        }
    }
}
