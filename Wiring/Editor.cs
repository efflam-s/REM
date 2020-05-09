using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wiring.Wiring;

namespace Wiring
{
    /// <summary>
    /// La partie éditeur de l'application. Permet d'ouvrir, de créer, de modifier et d'enregistrer des schematics
    /// </summary>
    public class Editor
    {
        private static Texture2D select; // texture du rectangle de selection
        private static MouseCursor scissor, hand, grab, magnifier; // textures des curseurs

        /* Edit : Outil pour faire un peu tout (selection, déplacement, clics composants...)
         * Select : Outil de rectangle de sélection
         * Move : Outil de déplacement de composant
         * Wire : Outil de connexion (création de nouveau fil)
         * Pan : Outil de déplacement panoramique (caméra)
         * Zoom : Outil de zoom/dezoom (clic gauche : zoom, clic droit : dezoom)
         */
        public enum Tool { Edit, Select, Move, Wire, Pan, Zoom }
        public Tool tool;
        public List<Schematic> schemPile; // pile des schematics parents
        public Schematic mainSchem
        {
            get => schemPile[schemPile.Count - 1];
            set => schemPile[schemPile.Count - 1] = value;
        }

        List<Component> selected; // la liste des composants sélectionnés
        public void clearSelection() { selected.Clear(); }
        List<Vector2> relativePositionToMouse; // la liste de leur positions relative à la souris (ou la position relative du composant déplacé en 0)
        Component MovingComp; // le composant déplacé non sélectionné

        InputManager Inpm;

        public Editor()
        {
            tool = Tool.Edit;

            selected = new List<Component>();
            relativePositionToMouse = new List<Vector2>();

            Inpm = new InputManager();
        }
        public void Initialize()
        {
            schemPile = new List<Schematic> { new Schematic("Schematic") };
            mainSchem.Initialize();
        }
        public static void LoadContent(ContentManager Content)
        {
            select = Content.Load<Texture2D>("selectRect");
            scissor = MouseCursor.FromTexture2D(Content.Load<Texture2D>("cursorScissor"), 4, 4);
            hand = MouseCursor.FromTexture2D(Content.Load<Texture2D>("cursorHand"), 16, 16);
            grab = MouseCursor.FromTexture2D(Content.Load<Texture2D>("cursorGrab"), 16, 16);
            magnifier = MouseCursor.FromTexture2D(Content.Load<Texture2D>("cursorMagnifier"), 13, 13);
        }

