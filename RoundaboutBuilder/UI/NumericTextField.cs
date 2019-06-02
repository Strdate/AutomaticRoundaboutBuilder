using ColossalFramework.UI;
using System;
using UnityEngine;

/* Version BETA 1.2.0 */

/* By Strad, 2019 */

/* Credit to SamsamTS a T__S from whom I copied big chunks of code */

namespace RoundaboutBuilder.UI
{
    public class NumericTextField : UITextField
    {
        private string prevText = "";

        public int Value
        {
            get
            {
                if (int.TryParse(text, out int result)) return result;
                else return -1;
            }
        }

        public const int RADIUS_MAX = 3000;
        public const int RADIUS_MIN = 4;
        public const int INCREMENT = 4;
        public int DefaultRadius = 40;

        public NumericTextField()
        {
            atlas = ResourceLoader.GetAtlas("Ingame");
            size = new Vector2(50, 20);
            padding = new RectOffset(4, 4, 3, 3);
            builtinKeyNavigation = true;
            isInteractive = true;
            readOnly = false;
            horizontalAlignment = UIHorizontalAlignment.Center;
            selectionSprite = "EmptySprite";
            selectionBackgroundColor = new Color32(0, 172, 234, 255);
            normalBgSprite = "TextFieldPanelHovered";
            disabledBgSprite = "TextFieldPanelHovered";
            textColor = new Color32(0, 0, 0, 255);
            disabledTextColor = new Color32(80, 80, 80, 128);
            color = new Color32(255, 255, 255, 255);
            textScale = 0.9f;
            useDropShadow = true;
            text = DefaultRadius.ToString();
        }

        public static bool IsValid(string text)
        {
            return int.TryParse(text, out int result) && IsValid(result);
        }
        public static bool IsValid(int value)
        {
            return value >= RADIUS_MIN && value <= RADIUS_MAX;
        }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            /*Debug.Log("On text changed");

            if (text == prevText) return;*/

            if (int.TryParse(text, out int result) || text == "" )
            {
                prevText = text;
            } 
            else
            {
                text = prevText;
                Unfocus();
            }
        }

        // Increase value on [+] click
        public void Increase()
        {
            int newValue = 0;
            if (!IsValid(Value))
            {
                text = DefaultRadius.ToString();
                return;
            }
            else
            {
                newValue = Convert.ToInt32(Math.Ceiling(new decimal(Value + 1) / new decimal(INCREMENT))) * INCREMENT;
            }
            if (IsValid(newValue)) text = newValue.ToString();
        }

        // Decrease value on [-] click
        public void Decrease()
        {
            int newValue = 0;
            if (!IsValid(Value))
            {
                text = DefaultRadius.ToString();
                return;
            }
            else
            {
                newValue = Convert.ToInt32(Math.Floor(new decimal(Value - 1) / new decimal(INCREMENT))) * INCREMENT;
            }
            if (IsValid(newValue)) text = newValue.ToString();
        }

    }
}
