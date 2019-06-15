using ColossalFramework.UI;
using System;
using UnityEngine;

/* Version RELEASE 1.4.0+ */

/* By Strad, 2019 */

/* Credit to SamsamTS a T__S from whom I copied big chunks of code */

namespace RoundaboutBuilder.UI
{
    public class NumericTextField : UITextField
    {
        private string prevText = "";

        public float? Value
        {
            get
            {
                if (float.TryParse(text, out float result) && IsValid(result)) return result;
                else return null;
            }
        }

        public float MaxVal = 3000f;
        public float MinVal = 4f;
        public int Increment = 8;
        public float DefaultVal = 40f;

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
            text = DefaultVal.ToString();
        }

        private bool IsValid(string text)
        {
            return float.TryParse(text, out float result) && IsValid(result);
        }
        public bool IsValid(float value)
        {
            return value >= MinVal && value <= MaxVal;
        }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            /*Debug.Log("On text changed");

            if (text == prevText) return;*/

            if (int.TryParse(text, out int result) || text == "" || text == "-" )
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
            if (Value == null)
            {
                text = DefaultVal.ToString();
                return;
            }
            else
            {
                newValue = Convert.ToInt32(Math.Ceiling( (double)(Value + 1) / (Increment))) * Increment;
            }
            if (IsValid(newValue)) text = newValue.ToString();
        }

        // Decrease value on [-] click
        public void Decrease()
        {
            int newValue = 0;
            if (Value == null)
            {
                text = DefaultVal.ToString();
                return;
            }
            else
            {
                newValue = Convert.ToInt32(Math.Floor((double)(Value - 1) / (Increment))) * Increment;
            }
            if (IsValid(newValue)) text = newValue.ToString();
        }

    }
}
