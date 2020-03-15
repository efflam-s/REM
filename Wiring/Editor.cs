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
        private static MouseCursor scissor;

        public enum Tool { Edit, Select, Move, Wire }
        // Edit : Outil pour faire un peu tout (selection, déplacement, clics composants...)
        public Tool tool;
        private Schematic mainSchem;

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
            scissor = MouseCursor.FromTexture2D(Content.Load<Texture2D>("cursorScissor"), 4, 4);
        }

        enum ClicState { Up, Clic, Down, Declic }
        public void Update(GameTime gameTime, Matrix Camera, Vector2 Size)
        {
            // variables utiles pour l'update
            KeyboardState KbState = Keyboard.GetState();
            MouseState MsState = Mouse.GetState();
            Vector2 virtualMousePosition = Vector2.Transform(MsState.Position.ToVector2(), Matrix.Invert(Camera));
            virtualMousePosition = new Vector2(minMax(virtualMousePosition.X, 0, Size.X), minMax(virtualMousePosition.Y, 0, Size.Y));
            MsState = new MouseState((int)virtualMousePosition.X, (int)virtualMousePosition.Y, MsState.ScrollWheelValue, MsState.LeftButton, MsState.MiddleButton, MsState.RightButton, MsState.XButton1, MsState.XButton2);
            bool Control = KbState.IsKeyDown(Keys.LeftControl) || KbState.IsKeyDown(Keys.RightControl);
            Component hoveredComponent = hover(MsState.Position.ToVector2()); // component pressed
            Wire hoveredWire = hoverWire(MsState.Position.ToVector2()); // wire pressed
            ClicState leftClic;
            if (MsState.LeftButton == ButtonState.Pressed)
                if (prevMsState.LeftButton == ButtonState.Pressed)
                    leftClic = ClicState.Down;
                else
                    leftClic = ClicState.Clic;
            else
                if (prevMsState.LeftButton == ButtonState.Pressed)
                    leftClic = ClicState.Declic;
                else
                    leftClic = ClicState.Up;

            // Automate à états finis (avec des if parce que j'aime pas les switch)
            if (tool == Tool.Edit)
            {
                if (leftClic == ClicState.Clic)
                {
                    // Au clic
                    if (hoveredComponent == null)
                    {
                        if (hoveredWire == null)
                        {
                            if (!Control && KbState.IsKeyUp(Keys.LeftAlt))
                                // deselectionne
                                selected.Clear();

                            // rectangle selection
                            tool = Tool.Select;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        if (!Control)
                        {
                            if (!selected.Contains(hoveredComponent))
                            {
                                // preparation deplacement d'un seul composant (pas dans la selection)
                                relativePositionToMouse.Clear();
                                relativePositionToMouse.Add(hoveredComponent.position - MsState.Position.ToVector2());
                                MovingComp = hoveredComponent;
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
                    }
                    // stockage des infos importantes du clic (position + temps)
                    mousePositionOnClic = MsState.Position.ToVector2();
                    timeOnClic = gameTime.TotalGameTime;
                }
                else if (leftClic == ClicState.Declic)
                {
                    // Au déclic
                    if (!hasMoved(MsState.Position.ToVector2(), Camera))
                    {
                        // clic simple (pas de déplacement)
                        if (hoveredComponent != null)
                        {
                            if (hoveredComponent is Input i && !Control && KbState.IsKeyUp(Keys.LeftAlt))
                            {
                                // switch d'un input
                                i.changeValue();
                            }
                            else// if (!selected.Contains(hoveredComponent))
                            {
                                // selection d'un composant
                                if (!Control && KbState.IsKeyUp(Keys.LeftAlt))
                                    selected.Clear();

                                if (KbState.IsKeyUp(Keys.LeftAlt) && !selected.Contains(hoveredComponent))
                                    selected.Add(hoveredComponent);
                                else if (KbState.IsKeyDown(Keys.LeftAlt) && selected.Contains(hoveredComponent))
                                    selected.Remove(hoveredComponent);

                            }
                        }
                        else if (hoveredWire != null)
                        {
                            mainSchem.DeleteWire(hoveredWire);
                        }
                    }
                }
                else if (leftClic == ClicState.Down)
                {
                    // Pendant le clic
                    if (hoveredComponent != null && hasMoved(MsState.Position.ToVector2(), Camera) && (MovingComp != null || selected.Contains(hoveredComponent)))
                    {
                        // déplacement d'un composant ou de la sélection
                        tool = Tool.Move;
                    }
                }
                else if (leftClic == ClicState.Up)
                {
                    // pas de clic
                    if (KbState.IsKeyDown(Keys.Delete) && prevKbState.IsKeyUp(Keys.Delete) && selected.Count() > 0)
                    {
                        // suppression de la selection
                        foreach (Component c in selected)
                        {
                            mainSchem.DeleteComponent(c);
                        }
                        selected.Clear();
                    }
                    if (Control && KbState.IsKeyDown(Keys.A) && prevKbState.IsKeyUp(Keys.A))
                    {
                        // selectionner tout
                        selected.Clear();
                        foreach (Component c in mainSchem.components)
                        {
                            selected.Add(c);
                        }
                    }
                    if (KbState.IsKeyDown(Keys.C) && prevKbState.IsKeyUp(Keys.C))
                    {
                        tool = Tool.Wire;
                    }
                }
            }
            else if (tool == Tool.Move)
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
                if (leftClic == ClicState.Declic)
                {
                    // fin de déplacement
                    MovingComp = null;
                    tool = Tool.Edit;
                }
            }
            else if (tool == Tool.Select)
            {
                if (leftClic == ClicState.Declic)
                {
                    // rectangle de selection
                    foreach (Component c in mainSchem.components)
                    {
                        bool rectInComp = rectInComponent(MsState.Position.ToVector2(), mousePositionOnClic, c.position);
                        if (KbState.IsKeyUp(Keys.LeftAlt) && !selected.Contains(c) && rectInComp)
                        {
                            selected.Add(c);
                        }
                        else if (KbState.IsKeyDown(Keys.LeftAlt) && selected.Contains(c) && rectInComp)
                        {
                            selected.Remove(c);
                        }
                    }
                    // fin de selection
                    //if (leftClic == ClicState.Declic)
                        tool = Tool.Edit;
                }
            }
            else if (tool == Tool.Wire)
            {
                if (leftClic == ClicState.Clic)
                {
                    if (hoveredComponent != null)
                    {
                        // Preparation liaison avec un autre composant
                        MovingComp = hoveredComponent;
                    }
                    // stockage des infos importantes du clic (position + temps)
                    mousePositionOnClic = MsState.Position.ToVector2();
                    timeOnClic = gameTime.TotalGameTime;
                }
                else if (leftClic == ClicState.Declic)
                {
                    if (hoveredComponent != null)
                    {
                        Wire start = MovingComp.nearestPlugWire(mousePositionOnClic);
                        Wire end = hoveredComponent.nearestPlugWire(MsState.Position.ToVector2());
                        if (hoveredComponent != MovingComp && start != end)
                        {
                            foreach (Component c in end.components)
                            {
                                c.wires[c.wires.IndexOf(end)] = start;
                                start.components.Add(c);
                            }
                            mainSchem.wires.Remove(end);
                        }
                    }
                    tool = Tool.Edit;
                }
            }
            
            prevKbState = KbState;
            prevMsState = MsState;

            //if ((gameTime.TotalGameTime - gameTime.ElapsedGameTime).TotalMilliseconds % 2000 > gameTime.TotalGameTime.TotalMilliseconds % 2000)
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
        /// Détermine le fil sur lequel est positionné la souris. Retourne null si pas de composant trouvé
        /// </summary>
        public Wire hoverWire(Vector2 position)
        {
            foreach (Wire w in mainSchem.wires)
            {
                if (w.touchWire(position))
                    return w;
            }
            return null;
        }
        /// <summary>
        /// Détemine si la souris a bougée de plus de 12 pixels depuis le dernier clic
        /// </summary>
        private bool hasMoved(Vector2 MousePosition, Matrix Camera)
        {
            // calcul de la distance (norme infinie) entre la position au clic et maintenant
            Vector2 v = Vector2.Transform(mousePositionOnClic, Camera) - Vector2.Transform(MousePosition, Camera);
            return Math.Max(Math.Abs(v.X), Math.Abs(v.Y)) > 4;
        }
        private bool rectInComponent(Vector2 Corner1, Vector2 Corner2, Vector2 CompPosition)
        {
            return Math.Min(Corner1.X, Corner2.X) < CompPosition.X + Component.size / 2 && Math.Max(Corner1.X, Corner2.X) > CompPosition.X - Component.size / 2
                && Math.Min(Corner1.Y, Corner2.Y) < CompPosition.Y + Component.size / 2 && Math.Max(Corner1.Y, Corner2.Y) > CompPosition.Y - Component.size / 2;
        }
        private float minMax(float x, float min, float max)
        {
            if (min < x && x < max)
                return x;
            else if (x < min)
                return min;
            else
                return max;
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
                    if (prevKbState.IsKeyUp(Keys.LeftControl) && prevKbState.IsKeyUp(Keys.RightControl) && prevKbState.IsKeyUp(Keys.LeftAlt) &&
                            hover(prevMsState.Position.ToVector2()) is Input)
                        Mouse.SetCursor(MouseCursor.Hand);
                    else if (prevKbState.IsKeyUp(Keys.LeftControl) && prevKbState.IsKeyUp(Keys.RightControl) &&
                            hoverWire(prevMsState.Position.ToVector2()) != null)
                        Mouse.SetCursor(scissor);
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
        public void AddComponent(Component c)
        {
            mainSchem.AddComponent(c);
            tool = Tool.Move;
            MovingComp = c;
            selected.Clear();
            relativePositionToMouse.Clear();
            relativePositionToMouse.Add(Vector2.Zero);
        }
        public string GetInfos()
        {
            return "M : ("+prevMsState.X+", "+prevMsState.Y+
                ")  Comp : "+mainSchem.components.Count()+" Selected : "+selected.Count()+
                "  Fil : "+mainSchem.wires.Count();
        }
    }
}
