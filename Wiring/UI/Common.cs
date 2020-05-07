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
    internal class Common
    {
        internal static Texture2D LineSeparator, ArrowSeparator;

        internal static void LoadContent(ContentManager Content)
        {
            LineSeparator = Content.Load<Texture2D>("Button/separator");
            ArrowSeparator = Content.Load<Texture2D>("Button/pathSeparator");
        }
    }
    public enum Separator
    {
        None,
        Line,
        Arrow
    }
}
