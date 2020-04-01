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
        public static float size { get => 16; }
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
            square = Content.Load<Texture2D>("Component/component");
            select = Content.Load<Texture2D>("selectComp");
            Output.LoadContent(Content);
            Input.LoadContent(Content);
            Not.LoadContent(Content);
            Diode.LoadContent(Content);
            BlackBox.LoadContent(Content);
        }
        protected void plugWires()
        {
            /* Fonction à executer après chaque constructeur pour que les wires reconnaissent le composant */
            foreach (Wire w in wires)
            {
                w.components.Add(this);
            }
        }
        public virtual bool GetOutput(Wire wire)
        {
            return false;
        }
        public virtual void Update()
        {
            MustUpdate = false;
        }
        public virtual Vector2 plugPosition(Wire wire)
        {
            return position;
        }
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
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(square, position - new Vector2(square.Width, square.Height) / 2, Color.White);
        }
        public virtual void DrawSelected(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(select, position - new Vector2(select.Width, select.Height) / 2, Color.White);
        }
        /*public void BasicDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(square, position - new Vector2(square.Width, square.Height) / 2, Color.White);
        }*/
        public abstract Component Copy();
    }
}
