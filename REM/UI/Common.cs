using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace REM.UI
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
