using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring.UI
{
    /// <summary>
    /// Représente un objet d'UI abstrait : un simple rectangle
    /// </summary>
    public abstract class UIObject
    {
        public Rectangle Bounds;
        public Point Position { get => Bounds.Location; }

        public UIObject(Rectangle? Bounds = null)
        {
            this.Bounds = Bounds == null ? Rectangle.Empty : (Rectangle)Bounds;
        }
        public UIObject(Point? Position = null)
        {
            Bounds = Position == null ? Rectangle.Empty : new Rectangle((Point)Position, Point.Zero);
        }

        public static void LoadContent(ContentManager Content)
        {
            Common.LoadContent(Content);
            Button.LoadContent(Content);
            ToolTip.LoadContent(Content);
        }

        public virtual void SetSize()
        {
        }

        public abstract void Draw(SpriteBatch spriteBatch);

        public bool Contains(Point position)
        {
            return Bounds.Contains(position);
        }

        public virtual void SetPosition(Point position)
        {
            Bounds.Location = position;
        }
    }
}
