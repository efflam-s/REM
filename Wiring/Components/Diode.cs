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
        private enum State { Down, Rise, Up, Fall }
        State state;

        public Diode(Wire input, Wire output, Vector2 position) : base(position)
        {
            base.wires.Add(input);
            base.wires.Add(output);
            plugWires();
            delay = TimeSpan.Zero;
            state = State.Down;
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
                if (delay == TimeSpan.Zero)
                    return wires[0].value;
                else
                    return state == State.Fall || state == State.Up;
            }
            return base.GetOutput(wire);
        }
        public override void Update()
        {
            if (state == State.Down)
            {
                if (wires[0].value)
                {
                    state = State.Rise;
                    time = TimeSpan.Zero;
                }
            }
            else if (state == State.Rise)
            {
                if (time >= delay)
                {
                    time = TimeSpan.Zero;
                    state = State.Up;
                }
            }
            else if (state == State.Up)
            {
                if (!wires[0].value) {
                    state = State.Fall;
                    time = TimeSpan.Zero;
                }
            }
            else if (state == State.Fall)
            {
                if(wires[0].value)
                {
                    state = State.Up;
                }
                if (time >= delay)
                {
                    time = TimeSpan.Zero;
                    state = State.Down;
                }
            }
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
            int i = (int)delay.TotalSeconds;
            if (i > texOn.Length) i = 0;
            Texture2D texture = GetOutput(wires[1]) ? texOn[i] : texOff[i];
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
        }

        /// <summary>
        /// S'execute une et une seule fois à chaque update du jeu
        /// </summary>
        /// <returns>Si il y a eu un changement</returns>
        public void UpdateTime(GameTime gameTime)
        {
            if (state == State.Rise || state == State.Fall)
                time += gameTime.ElapsedGameTime;
            if (time >= delay && time != TimeSpan.Zero)
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
        public override Component Copy()
        {
            Diode newDiode = new Diode(new Wire(), new Wire(), position);
            newDiode.delay = delay;
            return newDiode;
        }
    }
}
