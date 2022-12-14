using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace REM.Wiring
{
    /// <summary>
    /// Composant permettant d'implémenter une porte "non" ou un fil allumé
    /// </summary>
    public class Not : Component
    {
        public static Texture2D texOn, texOff;
        public Not(Wire input, Wire output, Vector2 position) : base(position)
        {
            wires.Add(input);
            wires.Add(output);
            plugWires();
        }
        public static new void LoadContent(ContentManager Content)
        {
            texOn = Content.Load<Texture2D>("Component/notOn");
            texOff = Content.Load<Texture2D>("Component/notOff");
        }
        public override bool GetOutput(Wire wire)
        {
            if (wire == wires[1])
            {
                return !wires[0].value;
            }
            return base.GetOutput(wire);
        }
        public override void Update()
        {
            if (wires[1].value != GetOutput(wires[1]))
                wires[1].Update();
            base.Update();
        }
        public override Vector2 plugPosition(Wire wire)
        {
            if (wire == wires[0])
                return position + new Vector2(-1 - size/2, 0);
            if (wire == wires[1])
                return position + new Vector2(1 + size/2, 0);
            return base.plugPosition(wire);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Texture2D texture = GetOutput(wires[1]) ? texOn : texOff;
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
        }
        public override Component Copy()
        {
            Not newNot = new Not(new Wire(), new Wire(), position);
            return newNot;
        }
    }
}
