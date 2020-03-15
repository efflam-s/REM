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
    /// Système de composants reliés par des fils
    /// </summary>
    public class Schematic
    {
        public string Name;
        public List<Wire> wires;
        public List<Component> components;
        public List<Input> inputs;
        public List<Output> outputs;
        public Schematic(string Name)
        {
            this.Name = Name;
            wires = new List<Wire>();
            components = new List<Component>();
            inputs = new List<Input>();
            outputs = new List<Output>();
        }
        public static void LoadContent(ContentManager Content)
        {

        }
        public void AddComponent(Component c)
        {
            if (c is Input i)
            {
                inputs.Add(i);
            }
            if (c is Output o)
            {
                outputs.Add(o);
            }
            components.Add(c);
            foreach (Wire w in c.wires)
            {
                if (!wires.Contains(w))
                    wires.Add(w);
            }
        }
        public void DeleteComponent(Component c)
        {
            int i;
            for (i = 0; i < components.Count() && components[i] != c; i++);
            if (i < components.Count())
            {
                foreach (Wire w in c.wires)
                {
                    w.components.Remove(c);
                }
                components.Remove(c);
                if (c is Input ip)
                {
                    inputs.Remove(ip);
                }
                if (c is Output op)
                {
                    outputs.Remove(op);
                }
            }
        }
        public void DeleteWire(Wire w)
        {
            int i;
            for (i = 0; i < wires.Count() && wires[i] != w; i++) ;
            if (i < wires.Count())
            {
                foreach (Component c in w.components)
                {
                    Wire newW = c.wires[c.wires.IndexOf(w)] = new Wire();
                    newW.components.Add(c);
                    wires.Add(newW);
                }
                wires.Remove(w);
            }
        }

        public void Initialize()
        {
            foreach(Wire w in wires)
            {
                w.Update();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Wire w in wires)
            {
                w.Draw(spriteBatch);
            }
            foreach (Component c in components)
            {
                c.Draw(spriteBatch);
            }
        }
        /*public void BasicDraw(SpriteBatch spriteBatch)
        {
            foreach (Wire w in wires)
            {
                w.Draw(spriteBatch);
            }
            foreach (Component c in components)
            {
                c.BasicDraw(spriteBatch);
            }
        }*/
    }
}
