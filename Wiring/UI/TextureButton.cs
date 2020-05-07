using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Wiring.UI
{
    /// <summary>
    /// Comme un bouton, mais avec une texture
    /// </summary>
    public class TextureButton : Button
    {
        private Texture2D texture;
        public TextureButton(Point? position, string ToolTip = "", bool drawHover = true) : base (position, ToolTip, drawHover)
        {
        }
        public TextureButton(Point? position, Texture2D texture, string ToolTip = "", bool drawHover = true) : base(position, ToolTip, drawHover)
        {
            SetTexture(texture);
        }
        public void SetTexture(Texture2D texture)
        {
            this.texture = texture;
            SetSize();
        }
        public override void SetSize()
        {
            Bounds.Size = texture.Bounds.Size;
            base.SetSize();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Draw(texture, Bounds, Color.White);
        }
        /*public override bool hover(Vector2 mouse)
        {
            return position.X - texture.Width / 2 < mouse.X && position.Y - texture.Height / 2 < mouse.Y
                && mouse.X < position.X + texture.Width / 2 && mouse.Y < position.Y + texture.Height / 2;
        }*/
    }
}
