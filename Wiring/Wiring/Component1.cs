using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring.Wiring
{
    /// <summary>
    /// Template of Component
    /// </summary>
    public class Component1 : Component
    {
        public static Texture2D texture;

        public Component1(Wire wire1, Vector2 position) : base(position)
        {
            wires.Add(wire1);

            plugWires();
        }
        public static new void LoadContent(ContentManager Content)
        {
            texture = Content.Load<Texture2D>("Component/Component1");

        }
        public override bool GetOutput(Wire wire)
        {
            if (wire == wires[0])
                return false;
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
                return position + new Vector2(0, 0);

            return base.plugPosition(wire);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
        }
        public override Component Copy()
        {
            Component1 newComponent1 = new Component1(new Wire(), position);
            return newComponent1;
        }
    }
}
