using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring
{
    /// <summary>
    /// Composant permettant de simuler une boîte noire, c'est à dire de mettre un schematic dans un composant pour pouvoir le réutiliser
    /// Il y a autant d'élement dans wires que dans l'ensemble de schem.inputs et schem.outputs
    /// </summary>
    public class BlackBox : Component
    {
        public static Texture2D texOn, texOff;
        public Schematic schem;
        public BlackBox(Schematic schem, Vector2 position) : base(position)
        {
            this.schem = schem;
        }
        public BlackBox(string name, Vector2 position) : base(position)
        {
            schem = new Schematic(name);
        }
        public static new void LoadContent(ContentManager Content)
        {
            texOn = Content.Load<Texture2D>("Component/blackBoxOn");
            texOff = Content.Load<Texture2D>("Component/blackBoxOff");
        }
        public override bool GetOutput(Wire wire)
        {
            for (int i = 0; i < schem.outputs.Count; i++)
            {
                if (wire == wires[schem.inputs.Count + i])
                {
                    return schem.outputs[i].GetValue();
                }
            }
            return base.GetOutput(wire);
        }
        public override void Update()
        {
            for (int i = 0; i < schem.inputs.Count; i++)
            {
                schem.inputs[i].changeValue(wires[i].value);
            }
            foreach (Component c in schem.components)
            {
                if (c.MustUpdate)
                    c.Update();
            }
            for (int i = schem.inputs.Count; i < schem.outputs.Count + schem.inputs.Count; i++)
            {
                if (wires[i].value != GetOutput(wires[i]))
                    wires[i].Update();
            }
            base.Update();
        }
        public void UpdateTime(GameTime gameTime)
        {
            foreach (Component c in schem.components)
            {
                if (c is Diode d)
                {
                    d.UpdateTime(gameTime);
                    if (d.MustUpdate) MustUpdate = true;
                }
                if (c is BlackBox bb)
                {
                    bb.UpdateTime(gameTime);
                    if (bb.MustUpdate) MustUpdate = true;
                }
            }
        }
        public override Vector2 plugPosition(Wire wire)
        {
            int inpCount = schem.inputs.Count;
            int outpCount = schem.outputs.Count;
            if (inpCount + outpCount != wires.Count)
                throw new Exception("inapropriate number of wires");
            for (int i = 0; i < wires.Count; i++)
            {
                if (wire == wires[i]) // on a trouvé le fil
                {
                    if (i < inpCount)
                    {
                        return position + (inpCount == 1 ? new Vector2(-8, 0) : new Vector2(-8, -6 + 12f * i / (inpCount - 1)));
                    } else
                    {
                        return position + (outpCount == 1 ? new Vector2(8, 0) : new Vector2(8, -6 + 12f * (i - inpCount) / (outpCount - 1)));
                    }
                }
            }
            return base.plugPosition(wire);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Texture2D texture = texOff;
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
            //schem.BasicDraw(spriteBatch);
        }
        public void ReloadPlugsFromInOut(bool recursive)
        {
            // probleme : pas de differentiation input/output (des outputs peuvent devenir des inputs)
            if (wires.Count < schem.inputs.Count + schem.outputs.Count) {
                // il manque des fils
                for (int i= wires.Count; i < schem.inputs.Count + schem.outputs.Count; i++)
                {
                    wires.Add(new Wire());
                }
            }
            else if (wires.Count > schem.inputs.Count + schem.outputs.Count)
            {
                // il y a des fils en trop
                wires.RemoveRange(schem.inputs.Count + schem.outputs.Count, wires.Count - schem.inputs.Count - schem.outputs.Count);
            }
            // sinon tout va bien (ou pas ?)

            if (recursive)
            {
                foreach (Component c in schem.components)
                    if (c is BlackBox bb)
                        bb.ReloadPlugsFromInOut(true);
            }
        }
        public static BlackBox Default(Vector2 position)
        {
            Schematic boxSchem = new Schematic("Box");
                boxSchem.wires.Add(new Wire());
                boxSchem.components.Add(new Input(boxSchem.wires[0], new Vector2(32, 32)));
                boxSchem.inputs.Add((Input)boxSchem.components[0]);
                boxSchem.components.Add(new Output(boxSchem.wires[0], new Vector2(64, 32)));
                boxSchem.outputs.Add((Output)boxSchem.components[1]);
            boxSchem.Initialize();
            BlackBox blackBox = new BlackBox(boxSchem, position);
            foreach (Component c in boxSchem.inputs)
                blackBox.wires.Add(new Wire());
            foreach (Component c in boxSchem.outputs)
                blackBox.wires.Add(new Wire());
            return blackBox;
        }
    }
}
