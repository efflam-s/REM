using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace REM.Wiring
{
    /// <summary>
    /// Système de composants reliés par des fils. Sa modification se fait par ajout et suppression de composants
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
        /// <summary>
        /// Ajoute un composant dans le schematic. Il sera relié aux autres composants si les fils leur sont identiques
        /// </summary>
        public void AddComponent(Component c)
        {
            components.Add(c);
            // ajoute les fils non encore existants au schematic
            foreach (Wire w in c.wires)
            {
                if (!wires.Contains(w))
                    wires.Add(w);
            }
        }
        /// <summary>
        /// Supprime un composant du schematic. Les connections avec ce composant seront supprimées
        /// </summary>
        public void DeleteComponent(Component c)
        {
            if (components.Contains(c))
            {
                foreach (Wire w in c.wires)
                {
                    w.components.Remove(c);
                    if (w.components.Count == 0)
                        wires.Remove(w);
                }
                components.Remove(c);
            }
            //ReloadWiresFromComponents();
        }
        /// <summary>
        /// Suprime un fil, et donc une connection entre plusieurs composants. 
        /// </summary>
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

        /// <summary>
        /// Supprime toute les informations que contiennent les fils (sauf leur valeur),
        /// et les recrée à partir des informations que contiennent les composants.
        /// </summary>
        public void ReloadWiresFromComponents()
        {
            wires.Clear();
            foreach (Component c in components)
            {
                foreach(Wire w in c.wires)
                {
                    if (!wires.Contains(w))
                    {
                        // Si on ne l'a pas encore ajouté, on l'ajoute et on supprime ses anciennes connections
                        wires.Add(w);
                        w.components.Clear();
                    }
                    // On ajoute ensuite tout les connexions que l'on avait pas déjà
                    if (!w.components.Contains(c))
                        w.components.Add(c);
                }
            }
        }
        /// <summary>
        /// ReloadWiresFromComponents + Update chaque fil (donc tout le schematic)
        /// </summary>
        /// <param name="recursive">si true : Commence par initialiser les schematics de chaque BlackBox</param>
        public void Initialize(bool recursive = false)
        {
            if (recursive) {
                foreach (Component c in components)
                {
                    if (c is BlackBox bb)
                        bb.schem.Initialize(true);
                }
            }
            ReloadWiresFromComponents();
            foreach (Wire w in wires)
            {
                w.Update();
            }
        }

        /// <summary>
        /// Met à jour les composants qui en ont besoin, y compris ceux dépendant du temps (diodes)
        /// </summary>
        public void Update(GameTime gameTime)
        {
            foreach (Component c in components)
            {
                if (c.MustUpdate)
                    c.Update();
            }
            foreach (Component c in components)
            {
                if (c is Diode d)
                    d.UpdateTime(gameTime);
                if (c is BlackBox bb)
                    bb.UpdateTime(gameTime);
            }
        }

        /// <summary>
        /// Dessine chaque fil, puis chaque composant du schematic
        /// </summary>
        /// <param name="spriteBatch"></param>
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
        /// <summary>
        /// Retourne une copie du schematic (recopie chaque composant)
        /// </summary>
        public Schematic Copy()
        {
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
        /*public void AddSchematic(Schematic other)
        {
            // TODO : ajoute un schematic (par exemple en presse-papier) à un autre
        }*/
    }
}
