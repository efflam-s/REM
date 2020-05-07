using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Wiring.UI
{
    /// <summary>
    /// Un bouton cliquable et tooglable ayant un Tooltip
    /// </summary>
    public class Button : UIObject
    {
        public static Texture2D hoverTex, toggleTex;
        public bool toggle, drawHover;
        public ToolTip toolTip;

        public Button(Rectangle? Bounds, string toolTip = "", bool drawHover = true) : base(Bounds)
        {
            toggle = false;
            this.toolTip = new ToolTip(toolTip, this.Bounds.Location + new Point(this.Bounds.Width / 2, this.Bounds.Height) + new Point(-13, 2));
            this.drawHover = drawHover;
        }
        public Button(Point? Position, string toolTip = "", bool drawHover = true) : base(Position)
        {
            toggle = false;
            this.toolTip = new ToolTip(toolTip, Bounds.Location + new Point(Bounds.Width / 2, Bounds.Height) + new Point(-13, 2));
            this.drawHover = drawHover;
        }
        public static new void LoadContent(ContentManager Content)
        {
            hoverTex = Content.Load<Texture2D>("Button/buttonHover");
            toggleTex = Content.Load<Texture2D>("Button/buttonToggle");
            StringButton.editCursor = Content.Load<Texture2D>("EditBar");
            StringButton.textColor = getFirstColor(Content.Load<Texture2D>("Button/textColor"));
            StringButton.font = Content.Load<SpriteFont>("Arial");
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Contains(Mouse.GetState().Position) && drawHover)
            {
                spriteBatch.Draw(hoverTex, Bounds, Color.White);
                
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                    spriteBatch.Draw(toggleTex, Bounds, Color.White);
            }
            else if (toggle)
            {
                spriteBatch.Draw(toggleTex, Bounds, Color.White);
            }
        }
        public void DrawToolTip(SpriteBatch spriteBatch)
        {
            if (Contains(Mouse.GetState().Position))
            {
                toolTip.Draw(spriteBatch);
            }
        }
        public override void SetPosition(Point position)
        {
            base.SetPosition(position);
            toolTip.Position = Bounds.Location + new Point(Bounds.Width / 2, Bounds.Height) + new Point(-13, 2);
        }
        public override void SetSize()
        {
            toolTip.Position = Bounds.Location + new Point(Bounds.Width / 2, Bounds.Height) + new Point(-13, 2);
            base.SetSize();
        }


        internal static Color getFirstColor(Texture2D texture)
        {
            // sera probablement suppr plus tard
            Color[] colors = new Color[1];
            texture.GetData<Color>(colors);
            return colors[0];
        }
    }
}
