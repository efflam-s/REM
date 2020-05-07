using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Wiring.UI
{
    /// <summary>
    /// Crée une liste d'UIObject avec ou non des séparateurs
    /// </summary>
    public class UIList<T> : UIObject where T : UIObject
    {
        public List<T> list { get; private set; }
        public int Count => list.Count;
        public void Add(T obj) => list.Add(obj);
        public T this[int i] => list[i];
        Separator separator;
        public UIList(Point? position = null, Separator separator = Separator.None) : base(position)
        {
            list = new List<T>();
            this.separator = separator;
        }

        public override void SetSize()
        {
            int separatorWidth = 4, separatorHeight = 0;
            if (separator == Separator.Line) { separatorWidth = 7; separatorHeight = 32; }
            if (separator == Separator.Arrow) { separatorWidth = 8; separatorHeight = 24; }
            Bounds.Height = separatorHeight;
            Bounds.Width = 0;
            for (int i = 0; i < list.Count; i++)
            {
                T o = list[i];
                o.SetSize();
                if (Bounds.Height < o.Bounds.Height)
                    Bounds.Height = o.Bounds.Height;
            }
            for (int i = 0; i < list.Count; i++)
            {
                T o = list[i];
                if (i != 0) Bounds.Width += separatorWidth;
                o.SetPosition(new Point(Bounds.X + Bounds.Width, Bounds.Y + (Bounds.Height - o.Bounds.Height) / 2));

                // recursion sur les sous-listes
                /*if (o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition() == typeof(UIList<>)) {
                    ((UIList<UIObject>)o).ResetChildsPosition();
                }*/

                Bounds.Width += o.Bounds.Width;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T o = list[i];
                if (i != 0)
                {
                    switch (separator)
                    {
                        case Separator.Line:
                            spriteBatch.Draw(Common.LineSeparator, new Vector2(o.Position.X - 6,  Position.Y - 2), Color.White);
                            break;
                        case Separator.Arrow:
                            spriteBatch.Draw(Common.ArrowSeparator, new Vector2(o.Position.X - 8, Position.Y - 2), Color.White);
                            break;
                    }
                }
                o.Draw(spriteBatch);
            }
        }
        public override void SetPosition(Point position)
        {
            Point oldPosition = Bounds.Location;
            base.SetPosition(position);
            foreach (T o in list)
            {
                o.SetPosition(o.Bounds.Location - oldPosition + position);
            }
        }
    }
}
