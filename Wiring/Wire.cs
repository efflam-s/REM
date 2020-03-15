using System;
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
                drawLine(spriteBatch, components[0].plugPosition(this), components[1].plugPosition(this));
            }
            else if (components.Count() > 2)
            {
                Vector2 center = Vector2.Zero;
                foreach (Component c in components)
                    center += c.plugPosition(this);
                center /= components.Count;
                foreach (Component c in components)
                    drawLine(spriteBatch, center, c.plugPosition(this));
                // TODO : add nodes
            }
        }
        private void drawLine(SpriteBatch spriteBatch, Vector2 A, Vector2 B)
        {
            int strokeWeight = 2;
            Vector2 ab = B - A;
            float angle = (float)Math.Atan2(ab.Y, ab.X);
            spriteBatch.Draw(value ? wOn : wOff,
                new Rectangle((int)A.X, (int)A.Y, (int)ab.Length(), strokeWeight),
                null, Color.White, angle, new Vector2(0, 0.5f), SpriteEffects.None, 0);
        }
        public bool touchWire(Vector2 position)
        {
            if (components.Count() == 2)
            {
                Vector2 vectAB = components[1].plugPosition(this) - components[0].plugPosition(this);
                Vector2 normal = Vector2.Normalize(vectAB);
                Vector2 center = (components[1].plugPosition(this) + components[0].plugPosition(this)) / 2;
                Vector2 relativ = position - center;
                return Math.Abs(Vector2.Dot(relativ, normal)) <= vectAB.Length() / 2 && Math.Abs(Vector2.Dot(relativ, new Vector2(normal.Y, -normal.X))) <= 1;
            }
            else if (components.Count() > 2)
            {
                Vector2 node = Vector2.Zero;
                foreach (Component c in components)
                    node += c.plugPosition(this);
                node /= components.Count;
                foreach (Component c in components)
                {
                    Vector2 vectAC = node - components[0].plugPosition(this);
                    Vector2 normal = Vector2.Normalize(vectAC);
                    Vector2 center = (components[1].plugPosition(this) + components[0].plugPosition(this)) / 2;
                    Vector2 relativ = position - center;
                    if (Math.Abs(Vector2.Dot(relativ, normal)) <= vectAC.Length() / 2 && Math.Abs(Vector2.Dot(relativ, new Vector2(normal.Y, -normal.X))) <= 1)
                        return true;
                }
                return false;
            }
            return false;
        }
    }
}
