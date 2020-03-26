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
    public class StringButton : Button
    {
        public String text { get; private set; }
        public StringButton(Vector2 position, String text, String ToolTip = "") : base(Rectangle.Empty, ToolTip)
        {
            Bounds.Location = position.ToPoint();
            this.text = text;
            Bounds.Size = font.MeasureString(text).ToPoint() + new Point(16, 8);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(font, text, Bounds.Location.ToVector2() + new Vector2(8, 4), Color.Black);
        }
        public void setText(string newText)
        {
            text = newText;
            Bounds.Size = font.MeasureString(text).ToPoint() + new Point(16, 8);
        }
    }
}
