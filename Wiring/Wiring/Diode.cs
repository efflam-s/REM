using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring.Wiring
{
    /// <summary>
    /// Composant permettant d'implémenter une diode produisant ou non un délai
    /// </summary>
    public class Diode : Component
    {
        public static Texture2D[] texOn, texOff;
        public enum Delay { Zero, Tick, Second }
        private const int DelayTypeNumber = 3;
        public Delay delay;
        //public TimeSpan delayTime;
        private TimeSpan time;
        private enum State { Down, Rise, Up, Fall }
        State state;

        public Diode(Wire input, Wire output, Vector2 position) : base(position)
        {
            wires.Add(input);
            wires.Add(output);
            plugWires();
            delay = Delay.Zero;
            state = State.Down;
        }
        public static new void LoadContent(ContentManager Content)
        {
            texOn = new Texture2D[DelayTypeNumber];
            texOff = new Texture2D[DelayTypeNumber];
            for (int i = 0; i < DelayTypeNumber; i++)
            {
                texOn[i] = Content.Load<Texture2D>("Component/diode" + i + "On");
                texOff[i] = Content.Load<Texture2D>("Component/diode" + i + "Off");
            }
        }
        public override bool GetOutput(Wire wire)
        {
            if (wire == wires[1])
            {
                /*if (delay == Delay.Zero)
                    return wires[0].value;
                else*/
                    return state == State.Fall || state == State.Up;
            }
            return base.GetOutput(wire);
        }
        public override void Update()
        {
            if (delay == Delay.Zero)
                state = wires[0].value ? State.Up : State.Down;
            if (state == State.Down && wires[0].value)
            {
                state = State.Rise;
                time = TimeSpan.Zero;
            }
            if (state == State.Up && !wires[0].value)
            {
                state = State.Fall;
                time = TimeSpan.Zero;
            }

            if (wires[1].value != GetOutput(wires[1]))
                wires[1].Update();
            base.Update();
        }
        /// <summary>
        /// S'execute une et une seule fois à chaque update du jeu
        /// </summary>
        public void UpdateTime(GameTime gameTime)
        {

            if (delay == Delay.Tick)
            {
                if (state == State.Rise)
                {
                    state = State.Up;
                    MustUpdate = true;
                }
                else if (state == State.Fall)
                {
                    state = State.Down;
                    MustUpdate = true;
                }
            }
            else if (delay == Delay.Second)
            {
                if (state == State.Rise || state == State.Fall)
                {
                    time += gameTime.ElapsedGameTime;
                    if (time >= TimeSpan.FromSeconds(1))
                    {
                        time = TimeSpan.Zero;
                        state = state == State.Rise ? State.Up : State.Down;
                        MustUpdate = true;
                    }
                    if (state == State.Fall && wires[0].value)
                    {
                        state = State.Up;
                    }
                }
            }
            /*if (state == State.Down)
            {
                if (wires[0].value)
                {
                    state = delay == Delay.Zero ? State.Up : State.Rise;
                    time = TimeSpan.Zero;
                }
            }
            else if (state == State.Rise)
            {
                if (time >= TimeSpan.FromSeconds(1) || delay == Delay.Tick || delay == Delay.Zero)
                {
                    time = TimeSpan.Zero;
                    state = State.Up;
                }
                if (!wires[0].value && (delay == Delay.Tick || delay == Delay.Zero))
                {
                    time = TimeSpan.Zero;
                    state = delay == Delay.Zero ? State.Fall : State.Down;
                }
            }
            else if (state == State.Up)
            {
                if (!wires[0].value)
                {
                    state = delay == Delay.Zero ? State.Down : State.Fall;
                    time = TimeSpan.Zero;
                }
            }
            else if (state == State.Fall)
            {
                if (wires[0].value)
                {
                    state = delay == Delay.Tick ? State.Rise : State.Up;
                }
                if (time >= TimeSpan.FromSeconds(1) || delay == Delay.Tick || delay == Delay.Zero)
                {
                    time = TimeSpan.Zero;
                    state = State.Down;
                }
            }*/
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
            int i = (int)delay;
            Texture2D texture = GetOutput(wires[1]) ? texOn[i] : texOff[i];
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
        }

        public void changeDelay()
        {
            delay = (Delay)(((int)delay + 1) % DelayTypeNumber);
            MustUpdate = true;
        }
        public void changeValue(bool value)
        {
            state = value ? State.Up : State.Down;
        }
        public override Component Copy()
        {
            Diode newDiode = new Diode(new Wire(), new Wire(), position);
            newDiode.delay = delay;
            return newDiode;
        }
    }
}
