using ColossalFramework.UI;
using System;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.0.0 */

namespace RoundaboutBuilder
{
    /* UI Window */

    class UIWindow : UIPanel
    {
        private static readonly int RADIUS_MAX = 500;
        private static readonly int RADIUS_MIN = 5;
        private static readonly int RADIUS_DEF = 40;

        private string textAreaString = RADIUS_DEF.ToString();
        private bool keepWindowOpen = false;
        private string keepWindowText = "Keep open (off)";
        private Rect windowRect = new Rect(85, 10, 150, 110);

        public static UIWindow Instance { get; private set; }

        public int Radius
        {
            get {
                int i = 0;
                if (!Int32.TryParse(textAreaString, out i) || !IsInBounds(i))
                {
                    //Debug.Log(string.Format("parse {0} bounds {1}", Int32.TryParse(textAreaString, out i), IsInBounds(i)));
                    return -1;
                }
                return i;
            }       
        }

        private bool IsInBounds( int radius )
        {
            return radius >= RADIUS_MIN && radius <= RADIUS_MAX;
        }

        public void ThrowErrorMsg(string content)
        {
            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage("Roundabout builder", content, false);
        }

        public void IncreaseRadius()
        {
            int newValue = 0;
            int value = Radius;
            if (!IsInBounds(value))
            {
                textAreaString = RADIUS_DEF.ToString();
                return;
            }
            else
            {
                newValue = Convert.ToInt32(Math.Ceiling(new decimal(value+1)/new decimal(5)))*5;
                //Debug.Log(string.Format("decimal {0} int {1}", ((new decimal((value + 1) / 5)) * 5), newValue));
            }
            if (IsInBounds(newValue)) textAreaString = newValue.ToString(); else value.ToString();
        }

        public void DecreaseRadius()
        {
            int newValue = 0;
            int value = Radius;
            if (!IsInBounds(value))
            {
                textAreaString = RADIUS_DEF.ToString();
                return;
            }
            else
            {
                newValue = (int)(Math.Floor(new Decimal((value - 1) / 5))*5);
            }
            if (IsInBounds(newValue)) textAreaString = newValue.ToString(); else value.ToString();

        }

        public override void Start()
        {
            Instance = this;
            name = "RoundaboutBuilderWindow";
            enabled = false;

            // At the beginning I started to create UI using the ColossalFramework.UI, but didn't manage to make it work. Thus I stuck to usual Unity UI.

            /*backgroundSprite = "GenericPanel";
            absolutePosition = new Vector3(80, 10);
            width = 1100;
            height = 1000;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            padding = new RectOffset(3, 3, 3, 3);

            UILabel nameLabel = this.AddUIComponent<UILabel>();
            nameLabel.text = "Roundabout Builder";
            UILabel radiusLabel = this.AddUIComponent<UILabel>();
            nameLabel.text = "Radius: ";
            UITextField radiusText = this.AddUIComponent<UITextField>();
            radiusText.text = "50";
            radiusText.numericalOnly = true;
            UIButton closeButton = this.AddUIComponent<UIButton>();
            closeButton.text = "Close";*/
            //Debug.Log("Window set up");
        }

        /* Default unity UI method */
        void OnGUI()
        {
            GUI.Window(666, windowRect, _populateWindow, "Roundabout Builder");
        }

        public void LostFocus()
        {
            if (!keepWindowOpen)
            {
                enabled = false;
                NodeSelection.instance.enabled = false;
            }
        }

        private void _populateWindow(int num)
        {
            Event e = Event.current;
            //Debug.Log("event: " + e.ToString());
            //Debug.Log("is mouse donwn: " + (e.type == EventType.MouseDown).ToString() + "; is inside rect: " + windowRect.Contains(e.mousePosition).ToString());
            if (e.type == EventType.MouseDown)
            {
                NodeSelection.instance.enabled = true;
                //Debug.Log("is mouse donwn: " + (e.type == EventType.MouseDown).ToString() + "; ismouse: " + e.isMouse.ToString() + "; isup: " + (e.type == EventType.MouseUp).ToString());
                //Debug.Log("window click");
            }
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius:");
            textAreaString = GUILayout.TextField(textAreaString);
            GUILayout.EndHorizontal();
            if (GUILayout.Button(keepWindowText))
            {
                //Debug.Log("keep open clicked");
                keepWindowOpen = !keepWindowOpen;
                if(keepWindowOpen)
                {
                    keepWindowText = "Keep open (On)";
                }
                else
                {
                    keepWindowText = "Keep open (Off)";
                }
            }

            if (GUILayout.Button("Close"))
            {
                enabled = false;
                NodeSelection.instance.enabled = false;
            }
            GUILayout.EndVertical();
        }

    }
}
