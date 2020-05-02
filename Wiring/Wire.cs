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
        public static Texture2D wOn, wOff, nodeOn, nodeOff;
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
            nodeOn = Content.Load<Texture2D>("wireNodeOn");
            nodeOff = Content.Load<Texture2D>("wireNodeOff");
        }

        public bool Update()
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
                return true;
            }
            return false;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (components.Count() == 2)
            {
                // fil simple
                drawLine(spriteBatch, components[0].plugPosition(this), components[1].plugPosition(this), value);
            }
            else if (components.Count() > 2)
            {
                // fil avec noeud
                Vector2 center = Vector2.Zero;
                foreach (Component c in components)
                    center += c.plugPosition(this);
                center /= components.Count;
                center.X = (int)center.X; center.Y = (int)center.Y;
                foreach (Component c in components)
                    drawLine(spriteBatch, center, c.plugPosition(this), value);
                spriteBatch.Draw(value ? nodeOn : nodeOff, center - new Vector2(nodeOn.Width, nodeOn.Height)/2, Color.White);
            }
        }
        public static void drawLine(SpriteBatch spriteBatch, Vector2 A, Vector2 B, bool value)
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
                return touchWire(position, components[1].plugPosition(this), components[0].plugPosition(this));
            }
            else if (components.Count() > 2)
            {
                Vector2 node = Node();
                foreach (Component c in components)
                {
                    if (touchWire(position, node, c.plugPosition(this)))
                        return true;
                }
                return false;
            }
            return false;
        }
        public static bool touchWire(Vector2 position, Vector2 plugA, Vector2 plugB)
        {
            Vector2 vectAB = plugB - plugA;
            Vector2 normal = Vector2.Normalize(vectAB);
            Vector2 center = (plugB + plugA) / 2;
            Vector2 relativ = position - center;
            return Math.Abs(Vector2.Dot(relativ, normal)) <= vectAB.Length() / 2 && Math.Abs(Vector2.Dot(relativ, new Vector2(normal.Y, -normal.X))) <= 1;
        }
        public Vector2 Node()
        {
            Vector2 node = Vector2.Zero;
            foreach (Component c in components)
                node += c.plugPosition(this);
            return node /= components.Count;
        }
    }
}
