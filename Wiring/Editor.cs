﻿using System;
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
        public List<Schematic> schemPile;
        public Schematic mainSchem
        {
            get => schemPile[schemPile.Count - 1];
            set => schemPile[schemPile.Count - 1] = value;
        }

        List<Component> selected; // la liste des composants sélectionnés
        List<Vector2> relativePositionToMouse; // la liste de leur positions relative à la souris (ou la position relative du composant déplacé en 0)
        Component MovingComp; // le composant déplacé non sélectionné

        /*KeyboardState prevKbState;
        MouseState prevMsState;
        Vector2 mousePositionOnClic;
        TimeSpan timeOnClic;*/
        InputManager Inpm;

        public Editor()
        {

        }
        public void Initialize()
        {
            tool = Tool.Edit;
            schemPile = new List<Schematic> { new Schematic("Schematic") };
            mainSchem.Initialize();

            selected = new List<Component>();
            relativePositionToMouse = new List<Vector2>();

            Inpm = new InputManager();
        }
        public static void LoadContent(ContentManager Content)
        {
            select = Content.Load<Texture2D>("selectRect");
            scissor = MouseCursor.FromTexture2D(Content.Load<Texture2D>("cursorScissor"), 4, 4);
        }

        public void Update(GameTime gameTime, ref Matrix Camera, Rectangle Window)
        {
            
            // variables utiles pour l'update
            //KeyboardState KbState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            Vector2 virtualMousePosition = new Vector2(minMax(mouseState.X, Window.X, Window.X + Window.Width), minMax(mouseState.Y, Window.Y, Window.Y + Window.Height));
            bool isMouseInScreen = virtualMousePosition == mouseState.Position.ToVector2();
            virtualMousePosition = Vector2.Transform(virtualMousePosition, Matrix.Invert(Camera));
            mouseState = new MouseState((int)virtualMousePosition.X, (int)virtualMousePosition.Y, mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
            Inpm.Update(mouseState:mouseState);
            //bool Control = KbState.IsKeyDown(Keys.LeftControl) || KbState.IsKeyDown(Keys.RightControl);
            Component hoveredComponent = hover(Inpm.MsPosition()); // component pressed
            Wire hoveredWire = hoverWire(Inpm.MsPosition()); // wire pressed

            // Navigation avec clic roulette
            
            if (Inpm.middleClic == InputManager.ClicState.Clic)
            {
                Inpm.SaveClic(gameTime);
            }
            else if (Inpm.middleClic == InputManager.ClicState.Down)
            {
                Camera.Translation += new Vector3(Inpm.MsPosition() - Inpm.mousePositionOnClic, 0) * Camera.M11; // pas trouvé de meilleur moyen pour trouver la valeur du zoom que M11
            }
            if (Inpm.MsState.ScrollWheelValue - Inpm.prevMsState.ScrollWheelValue != 0)
            {
                if (Inpm.Control)
                {
                    // Zoom avec roulette
                    float scale = (float)Math.Pow(2, (float)(Inpm.MsState.ScrollWheelValue - Inpm.prevMsState.ScrollWheelValue) / 720); // un cran de scroll = 120 ou -120
                    Vector3 CameraPosition = Camera.Translation;
                    float Zoom = Camera.M11;
                    // around scale to fit between 1 and 8
                    if (Zoom * scale < 1)
                        scale = 1 / Zoom;
                    if (Zoom * scale > 8)
                        scale = 8 / Zoom;
                    Vector2 originalMousePosition = Vector2.Transform(Inpm.MsPosition(), Camera);
                    Camera = Matrix.CreateTranslation(new Vector3(-Inpm.MsState.X, -Inpm.MsState.Y, 0)) * Matrix.CreateScale(scale * Zoom) * Matrix.CreateTranslation(CameraPosition + new Vector3(Inpm.MsState.X, Inpm.MsState.Y, 0) * Zoom);
                }
                else if (Inpm.Shift)
                {
                    // scroll horizontal
                    Camera *= Matrix.CreateTranslation((float)(Inpm.MsState.ScrollWheelValue - Inpm.prevMsState.ScrollWheelValue) / 3, 0, 0);
                }
                else
                {
                    // scroll vertical
                    Camera *= Matrix.CreateTranslation(0, (float)(Inpm.MsState.ScrollWheelValue - Inpm.prevMsState.ScrollWheelValue) / 3, 0);
                }
            }

            // Automate à états finis (avec des if parce que j'aime pas les switch)
            if (tool == Tool.Edit)
            {
                if (Inpm.leftClic == InputManager.ClicState.Clic)
                {
                    // Au clic
                    if (hoveredComponent == null)
                    {
                        if (hoveredWire == null)
                        {
                            if (!Inpm.Control && !Inpm.Alt)
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
                        if (!Inpm.Control)
                        {
                            if (!selected.Contains(hoveredComponent))
                            {
                                // preparation deplacement d'un seul composant (pas dans la selection)
                                relativePositionToMouse.Clear();
                                relativePositionToMouse.Add(hoveredComponent.position - Inpm.MsPosition());
                                MovingComp = hoveredComponent;
                            }
                            else
                            {
                                // preparation deplacement selection
                                relativePositionToMouse.Clear();
                                foreach (Component c in selected)
                                {
                                    relativePositionToMouse.Add(c.position - Inpm.MsPosition());
                                }
                                MovingComp = null;
                            }
                        }
                    }
                    // stockage des infos importantes du clic (position + temps)
                    Inpm.SaveClic(gameTime);
                }
                else if (Inpm.leftClic == InputManager.ClicState.Declic)
                {
                    // Au déclic
                    if (!hasMoved(Inpm.MsPosition(), Camera))
                    {
                        // clic simple (pas de déplacement)
                        if (hoveredComponent != null)
                        {
                            if (hoveredComponent is Input i && !Inpm.Control && !Inpm.Alt)
                            {
                                // switch d'un input
                                i.changeValue();
                            }
                            else if (hoveredComponent is Diode d && !Inpm.Control && !Inpm.Alt)
                            {
                                d.changeDelay();
                            }
                            else if (hoveredComponent is BlackBox bb && !Inpm.Control && !Inpm.Alt)
                            {
                                // ouvrir une schematic de blackbox
                                schemPile.Add(bb.schem);
                            }
                            else// if (!selected.Contains(hoveredComponent))
                            {
                                // selection d'un composant
                                if (!Inpm.Control && !Inpm.Alt)
                                    selected.Clear();
                                if (!Inpm.Alt && !selected.Contains(hoveredComponent))
                                    selected.Add(hoveredComponent);
                                else if (Inpm.Alt && selected.Contains(hoveredComponent))
                                    selected.Remove(hoveredComponent);

                            }
                        }
                        else if (hoveredWire != null)
                        {
                            // Suppression d'un fil (ou d'une partie de fil)
                            if (hoveredWire.components.Count() <= 2 || (Inpm.MsPosition() - hoveredWire.Node()).Length() < 5)
                                mainSchem.DeleteWire(hoveredWire);
                            else
                            {
                                Vector2 node = hoveredWire.Node();
                                foreach (Component c in hoveredWire.components)
                                {
                                    if (Wire.touchWire(Inpm.MsPosition(), node, c.plugPosition(hoveredWire)))
                                        c.wires[c.wires.IndexOf(hoveredWire)] = new Wire();
                                }
                                mainSchem.ReloadWiresFromComponents();
                                hoveredWire.Update();
                            }
                            // update les composant voisins/les fils ?
                        }
                    }
                }
                else if (Inpm.leftClic == InputManager.ClicState.Down)
                {
                    // Pendant le clic
                    if (hoveredComponent != null && hasMoved(Inpm.MsPosition(), Camera) && (MovingComp != null || selected.Contains(hoveredComponent)))
                    {
                        // déplacement d'un composant ou de la sélection
                        tool = Tool.Move;
                    }
                }
                else if (Inpm.leftClic == InputManager.ClicState.Up)
                {
                    // pas de clic
                    if (Inpm.OnPressed(Keys.Delete) && selected.Count() > 0)
                    {
                        // suppression de la selection
                        foreach (Component c in selected)
                        {
                            mainSchem.DeleteComponent(c);
                        }
                        selected.Clear();
                    }
                    if (Inpm.Control && Inpm.OnPressed(Keys.A))
                    {
                        // selectionner tout
                        selected.Clear();
                        foreach (Component c in mainSchem.components)
                        {
                            selected.Add(c);
                        }
                    }
                    if (Inpm.OnPressed(Keys.C))
                    {
                        tool = Tool.Wire;
                        //Inpm.mousePositionOnClic = new Vector2(); // je sais plus à quoi sert cette ligne mais il doit y avoir moyen de faire mieux !!
                    }
                }
            }
            else if (tool == Tool.Move)
            {
                // déplacement de composant(s)
                if (MovingComp != null)
                {
                    // composant seul, non sélectionné
                    MovingComp.position = Inpm.MsPosition() + relativePositionToMouse[0];
                }
                else
                {
                    // sélection
                    for (int i = 0; i < selected.Count; i++)
                    {
                        selected[i].position = Inpm.MsPosition() + relativePositionToMouse[i];
                    }
                }
                if (Inpm.leftClic == InputManager.ClicState.Declic)
                {
                    // fin de déplacement
                    MovingComp = null;
                    tool = Tool.Edit;
                }
            }
            else if (tool == Tool.Select)
            {
                if (Inpm.leftClic == InputManager.ClicState.Declic)
                {
                    // rectangle de selection
                    foreach (Component c in mainSchem.components)
                    {
                        bool rectInComp = rectInComponent(Inpm.MsPosition(), Inpm.mousePositionOnClic, c.position);
                        if (!Inpm.Alt && !selected.Contains(c) && rectInComp)
                        {
                            selected.Add(c);
                        }
                        else if (Inpm.Alt && selected.Contains(c) && rectInComp)
                        {
                            selected.Remove(c);
                        }
                    }
                    // fin de selection
                    //if (Inpm.leftClic == InputManager.ClicState.Declic)
                        tool = Tool.Edit;
                }
            }
            else if (tool == Tool.Wire)
            {
                if (Inpm.leftClic == InputManager.ClicState.Clic)
                {
                    if (hoveredComponent != null)
                    {
                        // Preparation liaison avec un autre composant
                        //MovingComp = hoveredComponent;
                    }
                    // stockage des infos importantes du clic (position + temps)
                    Inpm.SaveClic(gameTime);
                }
                else if (Inpm.leftClic == InputManager.ClicState.Declic)
                {
                    if ((hoveredComponent != null || hoveredWire != null) && (hover(Inpm.mousePositionOnClic) != null || hoverWire(Inpm.mousePositionOnClic) != null))
                    {
                        // Ajout d'un fil
                        Wire start = (hover(Inpm.mousePositionOnClic) != null) ?
                            hover(Inpm.mousePositionOnClic).nearestPlugWire(Inpm.mousePositionOnClic) :
                            hoverWire(Inpm.mousePositionOnClic);
                        Wire end = (hoveredComponent != null) ?
                            hoveredComponent.nearestPlugWire(Inpm.MsPosition()) :
                            hoveredWire;
                        if (start != end)// && hoveredComponent != hover(Inpm.mousePositionOnClic))
                        {
                            bool OK = true;
                            foreach (Component c1 in end.components)
                                foreach (Component c2 in start.components)
                                    if (c1 == c2)
                                        OK = false;
                            if (OK)
                            {
                                foreach (Component c in end.components)
                                {
                                    c.wires[c.wires.IndexOf(end)] = start;
                                    //start.components.Add(c);
                                }
                                //mainSchem.wires.Remove(end);
                                mainSchem.ReloadWiresFromComponents();
                            }
                        }
                        start.Update();
                    }
                    //MovingComp = null;
                    tool = Tool.Edit;
                }
            }
            
            //prevKbState = KbState;
            //prevMsState = MsState;

            //if ((gameTime.TotalGameTime - gameTime.ElapsedGameTime).TotalMilliseconds % 2000 > gameTime.TotalGameTime.TotalMilliseconds % 2000)
            foreach (Component c in mainSchem.components)
            {
                if (c.MustUpdate)
                    c.Update();
                if (c is Diode d)
                    d.UpdateTime(gameTime);
                if (c is BlackBox bb)
                    bb.UpdateTime(gameTime);
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
        /// Détermine le fil sur lequel est positionné la souris. Retourne null si pas de fil trouvé
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
        /// Détemine si la souris a bougée de plus de 4 pixels depuis le dernier clic
        /// </summary>
        private bool hasMoved(Vector2 MousePosition, Matrix Camera)
        {
            // calcul de la distance (norme infinie) entre la position au clic et maintenant
            Vector2 v = Vector2.Transform(Inpm.mousePositionOnClic, Camera) - Vector2.Transform(MousePosition, Camera);
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
            else if (x <= min)
                return min;
            else
                return max;
        }
        public void Draw(SpriteBatch spriteBatch, Rectangle Window)
        {
            foreach (Component c in selected)
            {
                c.DrawSelected(spriteBatch);
            }
            mainSchem.Draw(spriteBatch);

            if (tool == Tool.Wire && (hover(Inpm.mousePositionOnClic) != null || hoverWire(Inpm.mousePositionOnClic) != null) && Inpm.leftClic == InputManager.ClicState.Down)
            {
                // Ajout d'un fil
                Component hoveredClic = hover(Inpm.mousePositionOnClic);
                Wire hoveredWireClic = hoverWire(Inpm.mousePositionOnClic);
                Vector2 start, end;

                if (hoveredClic != null)
                {
                    start = hoveredClic.plugPosition(hoveredClic.nearestPlugWire(Inpm.mousePositionOnClic));
                }
                else
                    start = hoveredWireClic.Node();

                Component hovered = hover(Inpm.MsState.Position.ToVector2());
                Wire hoveredWire = hoverWire(Inpm.MsState.Position.ToVector2());
                if (hovered != null)
                    end = hovered.plugPosition(hovered.nearestPlugWire(Inpm.MsState.Position.ToVector2()));
                else if (hoveredWire != null)
                    end = hoveredWire.Node();
                else
                    end = Inpm.MsState.Position.ToVector2();

                Wire.drawLine(spriteBatch, start, end, hovered != null || hoveredWire != null);
            }

            if (Window.Contains(Mouse.GetState().Position))
                switch (tool) {
                    // Choix du curseur
                    case Tool.Edit:
                        Vector2 MsPos = Inpm.MsPosition();
                        if (!Inpm.Control && !Inpm.Alt &&
                                (hover(MsPos) is Input || hover(MsPos) is Diode || hover(MsPos) is BlackBox))
                            Mouse.SetCursor(MouseCursor.Hand);
                        else if (!Inpm.Control &&
                                hoverWire(MsPos) != null && hover(MsPos) == null)
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
            if (Inpm.MsState.LeftButton == ButtonState.Pressed)
            {
                if (tool == Tool.Select)
                {
                    // draw selection rectangle
                    spriteBatch.Draw(select,
                        new Rectangle((int)Math.Min(Inpm.MsState.X, Inpm.mousePositionOnClic.X), (int)Math.Min(Inpm.MsState.Y, Inpm.mousePositionOnClic.Y),
                            (int)Math.Abs(Inpm.mousePositionOnClic.X - Inpm.MsState.X), (int)Math.Abs(Inpm.mousePositionOnClic.Y - Inpm.MsState.Y)),
                        Color.White);
                }
            }
        }
        public void AddComponent(Component c)
        {
            c.position = Inpm.MsPosition();
            mainSchem.AddComponent(c);
            tool = Tool.Move;
            MovingComp = c;
            selected.Clear();
            relativePositionToMouse.Clear();
            relativePositionToMouse.Add(Vector2.Zero);
        }
        public string GetInfos()
        {
            return "M : ("+Inpm.MsState.X+", "+Inpm.MsState.Y+
                ")  Composants : "+mainSchem.components.Count()+" Selection : "+selected.Count()+
                "  Fils : "+mainSchem.wires.Count();
        }
    }
}
