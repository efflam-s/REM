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
        public static Texture2D[] texOn, texOff;
        public TimeSpan delay;
        private TimeSpan time;
        private bool value;
        public Diode(Wire input, Wire output, Vector2 position) : base(position)
        {
            base.wires.Add(input);
            base.wires.Add(output);
            plugWires();
            delay = TimeSpan.Zero;
            value = false;
        }
        public static new void LoadContent(ContentManager Content)
        {
            texOn = new Texture2D[2];
            texOff = new Texture2D[2];
            texOn[0] = Content.Load<Texture2D>("Component/diode0On");
            texOff[0] = Content.Load<Texture2D>("Component/diode0Off");
            texOn[1] = Content.Load<Texture2D>("Component/diode1On");
            texOff[1] = Content.Load<Texture2D>("Component/diode1Off");
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
            if (wires[0].value != value)
            {
                if (time < TimeSpan.Zero)
                {
                    time = TimeSpan.Zero;
                }
                if (time >= delay)
                {
                    value = wires[0].value;
                    time = TimeSpan.FromSeconds(-1);
                }
            }
            if (wires[1].value != GetOutput(wires[1]))
                wires[1].Update();
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
            int i = (int)delay.TotalSeconds;
            if (i > texOn.Length) i = 0;
            Texture2D texture = value ? texOn[i] : texOff[i];
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
        }

        public void UpdateTime(GameTime gameTime)
        {
            if (time >= TimeSpan.Zero)
                time += gameTime.ElapsedGameTime;
            if (time >= delay)
                MustUpdate = true;
        }
        public void changeDelay()
        {
            if (delay == TimeSpan.Zero)
            {
                delay = TimeSpan.FromSeconds(1.0);
            }
            else
            {
                delay = TimeSpan.Zero;
            }
        }
    }
}
