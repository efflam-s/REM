using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring.UI
{
    public class ToolTip
    {
        public string Text;
        public Point Position;
        public ToolTip(string Text = "", Point? Position = null)
        {
            this.Text = Text;
            this.Position = Position == null ? Point.Zero : (Point)Position;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawToolTip(spriteBatch, Text, Position);
        }

        public static Texture2D tipBox, tipBorder;
        public static Color tipTextColor;
        public static SpriteFont font;
        public static void LoadContent(ContentManager Content)
        {
            tipBox = Content.Load<Texture2D>("Button/toolTipBox");
            tipBorder = Content.Load<Texture2D>("Button/toolTipBorder");
            tipTextColor = Button.getFirstColor(tipBorder);
            font = Content.Load<SpriteFont>("Arial");
        }
        public void AlignRight()
        {
            Vector2 tipSize = font.MeasureString(Text);
            Position.X -= (int)tipSize.X - 13;
        }

        public static void DrawToolTip(SpriteBatch spriteBatch, string text, Point position)
        {
            if (text != "")
            {
                Vector2 tipSize = font.MeasureString(text) + new Vector2(4, 4);
                spriteBatch.Draw(tipBorder, new Rectangle(position, (tipSize + new Vector2(2, 2)).ToPoint()), Color.White);
                spriteBatch.Draw(tipBox, new Rectangle((position + new Point(1, 1)), tipSize.ToPoint()), Color.White);
                spriteBatch.DrawString(font, text, position.ToVector2() + new Vector2(3, 3), tipTextColor);
            }
        }
    }
}
