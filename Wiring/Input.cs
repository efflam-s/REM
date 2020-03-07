using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring
{
    /// <summary>
    /// Composant permettant de modifier des valeurs de fil ou d'implémenter des entrées
    /// </summary>
    public class Input : Component
    {
        public static Texture2D texOn, texOff;
        public bool value { set; private get; }
        public Input(Wire output, Vector2 position) : base(position)
        {
            value = false;
            base.wires.Add(output);
            plugWires();
        }
        public static new void LoadContent(ContentManager Content)
        {
            texOn = Content.Load<Texture2D>("inputOn");
            texOff = Content.Load<Texture2D>("inputOff");
        }
        public override bool GetOutput(Wire wire)
        {
            if (wire == wires[0])
                return value;
            return base.GetOutput(wire);
        }
        public override void Update()
        {
            if (wires[0].value != GetOutput(wires[0]))
                wires[0].Update();
            base.Update();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Texture2D texture = value ? texOn : texOff;
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
        }
    }
}