        public void Update(GameTime gameTime, ref Matrix Camera, Rectangle Window)
        {
            Inpm.Update(mouseState: getTransformedMouseState(Camera, Window));
            // variables utiles pour l'update
            bool isMouseInScreen = Window.Contains(Mouse.GetState().Position);
            Component hoveredComponent = hover(Inpm.MsPosition()); // component pressed
            Wire hoveredWire = hoverWire(Inpm.MsPosition()); // wire pressed

            // Choix de l'outil
            if (Inpm.leftClic == InputManager.ClicState.Up && !Inpm.Alt && !Inpm.Control)
            {
                // pas de clic ni de ctrl ou alt
                if (Inpm.OnPressed(Keys.S))
                    tool = Tool.Edit;

                if (Inpm.OnPressed(Keys.C))
                    tool = Tool.Wire;

                if (Inpm.OnPressed(Keys.H))
                    tool = Tool.Pan;

                if (Inpm.OnPressed(Keys.Z))
                    tool = Tool.Zoom;
            }
            
            // Navigation avec clic roulette
            if (Inpm.middleClic == InputManager.ClicState.Clic)
            {
                Inpm.SaveClic(gameTime);
            }
            else if (Inpm.middleClic == InputManager.ClicState.Down)
            {
                Camera.Translation += new Vector3(Inpm.MsPosition() - Inpm.mousePositionOnClic, 0) * Camera.M11; // pas trouvé de meilleur moyen pour trouver la valeur du zoom que M11
            }
            // Scroll (zoom, deplacement vertical et horizontal)
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
                    // scroll horizontal
                    Camera *= Matrix.CreateTranslation((float)(Inpm.MsState.ScrollWheelValue - Inpm.prevMsState.ScrollWheelValue) / 3, 0, 0);
                else
                    // scroll vertical
                    Camera *= Matrix.CreateTranslation(0, (float)(Inpm.MsState.ScrollWheelValue - Inpm.prevMsState.ScrollWheelValue) / 3, 0);
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
                                clearSelection();
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
                                clearSelection();
                            }
                            else// if (!selected.Contains(hoveredComponent))
                            {
                                // selection d'un composant
                                if (!Inpm.Control && !Inpm.Alt)
                                    clearSelection();
                                if (!Inpm.Alt && !selected.Contains(hoveredComponent))
                                    selected.Add(hoveredComponent);
                                else if (Inpm.Alt && selected.Contains(hoveredComponent))
                                    selected.Remove(hoveredComponent);

                            }
                        }
                        else if (hoveredWire != null)
                        {
                            // Suppression d'un fil (ou d'une partie de fil) (à mettre dans une fonction ?)
                            if (hoveredWire.components.Count() <= 2 || (Inpm.MsPosition() - hoveredWire.Node()).Length() < 5)
                                mainSchem.DeleteWire(hoveredWire);
                            else
                            {
                                Vector2 node = hoveredWire.Node();
                                foreach (Component c in hoveredWire.components)
                                {
                                    if (Wire.touchLine(Inpm.MsPosition(), node, c.plugPosition(hoveredWire)))
                                    {
                                        c.wires[c.wires.IndexOf(hoveredWire)] = new Wire();
                                        c.Update();
                                    }
                                }
                                mainSchem.ReloadWiresFromComponents();
                                hoveredWire.Update();
                            }
                        }
                    }
                }
                else if (Inpm.leftClic == InputManager.ClicState.Down)
                {
                    // Pendant le clic
                    if (hover(Inpm.mousePositionOnClic) != null && hasMoved(Inpm.MsPosition(), Camera) && (MovingComp != null || selected.Contains(hoveredComponent)))
                    {
                        // déplacement d'un composant ou de la sélection
                        tool = Tool.Move;
                    }
                }
                else if (Inpm.leftClic == InputManager.ClicState.Up)
                {
                    // pas de clic
                    if (!Inpm.Alt && !Inpm.Control && Inpm.OnPressed(Keys.Delete) && selected.Count() > 0)
                    {
                        // suppression de la selection
                        foreach (Component c in selected)
                        {
                            mainSchem.DeleteComponent(c);
                            foreach (Wire w in c.wires)
                            {
                                w.Update();
                            }
                        }
                        clearSelection();
                    }
                    if (Inpm.Control && !Inpm.Alt && Inpm.OnPressed(Keys.A))
                    {
                        // selectionner tout
                        clearSelection();
                        foreach (Component c in mainSchem.components)
                        {
                            selected.Add(c);
                        }
                    }
                    if (Inpm.Control && !Inpm.Alt && Inpm.OnPressed(Keys.D) && selected.Count == 1)
                    {
                        // Dupliquer un composant (temporaire ?)
                        AddComponent(selected[0].Copy());
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
                if ((Inpm.leftClic == InputManager.ClicState.Clic && !isMouseInScreen) || (Inpm.leftClic == InputManager.ClicState.Up && Inpm.rightClic == InputManager.ClicState.Clic))
                {
                    // Annulation de création de composant
                    mainSchem.DeleteComponent(MovingComp);
                    MovingComp = null;
                    tool = Tool.Edit;
                }
                if ((Inpm.leftClic == InputManager.ClicState.Down && Inpm.rightClic == InputManager.ClicState.Clic))
                {
                    // Annulation de déplacement de composant
                    if (MovingComp != null)
                    {
                        // composant seul, non sélectionné
                        MovingComp.position = Inpm.mousePositionOnClic + relativePositionToMouse[0];
                    }
                    else
                    {
                        // sélection
                        for (int i = 0; i < selected.Count; i++)
                            selected[i].position = Inpm.mousePositionOnClic + relativePositionToMouse[i];
                    }
                    MovingComp = null;
                    tool = Tool.Edit;
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
                    // stockage des infos importantes du clic (position + temps)
                    Inpm.SaveClic(gameTime);
                }
                else if (Inpm.leftClic == InputManager.ClicState.Declic)
                {
                    if (!hasMoved(Inpm.MsPosition(), Camera) && hoveredComponent != null)
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
                            clearSelection();
                        }
                    }
                    else if ((hover(Inpm.MsPosition(), true) != null || hoveredWire != null) && (hover(Inpm.mousePositionOnClic, true) != null || hoverWire(Inpm.mousePositionOnClic) != null))
                    {
                        // Ajout d'un fil
                        Wire start = (hover(Inpm.mousePositionOnClic, true) != null) ?
                            hover(Inpm.mousePositionOnClic, true).nearestPlugWire(Inpm.mousePositionOnClic) :
                            hoverWire(Inpm.mousePositionOnClic);
                        Wire end = (hover(Inpm.MsPosition(), true) != null) ?
                            hover(Inpm.MsPosition(), true).nearestPlugWire(Inpm.MsPosition()) :
                            hoveredWire;

                        if (start != end && start != null && end != null)
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
                                bool updated = start.Update();
                                if (!updated)
                                {
                                    foreach (Component c in start.components)
                                        c.Update();
                                }
                            }
                        }
                    }
                }
            }
            else if (tool == Tool.Pan)
            {
                if (Inpm.leftClic == InputManager.ClicState.Clic)
                {
                    Inpm.SaveClic(gameTime);
                }
                else if (Inpm.leftClic == InputManager.ClicState.Down)
                {
                    Camera.Translation += new Vector3(Inpm.MsPosition() - Inpm.mousePositionOnClic, 0) * Camera.M11; // pas trouvé de meilleur moyen pour trouver la valeur du zoom que M11
                }
            }
            else if (tool == Tool.Zoom && isMouseInScreen)
            {
                if (Inpm.leftClic == InputManager.ClicState.Clic)
                {
                    // Zoom avec clic
                    float scale = (float)Math.Pow(2, 120.0 / 720);
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
                if (Inpm.rightClic == InputManager.ClicState.Clic)
                {
                    // Dezoom avec clic draoit
                    float scale = (float)Math.Pow(2, -120.0 / 720);
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
            }

            //prevKbState = KbState;
            //prevMsState = MsState;

            //if ((gameTime.TotalGameTime - gameTime.ElapsedGameTime).TotalMilliseconds % 2000 > gameTime.TotalGameTime.TotalMilliseconds % 2000)
            mainSchem.Update(gameTime);

            // debug
            /*foreach (Component c in selected)
            {
                if (c is Output o)
                    Console.WriteLine(gameTime.TotalGameTime.Milliseconds + " " + o.GetValue());
            }*/
        }
        /// <summary>
        /// Transforme le position de la souris en fonction de la camra et des bordures de l'éditeur
        /// </summary>
        private MouseState getTransformedMouseState(Matrix Camera, Rectangle Window)
        {
            MouseState mouseState = Mouse.GetState();
            Vector2 virtualMousePosition = new Vector2(minMax(mouseState.X, Window.X, Window.X + Window.Width), minMax(mouseState.Y, Window.Y, Window.Y + Window.Height));
            virtualMousePosition = Vector2.Transform(virtualMousePosition, Matrix.Invert(Camera));
            return new MouseState((int)Math.Floor(virtualMousePosition.X), (int)Math.Floor(virtualMousePosition.Y), mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2);
        }

        /// <summary>
        /// Détermine le composant sur lequel est positionné la souris. Retourne null si pas de composant trouvé
        /// </summary>
        public Component hover(Vector2 position, bool includePlugs = false)
        {
            foreach (Component c in mainSchem.components)
            {
                if (c.touch(position, includePlugs))
                    return c;
            }
            return null;
        }
        public Component currentHoveredComponent()
        {
            return hover(Inpm.MsPosition());
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

            if (tool == Tool.Wire && (hover(Inpm.mousePositionOnClic, true) != null || hoverWire(Inpm.mousePositionOnClic) != null) && Inpm.leftClic == InputManager.ClicState.Down)
            {
                // Ajout d'un fil
                Vector2 start, end;

                Component hoveredClic = hover(Inpm.mousePositionOnClic, true); // au début du clic
                Wire hoveredWireClic = hoverWire(Inpm.mousePositionOnClic);
                if (hoveredClic != null)
                    start = hoveredClic.plugPosition(hoveredClic.nearestPlugWire(Inpm.mousePositionOnClic));
                else if (hoveredWireClic != null)
                    start = hoveredWireClic.Node();
                else
                    throw new InvalidOperationException("pas de fil ou de composant sélectionné pour le début de la création du fil");

                Component hovered = hover(Inpm.MsPosition(), true);
                Wire hoveredWire = hoverWire(Inpm.MsPosition());
                if (hovered != null && hovered.nearestPlugWire(Inpm.MsPosition()) != null)
                    end = hovered.plugPosition(hovered.nearestPlugWire(Inpm.MsPosition()));
                else if (hoveredWire != null)
                    end = hoveredWire.Node();
                else
                    end = Inpm.MsPosition();

                Wire.drawLine(spriteBatch, start, end, hovered != null || hoveredWire != null);
            }

            if (Window.Contains(Mouse.GetState().Position))
            {
                if (Inpm.middleClic == InputManager.ClicState.Down)
                    // panoramique avec middleclic
                    Mouse.SetCursor(grab);
                else
                    switch (tool)
                    {
                        // Choix du curseur
                        case Tool.Edit:
                            Component hovered = hover(Inpm.MsPosition());
                            if (!Inpm.Control && !Inpm.Alt &&
                                    (hovered is Input || hovered is Diode || hovered is BlackBox))
                                Mouse.SetCursor(MouseCursor.Hand);
                            else if (!Inpm.Control &&
                                    hoverWire(Inpm.MsPosition()) != null && hovered == null)
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
                        case Tool.Zoom:
                            Mouse.SetCursor(magnifier);
                            break;
                        case Tool.Pan:
                            if (Inpm.leftClic == InputManager.ClicState.Down)
                                Mouse.SetCursor(grab);
                            else
                                Mouse.SetCursor(hand);
                            break;
                        case Tool.Wire:
                            hovered = hover(Inpm.MsPosition());
                            if (!Inpm.Control && !Inpm.Alt &&
                                (hovered is Input || hovered is Diode || hovered is BlackBox))
                                Mouse.SetCursor(MouseCursor.Hand);
                            else
                                Mouse.SetCursor(MouseCursor.Arrow);
                            break;
                        default:
                            Mouse.SetCursor(MouseCursor.Arrow);
                            break;
                    }
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
            clearSelection();
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
