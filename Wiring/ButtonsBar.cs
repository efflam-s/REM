using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring
{
    public class ButtonsBar
    {
        private static Texture2D buttonsBar, separator;

        private List<Button>[] Buttons;
        public float Y;

        public List<Button> this[int i] { get => Buttons[i]; }
        public Button this[int i, int j] { get => Buttons[i][j]; }
        public void ForEach(Action<Button> action)
        {
            for (int i = 0; i < Buttons.Length; i++)
                foreach (Button b in Buttons[i])
                    action(b);
        }

        public ButtonsBar(float Y, int ListNumber)
        {
            this.Y = Y;
            Buttons = new List<Button>[ListNumber];
            for (int i = 0; i < ListNumber; i++)
            {
                Buttons[i] = new List<Button>();
            }
        }
        public static void LoadContent(ContentManager Content)
        {
            buttonsBar = Content.Load<Texture2D>("Button/buttonsBar");
            separator = Content.Load<Texture2D>("Button/separator");
        }
        public static ButtonsBar firstBar(ContentManager Content)
        {
            ButtonsBar bar = new ButtonsBar(0, 3);
            bar.Buttons[0].Add(new TextureButton(bar.lastPosition(0), Content.Load<Texture2D>("Button/arrow"), "Sélectionner/Modifier (S)"));
            bar.Buttons[1].Add(new TextureButton(bar.lastPosition(1), Content.Load<Texture2D>("Button/addWireButton"), "Ajouter une Connexion (C)"));
            bar.Buttons[1].Add(new TextureButton(bar.lastPosition(1), Content.Load<Texture2D>("Button/addInputButton"), "Ajouter une Entrée"));
            bar.Buttons[1].Add(new TextureButton(bar.lastPosition(1), Content.Load<Texture2D>("Button/addOutputButton"), "Ajouter une Sortie"));
            bar.Buttons[1].Add(new TextureButton(bar.lastPosition(1), Content.Load<Texture2D>("Button/addNotButton"), "Ajouter un Inverseur"));
            bar.Buttons[1].Add(new TextureButton(bar.lastPosition(1), Content.Load<Texture2D>("Button/addDiodeButton"), "Ajouter une Diode"));
            bar.Buttons[1].Add(new TextureButton(bar.lastPosition(1), Content.Load<Texture2D>("Button/addBlackBoxButton"), "Ajouter une Boîte Noire"));
            bar.Buttons[2].Add(new TextureButton(bar.lastPosition(2), Content.Load<Texture2D>("Button/hand"), "Panoramique (H)"));
            bar.Buttons[2].Add(new TextureButton(bar.lastPosition(2), Content.Load<Texture2D>("Button/zoom"), "Zoom (Z)"));
            return bar;
        }
        public static ButtonsBar secondBar(ContentManager Content)
        {
            ButtonsBar bar = new ButtonsBar(36, 3);
            bar.Buttons[0].Add(new TextureButton(bar.lastPosition(0), Content.Load<Texture2D>("Button/open"), "Ouvrir (Ctrl + O)"));
            bar.Buttons[0].Add(new TextureButton(bar.lastPosition(0), Content.Load<Texture2D>("Button/save"), "Enregistrer (Ctrl + S)"));
            bar.Buttons[0].Add(new TextureButton(bar.lastPosition(0), Content.Load<Texture2D>("Button/saveAs"), "Enregistrer Sous (Ctrl + Maj + S)"));
            bar.Buttons[1].Add(new TextureButton(bar.lastPosition(1), Content.Load<Texture2D>("Button/arrowUp"), "Retour (Echap)"));
            bar.Buttons[2].Add(new Button(new Rectangle(bar.lastPosition(2).ToPoint(), Point.Zero)));
            return bar;
        }
        private Vector2 lastPosition(int list)
        {
            int lastPosition = 4;
            if (Buttons[list].Count > 0)
            {
                lastPosition = Buttons[list][Buttons[list].Count - 1].Bounds.Right + 4;
            }
            else if (Buttons.Length > 0)
            {
                for (int i = 0; i < list; i++)
                {
                    if (Buttons[i].Count > 0)
                        lastPosition = Buttons[i][Buttons[i].Count - 1].Bounds.Right + 7;
                }
            }
            return new Vector2(lastPosition, Y + 2);
        }

        public void Draw(SpriteBatch spriteBatch, int WindowWidth)
        {
            // draw bar
            spriteBatch.Draw(buttonsBar, new Rectangle(0, (int)Y, WindowWidth, buttonsBar.Height), Color.White);
            // draw buttons and separators
            for (int i = 0; i < Buttons.Length; i++)
            {
                foreach (Button b in Buttons[i])
                {
                    b.Draw(spriteBatch);
                }
                if (i != 0 && Buttons[i].Count > 0)
                {
                    spriteBatch.Draw(separator, Buttons[i][0].Bounds.Location.ToVector2() + new Vector2(-6, -2), Color.White);
                }
            }
            // draw tooltips
            /*for (int i = 0; i < Buttons.Length; i++)
            {
                foreach (Button b in Buttons[i])
                    b.DrawToolTip(spriteBatch);
            }*/
            ForEach(b => b.DrawToolTip(spriteBatch));
        }
    }
}
