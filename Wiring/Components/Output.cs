using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring
{
    /// <summary>
    /// Composant permettant de connaître des valeurs de fil ou d'implémenter des sorties
    /// </summary>
    public class Output : Component
    {
        public static Texture2D texOn, texOff;
        public Output(Wire input, Vector2 position) : base(position)
        {
            base.wires.Add(input);
            plugWires();
        }
        public static new void LoadContent(ContentManager Content)
        {
            texOn = Content.Load<Texture2D>("Component/outputOn");
            texOff = Content.Load<Texture2D>("Component/outputOff");
        }
        public bool GetValue()
        {
            //wires[0].Update();
            return wires[0].value;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Texture2D texture = GetValue() ? texOn : texOff;
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
        }
        public override Component Copy()
        {
            Output newOutput = new Output(new Wire(), position);
            return newOutput;
        }
    }
}
