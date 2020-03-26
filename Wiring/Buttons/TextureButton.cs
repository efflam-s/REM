﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Wiring
{
    public class TextureButton : Button
    {
        private Texture2D texture;
        public TextureButton(Vector2 position, String ToolTip = "") : base (Rectangle.Empty, ToolTip)
        {
            Bounds.Location = position.ToPoint();
        }
        public void setTexture(Texture2D texture)
        {
            this.texture = texture;
            Bounds.Size = texture.Bounds.Size;
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