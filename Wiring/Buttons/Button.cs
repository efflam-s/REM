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
        public static SpriteFont font;
        private Texture2D texture;
        private Vector2 position;
        public bool toggle;
        public String ToolTip;

        public Button(Vector2 position, String ToolTip = "")
        {
            this.position = position;
            toggle = false;
            this.ToolTip = ToolTip;
        }
        public static void LoadContent(ContentManager Content)
        {
            hoverTex = Content.Load<Texture2D>("Button/buttonHover");
            toggleTex = Content.Load<Texture2D>("Button/buttonToggle");
            font = Content.Load<SpriteFont>("Arial");
        }
        public void setTexture(Texture2D texture)
        {
            this.texture = texture;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
            if (hover(Mouse.GetState().Position.ToVector2()))
            {
                spriteBatch.Draw(hoverTex, position - new Vector2(hoverTex.Width, hoverTex.Height) / 2, Color.White);
                //tooltip
                spriteBatch.DrawString(font, ToolTip, position + new Vector2(0, texture.Height) / 2, Color.Black);
            }
            else if (toggle)
            {
                spriteBatch.Draw(toggleTex, position - new Vector2(toggleTex.Width, toggleTex.Height) / 2, Color.White);
            }
        }
        public bool hover(Vector2 mouse)
        {
            return position.X - texture.Width / 2 < mouse.X && position.Y - texture.Height / 2 < mouse.Y
                && mouse.X < position.X + texture.Width / 2 && mouse.Y < position.Y + texture.Height / 2;
        }
    }
}
