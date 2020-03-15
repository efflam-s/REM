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
    /// Composant permettant d'implémenter une diode produisant ou non un délai
    /// </summary>
    public class Diode : Component
    {
        public static Texture2D texOn, texOff;
        public int delay;
        private bool value;
        public Diode(Wire input, Wire output, Vector2 position) : base(position)
        {
            base.wires.Add(input);
            base.wires.Add(output);
            plugWires();
            delay = 1;
            value = false;
        }
        public static new void LoadContent(ContentManager Content)
        {
            texOn = Content.Load<Texture2D>("Component/diode1On");
            texOff = Content.Load<Texture2D>("Component/diode1Off");
        }
        public override bool GetOutput(Wire wire)
        {
            if (wire == wires[1])
            {
                return value;
            }
            return base.GetOutput(wire);
        }
        public override void Update()
        {
            base.Update();
            if (wires[1].value != GetOutput(wires[1]))
                wires[1].Update();
            else
            if (wires[0].value != value)
            {
                value = wires[0].value;
                MustUpdate = true;
            }
            //if (wires[1].value != GetOutput(wires[1]))// && delay == 0)
            //    wires[1].Update();
        }
        public override Vector2 plugPosition(Wire wire)
        {
            if (wire == wires[0])
                return position + new Vector2(-8, 0);
            if (wire == wires[1])
                return position + new Vector2(8, 0);
            return base.plugPosition(wire);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Texture2D texture = value ? texOn : texOff;
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
        }
    }
}
