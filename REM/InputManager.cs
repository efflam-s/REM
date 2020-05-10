using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace REM
{
    /// <summary>
    /// Un ensemble d'élements utiles pour les entrées souris et clavier
    /// </summary>
    public class InputManager
    {
        public MouseState MsState;
        public MouseState prevMsState { get; private set; }
        public KeyboardState KbState;
        public KeyboardState prevKbState { get; private set; }
        public Vector2 mousePositionOnClic { get; private set; }
        public TimeSpan timeOnClic { get; private set; } // actuellement non utilisé

        public Vector2 MsPosition() => MsState.Position.ToVector2();
        public Vector2 MsPosition(MouseState mouseState) => mouseState.Position.ToVector2();
        public bool Control => KbState.IsKeyDown(Keys.LeftControl) || KbState.IsKeyDown(Keys.RightControl);
        public bool Shift => KbState.IsKeyDown(Keys.LeftShift) || KbState.IsKeyDown(Keys.RightShift);
        public bool Alt => KbState.IsKeyDown(Keys.LeftAlt);
        public bool OnPressed(Keys keys) => KbState.IsKeyDown(keys) && prevKbState.IsKeyUp(keys);

        public enum ClicState { Up, Clic, Down, Declic }
        public ClicState leftClic, rightClic, middleClic;

        public InputManager()
        {
            MsState = new MouseState();
            KbState = new KeyboardState();
            mousePositionOnClic = new Vector2();
            timeOnClic = new TimeSpan();
        }
        /// <summary>
        /// A excécuter au début d'un Update(), avant d'utiliser ses variables
        /// </summary>
        /// <param name="keyboardState">L'état du clavier à utiliser, prendra Keyboard.GetState() si non donné</param>
        /// <param name="mouseState">L'état de la souris à utiliser, prendra Mouse.GetState() si non donné</param>
        public void Update(KeyboardState? keyboardState = null, MouseState? mouseState = null)
        {
            // mise à jour des inputs
            prevMsState = MsState;
            prevKbState = KbState;
            MsState = mouseState == null ? Mouse.GetState() : (MouseState)mouseState;
            KbState = keyboardState == null ? Keyboard.GetState() : (KeyboardState)keyboardState;

            // get left clic state
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

            // get middle clic state
            if (MsState.MiddleButton == ButtonState.Pressed)
                if (prevMsState.MiddleButton == ButtonState.Pressed)
                    middleClic = ClicState.Down;
                else
                    middleClic = ClicState.Clic;
            else
                if (prevMsState.MiddleButton == ButtonState.Pressed)
                    middleClic = ClicState.Declic;
                else
                    middleClic = ClicState.Up;

            // get right clic state
            if (MsState.RightButton == ButtonState.Pressed)
                if (prevMsState.RightButton == ButtonState.Pressed)
                    rightClic = ClicState.Down;
                else
                    rightClic = ClicState.Clic;
            else
                if (prevMsState.RightButton == ButtonState.Pressed)
                    rightClic = ClicState.Declic;
                else
                    rightClic = ClicState.Up;
        }

        /// <summary>
        /// Stockage des infos importantes du clic (position + temps)
        /// </summary>
        /// <param name="gameTime"></param>
        public void SaveClic(GameTime gameTime)
        {
            mousePositionOnClic = MsPosition();
            timeOnClic = gameTime.TotalGameTime;
        }
    }
}
