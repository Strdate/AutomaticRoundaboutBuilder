using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* This was mostly copied from Precision Engineering by Simie. Thanks! */

namespace RoundaboutBuilder.UI
{
    public class HoveringLabel : UILabel
    {
        public void SetValue(string v)
        {
            text = string.Format("<color=#87d3ff>{0}</color>", v);
        }

        /*public void SetWorldPosition(RenderManager.CameraInfo camera, Vector3 worldPos)
        {
            var uiView = GetUIView();

            var vector3_1 = Camera.main.WorldToScreenPoint(worldPos) / uiView.inputScale;
            var vector3_3 = uiView.ScreenPointToGUI(vector3_1) - new Vector2(size.x * 0.5f, size.y + 10  * 0.5f );
            // + new Vector2(vector3_2.x, vector3_2.y);

            relativePosition = vector3_3;
            textScale = 0.65f; // 0.65f 0.8f 1.1f
        }*/

        public override void Start()
        {
            base.Start();

            backgroundSprite = "CursorInfoBack";
            autoSize = true;
            padding = new RectOffset(5, 5, 5, 5);
            textScale = 0.65f;
            textAlignment = UIHorizontalAlignment.Center;
            verticalAlignment = UIVerticalAlignment.Middle;
            zOrder = 100;

            pivot = UIPivotPoint.MiddleCenter;

            color = new Color32(255, 255, 255, 190);
            processMarkup = true;

            isInteractive = false;

            //<color #87d3ff>Construction cost: 520</color>
        }
    }
}
