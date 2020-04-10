using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Wiring
{
    public class Button
    {
        public static Texture2D hoverTex, toggleTex;
        public static Texture2D tipBox, tipBorder;
        public static Color tipTextColor;
        public static SpriteFont font;
        public Rectangle Bounds;
        public bool toggle;
        public String ToolTip;

        public Button(Rectangle Bounds, String ToolTip = "")
        {
            this.Bounds = Bounds;
            toggle = false;
            this.ToolTip = ToolTip;
        }
        public static void LoadContent(ContentManager Content)
        {
            hoverTex = Content.Load<Texture2D>("Button/buttonHover");
            toggleTex = Content.Load<Texture2D>("Button/buttonToggle");
            tipBox = Content.Load<Texture2D>("Button/toolTipBox");
            tipBorder = Content.Load<Texture2D>("Button/toolTipBorder");
            tipTextColor = getFirstColor(tipBorder);
            StringButton.editCursor = Content.Load<Texture2D>("EditBar");
            StringButton.textColor = getFirstColor(Content.Load<Texture2D>("Button/textColor"));
            font = Content.Load<SpriteFont>("Arial");
        }
        private static Color getFirstColor(Texture2D texture)
        {
            Color[] colors = new Color[1];
            texture.GetData<Color>(colors);
            return colors[0];
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (hover(Mouse.GetState().Position.ToVector2()))
            {
                spriteBatch.Draw(hoverTex, Bounds, Color.White);
                //tooltip
                if (ToolTip != "")
                {
                    DrawToolTip(spriteBatch, ToolTip, Bounds.Location.ToVector2() + new Vector2(Bounds.Width / 2, Bounds.Height) + new Vector2(-13, 2));
                }
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                    spriteBatch.Draw(toggleTex, Bounds, Color.White);
            }
            else if (toggle)
            {
                spriteBatch.Draw(toggleTex, Bounds, Color.White);
            }
        }
        public bool hover(Vector2 mouse)
        {
            return Bounds.Contains(mouse);
        }
        public static void DrawToolTip(SpriteBatch spriteBatch, string text, Vector2 position)
        {
            Vector2 tipSize = font.MeasureString(text) + new Vector2(4, 4);
            spriteBatch.Draw(tipBorder, new Rectangle(position.ToPoint(), (tipSize + new Vector2(2, 2)).ToPoint()), Color.White);
            spriteBatch.Draw(tipBox, new Rectangle((position + new Vector2(1, 1)).ToPoint(), tipSize.ToPoint()), Color.White);
            spriteBatch.DrawString(font, text, position + new Vector2(3, 3), tipTextColor);
        }
    }
}
