﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring
{
    /// <summary>
    /// Les fils relient les composant entre eux selon une logique de porte "ou"
    /// </summary>
    public class Wire
    {
        public static Texture2D wOn, wOff;
        public bool value { get; private set; }
        public List<Component> components;
        public Wire()
        {
            components = new List<Component>();
            value = false;
        }
        public static void LoadContent(ContentManager Content)
        {
            wOn = Content.Load<Texture2D>("wireOn");
            wOff = Content.Load<Texture2D>("wireOff");
        }

        public void Update()
        {
            bool oldValue = value;
            value = false;
            foreach (Component c in components)
            {
                if (c.GetOutput(this))
                    value = true;
            }
            if (oldValue != value)
            {
                foreach (Component c in components)
                {
                    c.Update();
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (components.Count() == 2)
            {
                drawLine(components[0].plugPosition(this), components[1].plugPosition(this));
            }
            else if (components.Count() > 2)
            {
                Vector2 center = Vector2.Zero;
                foreach (Component c in components)
                    center += c.plugPosition(this);
                center /= components.Count;
                foreach (Component c in components)
                    drawLine(center, c.plugPosition(this));
            }

            void drawLine(Vector2 A, Vector2 B)
            {
                int strokeWeight = 2;
                Vector2 ab = B - A;
                float angle = (float)Math.Atan2(ab.Y, ab.X);
                spriteBatch.Draw(value ? wOn : wOff,
                    new Rectangle((int)A.X, (int)A.Y, (int)ab.Length(), strokeWeight),
                    null, Color.White, angle, new Vector2(0, 0.5f), SpriteEffects.None, 0);
            }
        }
    }
}
