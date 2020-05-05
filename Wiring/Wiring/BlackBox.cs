using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Wiring.Wiring
{
    /// <summary>
    /// Composant permettant de simuler une boîte noire, c'est à dire de mettre un schematic dans un composant pour pouvoir le réutiliser
    /// Il y a autant d'élement dans wires que dans l'ensemble de inputs et outputs
    /// </summary>
    public class BlackBox : Component
    {
        public static Texture2D texture;
        public Schematic schem;
        private List<Input> inputs; // liste des composants Input du schematic
        private List<Output> outputs; // liste des composants Output du schematic

        public BlackBox(Schematic schem, Vector2 position) : base(position)
        {
            this.schem = schem;
            inputs = new List<Input>();
            outputs = new List<Output>();
            ReloadPlugsFromSchematic(false);
        }
        public BlackBox(string name, Vector2 position) : base(position)
        {
            schem = new Schematic(name);
            inputs = new List<Input>();
            outputs = new List<Output>();
        }
        public static new void LoadContent(ContentManager Content)
        {
            texture = Content.Load<Texture2D>("Component/blackBox");
        }

        public override bool GetOutput(Wire wire)
        {
            int i = wires.IndexOf(wire);
            if (i >= inputs.Count && i < wires.Count)
            {
                return outputs[i - inputs.Count].GetValue();
            }

            return base.GetOutput(wire);
        }
        public override void Update()
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                inputs[i].changeValue(wires[i].value);
            }
            foreach (Component c in schem.components)
            {
                if (c.MustUpdate)
                    c.Update();
            }
            for (int i = inputs.Count; i < outputs.Count + inputs.Count; i++)
            {
                // update les wires sortant qui ont changé
                if (wires[i].value != GetOutput(wires[i]))
                    wires[i].Update();
            }
            base.Update();
        }
        /// <summary>
        /// S'execute une et une seule fois à chaque update du jeu
        /// </summary>
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
            if (inputs.Count + outputs.Count != wires.Count)
                throw new Exception("inapropriate number of wires");
            for (int i = 0; i < wires.Count; i++)
            {
                if (wire == wires[i]) // on a trouvé le fil
                {
                    if (i < inputs.Count)
                    {
                        return position + new Vector2(-1 - size/2, (inputs.Count == 1) ? 0 : -6 + 12f * i / (inputs.Count - 1));
                    } else
                    {
                        return position + new Vector2(1 + size/2, (outputs.Count == 1) ? 0 : -6 + 12f * (i - inputs.Count) / (outputs.Count - 1));
                    }
                }
            }
            return base.plugPosition(wire);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Draw(texture, position - new Vector2(texture.Width, texture.Height) / 2, Color.White);
            //schem.BasicDraw(spriteBatch);
        }
        /// <summary>
        /// ReloadInOutFromSchematic + ReloadPlugsFromInOut
        /// </summary>
        /// <param name="recursive"></param>
        public void ReloadPlugsFromSchematic(bool recursive)
        {
            if (inputs.Count + outputs.Count != wires.Count)
                throw new Exception("inapropriate number of wires");

            int prevInCount = inputs.Count, prevOutCount = outputs.Count;
            reloadInOutFromSchematic();

            if (inputs.Count > prevInCount)
                // il manque des fils d'input
                for (int i = prevInCount; i < inputs.Count; i++)
                    wires.Insert(prevInCount, new Wire());
            else if (inputs.Count < prevInCount)
                // il y a des fils en trop
                wires.RemoveRange(inputs.Count, prevInCount - inputs.Count);

            if (outputs.Count > prevOutCount)
                // il manque des fils d'output
                for (int i = prevOutCount; i < outputs.Count; i++)
                    wires.Add(new Wire());
            else if (outputs.Count < prevOutCount)
                // il y a des fils en trop
                wires.RemoveRange(inputs.Count + outputs.Count, prevOutCount - outputs.Count);

            plugWires();

            if (recursive)
            {
                foreach (Component c in schem.components)
                    if (c is BlackBox bb)
                        bb.ReloadPlugsFromSchematic(true);
            }
        }
        public static BlackBox Default(Vector2 position)
        {
            Schematic boxSchem = new Schematic("Box");
                boxSchem.wires.Add(new Wire());
                boxSchem.components.Add(new Input(boxSchem.wires[0], new Vector2(32, 32)));
                boxSchem.components.Add(new Output(boxSchem.wires[0], new Vector2(64, 32)));
            boxSchem.Initialize();
            BlackBox blackBox = new BlackBox(boxSchem, position);
            //blackBox.reloadInOutFromSchematic(); // déjà fait dans le constructeur
            return blackBox;
        }
        public override Component Copy()
        {
            BlackBox newBlackBox = new BlackBox(schem.Copy(), position);
            newBlackBox.ReloadPlugsFromSchematic(false);
            return newBlackBox;
        }
        private void reloadInOutFromSchematic()
        {
            inputs.Clear();
            outputs.Clear();
            // Récupère tout les composants de type Input et Output du schematic pour les stocker dans inputs et outputs
            foreach (Component c in schem.components)
            {
                if (c is Input i)
                {
                    inputs.Add(i);
                }
                if (c is Output o)
                {
                    outputs.Add(o);
                }
            }
            // trie les Input et Output par hauteur
            inputs.Sort((x, y) => x.position.Y <= y.position.Y ? x.position.Y == y.position.Y ? 0 : -1 : 1);
            outputs.Sort((x, y) => x.position.Y <= y.position.Y ? x.position.Y == y.position.Y ? 0 : -1 : 1);
        }
    }
}
