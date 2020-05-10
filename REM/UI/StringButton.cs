using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace REM.UI
{
    /// <summary>
    /// Comme un bouton, mais avec un texte dessus
    /// </summary>
    public class StringButton : Button
    {
        public static Texture2D editCursor;
        public static Color textColor;
        public static SpriteFont font;

        public string text { get; private set; }
        public StringButton(Point? position, string text, string ToolTip = "", bool drawHover = true) : base(position, ToolTip, drawHover)
        {
            this.text = text;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(font, text, Bounds.Location.ToVector2() + new Vector2(6, 4), textColor);
        }
        public void SetText(string newText)
        {
            text = newText;
            SetSize();
        }
        public override void SetSize()
        {
            Bounds.Size = font.MeasureString(text).ToPoint() + new Point(16, 8);
            base.SetSize();
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
