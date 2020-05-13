﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace REM.Wiring
{
    /// <summary>
    /// Les fils relient les composant entre eux selon une logique de porte "ou"
    /// </summary>
    public class Wire
    {
        public static Texture2D wOn, wOff, nodeOn, nodeOff;
        public bool value { get; private set; }
        public List<Component> components; // List of connected components
        public Vector2 Node;
        public Wire()
        {
            components = new List<Component>();
            value = false;
            Node = Vector2.Zero;
        }
        public void ResetNodePosition()
        {
            Node = Vector2.Zero;
            foreach (Component c in components)
                Node += c.plugPosition(this);
            Node /= components.Count;
            Node.X = (int)Node.X; Node.Y = (int)Node.Y;
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
            // On récupère la valeur : porte "ou"
            value = components.Any(x => x.GetOutput(this));

            if (oldValue != value)
            {
                // Si la valeur a changé, on update tout les composants connectés
                foreach (Component c in components)
                    c.Update();
                return true;
            }
            return false;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (components.Count() > 1)
            {
                foreach (Component c in components)
                    drawLine(spriteBatch, Node, c.plugPosition(this), value);
                if (components.Count() > 2)
                    DrawNode(spriteBatch);
            }
        }
        public void DrawNode(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(value ? nodeOn : nodeOff, Node - new Vector2(nodeOn.Width, nodeOn.Height) / 2, Color.White);
        }

        public bool touchWire(Vector2 position)
        {
            if (touchNode(position))
                return true;

            if (components.Count() > 1)
            {
                foreach (Component c in components)
                {
                    if (touchLine(position, Node, c.plugPosition(this)))
                        return true;
                }
            }
            return false;
        }
        public bool touchNode(Vector2 position)
        {
            return Vector2.Distance(Node, position) <= 2;
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
        /// <summary>
        /// Détermine si une position touche une ligne d'épaisseur 2 et définie par les points A et B
        /// </summary>
        public static bool touchLine(Vector2 position, Vector2 A, Vector2 B)
        {
            int strokeWeight = 2;
            Vector2 ab = B - A;
            float angle = (float)Math.Atan2(ab.Y, ab.X);
            Vector2 relativ = position - A;
            relativ = new Vector2(relativ.X * (float)Math.Cos(angle) + relativ.Y * (float)Math.Sin(angle), relativ.Y * (float)Math.Cos(angle) - relativ.X * (float)Math.Sin(angle));
            return -strokeWeight/2 <= relativ.Y && relativ.Y <= strokeWeight/2 && 0 <= relativ.X && relativ.X <= ab.Length();
        }
    }
}
