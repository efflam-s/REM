using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Wiring
{
    public class StringButton : Button
    {
        public static Texture2D editCursor;
        public static Color textColor;
        public String text { get; private set; }
        public StringButton(Vector2 position, String text, String ToolTip = "") : base(Rectangle.Empty, ToolTip)
        {
            Bounds.Location = position.ToPoint();
            this.text = text;
            Bounds.Size = font.MeasureString(text).ToPoint() + new Point(12, 8);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(font, text, Bounds.Location.ToVector2() + new Vector2(6, 4), textColor);
        }
        public void setText(string newText)
        {
            text = newText;
            Bounds.Size = font.MeasureString(text).ToPoint() + new Point(16, 8);
        }
        public void DrawEditCursor(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (gameTime.TotalGameTime.Milliseconds < 500)
            {
                spriteBatch.Draw(editCursor, new Vector2(Bounds.Right - 5 - editCursor.Width, Bounds.Top + 3), Color.White);
            }
        }
    }
}
