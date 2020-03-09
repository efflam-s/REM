using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Wiring
{
    public class Editor
    {
        private static Texture2D select;

        private enum Tool { Edit, Select, Move }
        // Edit : Outil pour faire un peu tout (selection, déplacement, clics composants...)
        Tool tool;
        public Schematic mainSchem;

        List<Component> selected; // la liste des composants sélectionnés
        List<Vector2> relativePositionToMouse; // la liste de leur positions relative à la souris (ou la position relative du composant déplacé en 0)
        Component MovingComp; // le composant déplacé non sélectionné
        
        KeyboardState prevKbState;
        MouseState prevMsState;
        Vector2 mousePositionOnClic;
        TimeSpan timeOnClic;

        public Editor()
        {

        }
        public void Initialize()
        {
            tool = Tool.Edit;
            mainSchem = new Schematic("main");

            mainSchem.wires.Add(new Wire());
            mainSchem.wires.Add(new Wire());
            mainSchem.wires.Add(new Wire());
            mainSchem.wires.Add(new Wire());
            mainSchem.AddComponent(new Input(mainSchem.wires[0], new Vector2(32, 32)));
            mainSchem.AddComponent(new Input(mainSchem.wires[1], new Vector2(32, 96)));
            mainSchem.AddComponent(new Not(mainSchem.wires[0], mainSchem.wires[2], new Vector2(64, 32)));
            mainSchem.AddComponent(new Not(mainSchem.wires[1], mainSchem.wires[2], new Vector2(64, 96)));
            mainSchem.AddComponent(new Not(mainSchem.wires[2], mainSchem.wires[3], new Vector2(96, 64)));
            mainSchem.AddComponent(new Output(mainSchem.wires[3], new Vector2(128, 64)));
            mainSchem.Initialize();

            selected = new List<Component>();
            relativePositionToMouse = new List<Vector2>();
        }
        public static void LoadContent(ContentManager Content)
        {
            select = Content.Load<Texture2D>("selectRect");
        }
        public void Update(GameTime gameTime, Matrix Camera)
        {
            KeyboardState KbState = Keyboard.GetState();
            MouseState MsState = Mouse.GetState();
            Vector2 newPosition = Vector2.Transform(MsState.Position.ToVector2(), Matrix.Invert(Camera));
            MsState = new MouseState((int)newPosition.X, (int)newPosition.Y, MsState.ScrollWheelValue, MsState.LeftButton, MsState.MiddleButton, MsState.RightButton, MsState.XButton1, MsState.XButton2);
            bool Control = KbState.IsKeyDown(Keys.LeftControl) || KbState.IsKeyDown(Keys.RightControl);

            Component hovered = hover(MsState.Position.ToVector2()); // component pressed
            if (MsState.LeftButton == ButtonState.Pressed && prevMsState.LeftButton == ButtonState.Released)
            {
                // Au clic
                if (tool == Tool.Edit) {
                    // deselectionne
                    if (hovered == null && !Control)
                        selected.Clear();

                    if (hovered != null && !Control)
                    {
                        if (!selected.Contains(hovered))
                        {
                            // preparation deplacement d'un seul composant (pas dans la selection)
                            relativePositionToMouse.Clear();
                            relativePositionToMouse.Add(hovered.position - MsState.Position.ToVector2());
                            MovingComp = hovered;
                        }
                        else
                        {
                            // preparation deplacement selection
                            relativePositionToMouse.Clear();
                            foreach (Component c in selected)
                            {
                                relativePositionToMouse.Add(c.position - MsState.Position.ToVector2());
                            }
                            MovingComp = null;
                        }
                    }
                    else if (hovered == null)
                    {
                        tool = Tool.Select;
                    }
                }
                // stockage des infos importantes du clic (position + temps)
                mousePositionOnClic = MsState.Position.ToVector2();
                timeOnClic = gameTime.TotalGameTime;
            }
            if (MsState.LeftButton == ButtonState.Released && prevMsState.LeftButton == ButtonState.Pressed)
            {
                // Au déclic
                if (!hasMoved(MsState.Position.ToVector2(), Camera))
                {
                    // clic simple (pas de déplacement)
                    if (tool == Tool.Edit && hovered != null)
                    {
                        if (hovered is Input i && !Control)
                        {
                            // switch d'un input
                            i.changeValue();
                        }
                        else if (!selected.Contains(hovered))
                        {
                            // selection d'un composant
                            if (!Control)
                                selected.Clear();

                            selected.Add(hovered);
                        }
                    }
                }
                if (tool == Tool.Select)// && hasMoved(MsState.Position.ToVector2(), Camera))
                {
                    // rectangle de selection
                    foreach (Component c in mainSchem.components)
                    {
                        if (!selected.Contains(c) && rectInComponent(MsState.Position.ToVector2(), mousePositionOnClic, c.position))
                        {
                            selected.Add(c);
                        }
                    }
                }
                if (tool == Tool.Move)
                {
                    // fin de déplacement
                    MovingComp = null;
                }
                tool = Tool.Edit;
            }
            if (MsState.LeftButton == ButtonState.Pressed)
            {
                // Pendant le clic
                if (tool == Tool.Edit && hovered != null && hasMoved(MsState.Position.ToVector2(), Camera) && (MovingComp != null || selected.Contains(hovered)))
                    tool = Tool.Move;
            }
            if (tool == Tool.Move)
            {
                // déplacement de composant(s)
                if (MovingComp != null)
                {
                    // composant seul, non sélectionné
                    MovingComp.position = MsState.Position.ToVector2() + relativePositionToMouse[0];
                }
                else
                {
                    // sélection
                    for (int i = 0; i < selected.Count; i++)
                    {
                        selected[i].position = MsState.Position.ToVector2() + relativePositionToMouse[i];
                    }
                }
            }

            prevKbState = KbState;
            prevMsState = MsState;

            foreach (Component c in mainSchem.components)
            {
                if (c.MustUpdate)
                    c.Update();
            }
        }

        /// <summary>
        /// Détermine le composant sur lequel est positionné la souris. Retourne null si pas de composant trouvé
        /// </summary>
        public Component hover(Vector2 position)
        {
            foreach (Component c in mainSchem.components)
            {
                Vector2 v = c.position - position;
                if (Math.Max(Math.Abs(v.X), Math.Abs(v.Y)) <= Component.size/2)
                {
                    return c;
                }
            }
            return null;
        }
        /// <summary>
        /// Détemine si la souris a bougée de plus de 12 pixels depuis le dernier clic
        /// </summary>
        private bool hasMoved(Vector2 MousePosition, Matrix Camera)
        {
            // calcul de la distance (norme infinie) entre la position au clic et maintenant
            return Math.Max(Math.Abs(Vector2.Transform(mousePositionOnClic - MousePosition, Camera).X), Math.Abs(Vector2.Transform(mousePositionOnClic - MousePosition, Camera).Y)) > 4;
        }
        private bool rectInComponent(Vector2 Corner1, Vector2 Corner2, Vector2 CompPosition)
        {
            return Math.Min(Corner1.X, Corner2.X) < CompPosition.X + Component.size / 2 && Math.Max(Corner1.X, Corner2.X) > CompPosition.X - Component.size / 2
                && Math.Min(Corner1.Y, Corner2.Y) < CompPosition.Y + Component.size / 2 && Math.Max(Corner1.Y, Corner2.Y) > CompPosition.Y - Component.size / 2;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Component c in selected)
            {
                c.DrawSelected(spriteBatch);
            }
            if (MovingComp != null)
            {
                //MovingComp.DrawSelected(spriteBatch);
            }
            mainSchem.Draw(spriteBatch);

            switch (tool) {
                case Tool.Edit:
                    if (prevKbState.IsKeyUp(Keys.LeftControl) && prevKbState.IsKeyUp(Keys.RightControl) &&
                            hover(prevMsState.Position.ToVector2()) is Input)
                        Mouse.SetCursor(MouseCursor.Hand);
                    else
                        Mouse.SetCursor(MouseCursor.Arrow);
                    break;
                case Tool.Select:
                    Mouse.SetCursor(MouseCursor.Crosshair); // peut-etre a modifier pour un curseur curstom
                    break;
                case Tool.Move:
                    Mouse.SetCursor(MouseCursor.SizeAll);
                    break;
                default:
                    Mouse.SetCursor(MouseCursor.Arrow);
                    break;
            }
            if (prevMsState.LeftButton == ButtonState.Pressed)
            {
                if (tool == Tool.Select)
                {
                    // draw selection rectangle
                    spriteBatch.Draw(select,
                        new Rectangle((int)Math.Min(prevMsState.X, mousePositionOnClic.X), (int)Math.Min(prevMsState.Y, mousePositionOnClic.Y),
                            (int)Math.Abs(mousePositionOnClic.X - prevMsState.X), (int)Math.Abs(mousePositionOnClic.Y - prevMsState.Y)),
                        Color.White);
                }
            }
        }
    }
}
