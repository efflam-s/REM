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
    /// Unité de logique permettant de modifier des valeurs de fils en fonction d'autres valeurs.
    /// Leur méthode principale est GetOutput, et permet de mettre à jour les fils reliés au composant.
    /// </summary>
    public abstract class Component
    {
        public static Texture2D square;
        public static Texture2D select;
        public static float size { get => 16; } // const et static incompatibles
        public List<Wire> wires;
        public Vector2 position;
        public bool MustUpdate;
        //public bool updated;
        public Component(Vector2 position)
        {
            wires = new List<Wire> { };
            this.position = position;
            MustUpdate = false;
        }
        public static void LoadContent(ContentManager Content)
        {
            Output.LoadContent(Content);
            Input.LoadContent(Content);
            Not.LoadContent(Content);
            Diode.LoadContent(Content);
            BlackBox.LoadContent(Content);
            square = Content.Load<Texture2D>("Component/component");
            select = Content.Load<Texture2D>("selectComp");
        }
        /// <summary>
        /// Fonction à executer après chaque constructeur pour que les wires reconnaissent le composant
        /// </summary>
        protected void plugWires()
        {
            foreach (Wire w in wires)
            {
                if (!w.components.Contains(this))
                    w.components.Add(this);
            }
        }
        /// <summary>
        /// <i>Override</i> : Permet de récupérer la valeur d'un fil, renvoie faux si le fil n'est pas dans les sorties
        /// </summary>
        /// <param name="wire">Fil dont on veut connaître la valeur</param>
        public virtual bool GetOutput(Wire wire)
        {
            return false;
        }
        /// <summary>
        /// <i>Override</i> : Doit mettre à jour chaque fil de sortie qui a changé
        /// </summary>
        public virtual void Update()
        {
            MustUpdate = false;
        }
        /// <summary>
        /// <i>Override</i> : Donne la position d'une prise du composant reliée à fil donné, renvoie la position du centre par défaut
        /// </summary>
        /// <param name="wire">Fil dont on veut connaître la position de la prise</param>
        public virtual Vector2 plugPosition(Wire wire)
        {
            return position;
        }
        /// <summary>
        /// Renvoie le fil dont la prise sur le composant est la plus proche de la position donnée
        /// </summary>
        public Wire nearestPlugWire(Vector2 position)
        {
            if (wires.Count() == 0)
                return null;
            Wire minWire = wires[0];
            foreach (Wire w in wires)
            {
                if ((plugPosition(w) - position).Length() < (plugPosition(minWire) - position).Length())
                    minWire = w;
            }
            return minWire;
        }
        public bool touch(Vector2 position, bool includePlugs = false)
        {
            Vector2 v = this.position - position;
            return Math.Min(v.X, v.Y) > -size / 2 && Math.Max(v.X, v.Y) <= size / 2 || (includePlugs && touchPlug(position));
        }
        public bool touchPlug(Vector2 position)
        {
            foreach (Wire w in wires)
            {
                // bouts de fils au niveau des prises
                if (Wire.touchLine(position, plugPosition(w), new Vector2(this.position.X, plugPosition(w).Y)))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Dessine la base du composant. <br/>
        /// <i>Override</i> : Dessine la partie spécifique du composant
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            foreach(Wire w in wires)
            {
                Vector2 plugPos = plugPosition(w);
                Wire.drawLine(spriteBatch, plugPos, new Vector2(position.X, plugPos.Y), w.value);
            }
            spriteBatch.Draw(square, position - new Vector2(square.Width, square.Height) / 2, Color.White);
        }
        /// <summary>
        /// Dessine le retangle montrant le composant sélectionné
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void DrawSelected(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(select, position - new Vector2(select.Width, select.Height) / 2, Color.White);
        }
        /*public void BasicDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(square, position - new Vector2(square.Width, square.Height) / 2, Color.White);
        }*/
        /// <summary>
        /// <i>Override</i> : Crée une copie d'un composant avec des nouveaux fils, la même position, et les autres paramètres identiques
        /// </summary>
        public abstract Component Copy();
    }
}
