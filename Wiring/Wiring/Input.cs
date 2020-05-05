using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring.Wiring
{
    /// <summary>
    /// Composant permettant de modifier des valeurs de fil ou d'implémenter des entrées
    /// </summary>
    public class Input : Component
    {
        public static Texture2D texOn, texOff;
        private bool value;
        public void changeValue()
        {
            value = !value;
            MustUpdate = true;
        }
        public void changeValue(bool v)
        {
            MustUpdate = (value != v);
            value = v;
        }
        public bool getValue() => value;

        public Input(Wire output, Vector2 position) : base(position)
        {
            value = false;
            wires.Add(output);
            plugWires();
        }
        public static new void LoadContent(ContentManager Content)
        {
            texOn = Content.Load<Texture2D>("Component/inputOn");
            texOff = Content.Load<Texture2D>("Component/inputOff");
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
        public override Vector2 plugPosition(Wire wire)
        {
            if (wire == wires[0])
                return position + new Vector2(1 + size / 2, 0);
            return base.plugPosition(wire);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Texture2D texture = value ? texOn : texOff;
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
        }
        public override Component Copy()
        {
            Input newInput = new Input(new Wire(), position);
            newInput.value = value;
            return newInput;
        }
    }
}
