﻿using System;
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
        public Schematic(string Name)
        {
            this.Name = Name;
            wires = new List<Wire>();
            components = new List<Component>();
        }
        public static void LoadContent(ContentManager Content)
        {

        }
        public void AddComponent(Component c, bool reloadWires = true)
        {
            components.Add(c);
            foreach (Wire w in c.wires)
            {
                if (!wires.Contains(w))
                    wires.Add(w);
            }
            if (reloadWires)
                ReloadWiresFromComponents();
        }
        public void DeleteComponent(Component c, bool reloadWires = true)
        {
            if (components.Contains(c))
            {
                /*foreach (Wire w in c.wires)
                {
                    w.components.Remove(c);
                }*/
                components.Remove(c);
            }
            if (reloadWires)
                ReloadWiresFromComponents();
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

        public void ReloadWiresFromComponents()
        {
            // Supprime toute les informations que contiennent les fils (sauf leur valeur),
            // et les recrée à partir des informations que contiennent les composants
            foreach (Wire w in wires)
            {
                w.components.Clear();
            }
            wires.Clear();
            foreach (Component c in components)
            {
                foreach(Wire w in c.wires)
                {
                    if (!wires.Contains(w))
                    {
                        wires.Add(w);
                        w.components.Clear();
                    }
                    if (!w.components.Contains(c))
                        w.components.Add(c);
                }
            }
            //wires.RemoveAll(w => w.components.Count() == 0);
        }
        public void Initialize(bool recursive = false)
        {
            foreach (Component c in components)
            {
                if (c is BlackBox bb && recursive)
                {
                    bb.schem.Initialize(true);
                }
            }
            ReloadWiresFromComponents();
            foreach (Wire w in wires)
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
        public Schematic Copy()
        {
            // Retourne une copie du schematic
            Schematic newSchem = new Schematic(Name);
            foreach (Component c in components)
            {
                newSchem.AddComponent(c.Copy());
            }
            // Copier chaque composant déconnecte tout les fils. Il faut donc les reconnecter après coup
            // Normalement, l'ordre des composants est conservé
            foreach (Wire w in wires)
            {
                Wire newWire = new Wire();
                foreach (Component c in w.components)
                {
                    int i = components.IndexOf(c);
                    int j = c.wires.IndexOf(w);
                    newSchem.components[i].wires[j] = newWire;
                }
            }
            newSchem.Initialize();
            return newSchem;
        }
        public void AddSchematic(Schematic other)
        {
            // TODO
        }
        public static Schematic operator +(Schematic a, Schematic b) {
            Schematic c = a.Copy();
            c.AddSchematic(b);
            return c;
        }
    }
}
