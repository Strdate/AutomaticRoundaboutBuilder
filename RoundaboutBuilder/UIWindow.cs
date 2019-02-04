using ColossalFramework.UI;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.1.0 */

namespace RoundaboutBuilder
{
    /* UI Window */

    public class UIWindow : UIPanel
    {
        public static readonly int BUTTON_HEIGHT = 25;

        public bool keepWindowOpen = true;
        private string keepWindowText = "Keep open (On)";
        private Rect windowRect = new Rect(85, 10, 150, 200);

        public static UIWindow Instance { get; private set; }

        private ToolBaseExtended toolOnUI;
        private bool inSettings;

        public bool OldSnappingAlgorithm { get; private set; } = false;
        private string snappingAlgorithmText = "Standard";

        public void ThrowErrorMsg(string content, bool error=false)
        {
            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage("Roundabout builder", content, error);
        }


        public override void Start()
        {
            Instance = this;
            name = "RoundaboutBuilderWindow";
            enabled = false;

            /* Hahaha good programming practice */
            width = 1;
            height = 1;
            
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

        internal void GoToMenu()
        {
            if (toolOnUI != null)
                toolOnUI.enabled = false;
            toolOnUI = null;
            inSettings = false;
        }

        /* Default unity UI method */
        void OnGUI()
        {
            GUI.Window(666, windowRect, _populateWindow, "Roundabout Builder");
        }

        public void IncreaseButton()
        {
            if (toolOnUI != null)
                toolOnUI.IncreaseButton();
        }

        public void DecreaseButton()
        {
            if (toolOnUI != null)
                toolOnUI.DecreaseButton();
        }

        public void LostFocus()
        {
            if (!keepWindowOpen)
            {
                enabled = false;
            }
            if (toolOnUI != null)
                toolOnUI.enabled = false;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (toolOnUI != null)
                toolOnUI.enabled = true;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            inSettings = false;
            if (toolOnUI != null)
                toolOnUI.enabled = false;
        }

        private void _populateWindow(int num)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if(toolOnUI != null)
                toolOnUI.enabled = true;
            }
            GUILayout.BeginVertical();

            /* If user is in menu of one of the tools, we call its own UI Method. Otherwise we draw main menu/settings. */
            if(toolOnUI != null)
            {
                toolOnUI.UIWindowMethod();
                if (GUILayout.Button("Back"))
                {
                    bool buffer = keepWindowOpen; // Little cheat, the window would close without this
                    keepWindowOpen = true;
                    toolOnUI.enabled = false;
                    toolOnUI = null;
                    keepWindowOpen = buffer;
                }
            }
            else
            {
                if(inSettings)
                {
                    drawSettingsMenu();
                }
                else
                {
                    drawMainMenu();
                }
            }
            

            if (GUILayout.Button("Close"))
            {
                enabled = false;
                if(toolOnUI != null)
                    toolOnUI.enabled = false;
            }
            GUILayout.EndVertical();
        }

        private void drawMainMenu()
        {
            if (GUILayout.Button("Roundabout"))
            {
                toolOnUI = RoundaboutTool.Instance;
                toolOnUI.enabled = true;
            }
            else if (GUILayout.Button("Elliptic roundabout"))
            {
                toolOnUI = EllipseTool.Instance;
                toolOnUI.enabled = true;
            }
            GUILayout.Space(2 * BUTTON_HEIGHT);
            if (GUILayout.Button("About"))
            {
                inSettings = true;
            }
            else if (GUILayout.Button(keepWindowText))
            {
                keepWindowOpen = !keepWindowOpen;
                if (keepWindowOpen)
                {
                    keepWindowText = "Keep open (On)";
                }
                else
                {
                    keepWindowText = "Keep open (Off)";
                }
            }
        }

        private void drawSettingsMenu()
        {
            GUILayout.Label("Sanpping algorithm:");
            if (GUILayout.Button(snappingAlgorithmText))
            {
                OldSnappingAlgorithm = !OldSnappingAlgorithm;
                if (OldSnappingAlgorithm)
                {
                    snappingAlgorithmText = "Old";
                }
                else
                {
                    snappingAlgorithmText = "Standard";
                }
            }
            GUILayout.Label("Mod by Strad");
            GUILayout.Label("For more information see workshop page");
            GUILayout.Space(0.5f * BUTTON_HEIGHT);
            if (GUILayout.Button("Back"))
            {
                inSettings = false;
            }
        }

    }
}
