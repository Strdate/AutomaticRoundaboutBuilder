using ColossalFramework.UI;
using UnityEngine;

/* Version BETA 1.2.0 */

/* By Strad, 2019 */

/* Credit to SamsamTS a T__S from whom I copied big chunks of code */

namespace RoundaboutBuilder.UI
{
    public class UIWindow2 : UIPanel
    {
        public static UIWindow2 instance;

        public bool keepOpen = true;

        private ToolBaseExtended toolOnUI;

        private UIPanel m_topSection;
        private UIPanel m_bottomSection;
        private UIButton backButton;
        public UINetInfoDropDown dropDown;

        /* If you have a dropdown and user selects an item which is in the list outside the window boundary, the containsMouse method still returns true.
         * This is a bug in the game itself. I have to use a 'cheat' to sort it out. */
        internal bool MouseInWindowCheat = false;

        //public static UIPanel toolOptionsPanel = null;

        //private UIPanel m_toolOptionsPanel;

        public UIWindow2()
        {
            instance = this;
        }

        public override void Start()
        {
            CreateOptionPanel();
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            base.OnMouseDown(p);
            if (toolOnUI != null)
                toolOnUI.enabled = true; // Reenable the tool when user clicks in the window
        }

        private void CreateOptionPanel()
        {
            name = "RAB_ToolOptionsPanel";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(204, 180);
            absolutePosition = new Vector3(RoundAboutBuilder.savedWindowX.value, RoundAboutBuilder.savedWindowY.value);

            isVisible = false;

            //DebugUtils.Log("absolutePosition: " + absolutePosition);

            eventPositionChanged += (c, p) =>
            {
                if (absolutePosition.x < 0)
                    absolutePosition = RoundAboutBuilder.defWindowPosition;

                Vector2 resolution = GetUIView().GetScreenResolution();

                absolutePosition = new Vector2(
                    Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                    Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

                RoundAboutBuilder.savedWindowX.value = (int)absolutePosition.x;
                RoundAboutBuilder.savedWindowY.value = (int)absolutePosition.y;
            };

            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.width = width;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = parent;

            float cummulativeHeight = 8;

            /* Top section */
            m_topSection = AddUIComponent<UIPanel>();
            m_topSection.relativePosition = new Vector2(0, 0);
            m_topSection.SendToBack();

            UILabel label = m_topSection.AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Roundabout Builder";
            label.relativePosition = new Vector2(8, cummulativeHeight);
            label.SendToBack();
            cummulativeHeight += label.height + 8;

            dragHandle.height = cummulativeHeight;

            dropDown = m_topSection.AddUIComponent<UINetInfoDropDown>();
            dropDown.relativePosition = new Vector2(8, cummulativeHeight);
            dropDown.width = width - 16;
            cummulativeHeight += dropDown.height + 8;

            m_topSection.height = cummulativeHeight;
            m_topSection.width = width;

            /* Bottom section */
            cummulativeHeight = 0;
            m_bottomSection = AddUIComponent<UIPanel>();

            var keepOpen = CreateCheckBox(m_bottomSection);
            keepOpen.name = "RAB_keepOpen";
            keepOpen.label.text = "Keep open";
            keepOpen.tooltip = "Window won't close automatically when the tool is unselected";
            keepOpen.isChecked = true;
            keepOpen.relativePosition = new Vector3(8, cummulativeHeight);
            keepOpen.eventCheckChanged += (c, state) =>
            {
                this.keepOpen = state;
            };
            cummulativeHeight += keepOpen.height + 8;


            var closeButton = CreateButton(m_bottomSection);
            closeButton.text = "Close";
            closeButton.relativePosition = new Vector2(8, cummulativeHeight);
            closeButton.eventClick += (c, p) =>
            {
                enabled = false;
                if (toolOnUI != null)
                    toolOnUI.enabled = false;
            };

            backButton = CreateButton(m_bottomSection);
            backButton.text = "Back";
            backButton.relativePosition = new Vector2(16 + closeButton.width, cummulativeHeight);
            backButton.eventClick += (c, p) =>
            {
                if (backButton.isVisible)
                {
                    SwitchTool(RoundaboutTool.Instance);
                }
            };
            
            cummulativeHeight += closeButton.height + 8;

            m_bottomSection.height = cummulativeHeight;
            m_bottomSection.width = width;

            /* Enable roundabout tool as default */
            SwitchTool(RoundaboutTool.Instance);

            enabled = false;
        }

        internal void GoBack()
        {
            if (toolOnUI != null)
                toolOnUI.enabled = false;
            SwitchTool( RoundaboutTool.Instance );
        }

        public void SwitchTool(ToolBaseExtended tool)
        {
            //if (tool == toolOnUI) return;

            if(toolOnUI != null)
            {
                RemoveUIComponent(toolOnUI.UIPanel);
                Destroy(toolOnUI.UIPanel.gameObject);
            }

            UIPanel toolPanel = AddUIComponent<UIPanel>();
            toolPanel.width = width;
            tool.InitUIComponent(toolPanel);

            toolPanel.relativePosition = new Vector2(0, m_topSection.height);
            m_bottomSection.relativePosition = new Vector2(0, m_topSection.height + toolPanel.height);

            height = m_topSection.height + toolPanel.height + m_bottomSection.height;

            backButton.isVisible = (tool == EllipseTool.Instance);

            tool.enabled = true;
            toolOnUI = tool;
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
            if (!keepOpen)
            {
                enabled = false;
            }
            if (toolOnUI != null)
                toolOnUI.enabled = false;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            isVisible = true;
            if (toolOnUI != null)
                toolOnUI.enabled = true;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            isVisible = false;
            if (toolOnUI != null)
                toolOnUI.enabled = false;
        }

        // cheat, see note at the top
        public new bool containsMouse
        {
            get {
                return (MouseInWindowCheat ? false : base.containsMouse);
            }
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            MouseInWindowCheat = false;
        }

        public void ThrowErrorMsg(string content, bool error = false)
        {
            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage("Roundabout builder", content, error);
        }

        /* The code below was copied from Fine Road Tool and More Shortcuts mod by SamsamTS. Thanks! */

        public static UICheckBox CreateCheckBox(UIComponent parent)
        {
            UICheckBox checkBox = (UICheckBox)parent.AddUIComponent<UICheckBox>();

            checkBox.width = 300f;
            checkBox.height = 20f;
            checkBox.clipChildren = true;

            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.atlas = ResourceLoader.GetAtlas("Ingame");
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).atlas = ResourceLoader.GetAtlas("Ingame");
            ((UISprite)checkBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.text = " ";
            checkBox.label.textScale = 0.9f;
            checkBox.label.relativePosition = new Vector3(22f, 2f);

            return checkBox;
        }

        public static UIButton CreateButton(UIComponent parent)
        {
            UIButton button = (UIButton)parent.AddUIComponent<UIButton>();

            button.atlas = ResourceLoader.GetAtlas("Ingame");
            button.size = new Vector2(90f, 30f);
            button.textScale = 0.9f;
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.canFocus = false;

            return button;
        }
    }
}
