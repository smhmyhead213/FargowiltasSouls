﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.GameInput;
using Terraria.GameContent.UI.Elements;

namespace FargowiltasSouls.UI
{
    public class UISearchBar : UIElement
    {
        public const int CharacterLimit = 16;
        public const string HintText = "Search...";

        public bool IsEmpty => string.IsNullOrEmpty(Input);

        public UIPanel BackPanel;
        public string Input;
        public bool Focused;
        public int CursorBlinkTimer;
        public bool ShowCursorBlink;

        public delegate void TextChangeDelegate(string oldText, string currentText);
        public event TextChangeDelegate OnTextChange;

        public UISearchBar(int width, int height)
        {
            Width.Set(width, 0);
            Height.Set(height, 0);

            BackPanel = new UIPanel();
            BackPanel.Width.Set(width, 0);
            BackPanel.Height.Set(height, 0);
            BackPanel.BackgroundColor = new Color(22, 25, 55);
            BackPanel.PaddingLeft = BackPanel.PaddingRight = BackPanel.PaddingTop = BackPanel.PaddingBottom = 0;
            Append(BackPanel);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (PlayerInput.Triggers.Current.MouseLeft)
            {
                if (ContainsPoint(Main.MouseScreen))
                {
                    Main.clrInput();
                    Focused = true;
                }
                else
                {
                    Focused = false;
                }
            }

            if (PlayerInput.Triggers.Current.Inventory)
            {
                Focused = false;
            }
        }

        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            base.DrawChildren(spriteBatch);

            PlayerInput.WritingText = Focused;
            Main.LocalPlayer.mouseInterface = Focused;
            if (Focused)
            {
                Main.instance.HandleIME();

                string newInput = Main.GetInputText(Input);
                if (newInput != Input)
                {
                    OnTextChange?.Invoke(Input, newInput);
                    Input = newInput;
                }
            }

            Vector2 position = GetDimensions().Position() + new Vector2(6, 4);
            string displayText = Input ?? "";

            if (string.IsNullOrEmpty(displayText) && !Focused)
            {
                Utils.DrawBorderString(spriteBatch, HintText, position, Color.DarkGray);
            }

            if (Focused && ++CursorBlinkTimer >= 20)
            {
                ShowCursorBlink = !ShowCursorBlink;
                CursorBlinkTimer = 0;
            }

            if (Focused && ShowCursorBlink)
            {
                displayText += "|";
            }

            Utils.DrawBorderString(spriteBatch, displayText, position, Color.White);
        }
    }
}
