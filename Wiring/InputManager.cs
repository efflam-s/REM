using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Wiring
{
    public class InputManager
    {
        public MouseState MsState, prevMsState;
        public KeyboardState KbState, prevKbState;
        public Vector2 mousePositionOnClic;
        public TimeSpan timeOnClic;

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

        }
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

        public void SaveClic(GameTime gameTime)
        {
            // stockage des infos importantes du clic (position + temps)
            mousePositionOnClic = MsPosition();
            timeOnClic = gameTime.TotalGameTime;
        }
    }
}
