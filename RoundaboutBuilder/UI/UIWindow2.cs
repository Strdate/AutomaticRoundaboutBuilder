using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using System;
using UnityEngine;

/* Version RELEASE 1.4.0+ */

/* By Strad, 2019 */

/* Credit to SamsamTS and T__S from whom I copied big chunks of code */

/* Welcome to hell on earth */

namespace RoundaboutBuilder.UI
{
    public class UIWindow2 : UIPanel
    {
        public static UIWindow2 instance;

        public bool keepOpen = true;

        public static readonly SavedBool SavedSetupTmpe = new SavedBool("savedSetupTMPE", RoundAboutBuilder.settingsFileName, true, true);

        public ToolBaseExtended toolOnUI;
        private AbstractPanel m_panelOnUI;
        private AbstractPanel m_lastStandardPanel;

        public RoundAboutPanel P_RoundAboutPanel;
        public EllipsePanel_1 P_EllipsePanel_1;
        public EllipsePanel_2 P_EllipsePanel_2;
        public EllipsePanel_3 P_EllipsePanel_3;
        public TmpeSetupPanel P_TmpeSetupPanel;
        public FreeToolPanel P_FreeToolPanel;

        public HoveringLabel m_hoveringLabel;

        private UIPanel m_topSection;
        private UIPanel m_bottomSection;
        private UIPanel m_setupTmpeSection;
        private UIButton backButton;
        private UIButton closeButton;
        public UIButton undoButton;

        public UINetInfoDropDown dropDown;

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
            if (toolOnUI != null && !m_panelOnUI.IsSpecialWindow)
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

            P_RoundAboutPanel = AddUIComponent<RoundAboutPanel>();
            P_EllipsePanel_1 = AddUIComponent<EllipsePanel_1>();
            P_EllipsePanel_2 = AddUIComponent<EllipsePanel_2>();
            P_EllipsePanel_3 = AddUIComponent<EllipsePanel_3>();
            P_TmpeSetupPanel = AddUIComponent<TmpeSetupPanel>();
            P_FreeToolPanel = AddUIComponent<FreeToolPanel>();
            //P_RoundAboutPanel.height = 104f; // cheat

            // From Elektrix's Road Tools
            UIButton openDescription = AddUIComponent<UIButton>();
            openDescription.relativePosition = new Vector3(width - 24f, 8f);
            openDescription.size = new Vector3(15f, 15f);
            openDescription.normalFgSprite = "ToolbarIconHelp";
            openDescription.name = "RAB_workshopButton";
            openDescription.tooltip = "Roundabout Builder [" + RoundAboutBuilder.VERSION + "] by Strad\nOpen in Steam Workshop";
            SetupButtonStateSprites(ref openDescription, "OptionBase", true);
            if (!PlatformService.IsOverlayEnabled())
            {
                openDescription.isVisible = false;
                openDescription.isEnabled = false;
            }
            openDescription.eventClicked += delegate (UIComponent component, UIMouseEventParameter click)
            {
                if (PlatformService.IsOverlayEnabled() && RoundAboutBuilder.WORKSHOP_FILE_ID != null)
                {
                    PlatformService.ActivateGameOverlayToWorkshopItem(RoundAboutBuilder.WORKSHOP_FILE_ID);
                }
                openDescription.Unfocus();
            };
            // -- Elektrix

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

            m_topSection.height = cummulativeHeight;
            m_topSection.width = width;
            dragHandle.height = cummulativeHeight;

            dropDown = AddUIComponent<UINetInfoDropDown>();
            //dropDown.relativePosition = new Vector2(8, cummulativeHeight);
            dropDown.width = width - 16;
            //cummulativeHeight += dropDown.height + 8;


            /* Bottom section */

            m_setupTmpeSection = AddUIComponent<UIPanel>();
            m_setupTmpeSection.width = 204f;
            m_setupTmpeSection.clipChildren = true;
            var setupTmpe = CreateCheckBox(m_setupTmpeSection);
            setupTmpe.name = "RAB_setupTmpe";
            setupTmpe.label.text = "Set up TMPE";
            setupTmpe.tooltip = "Apply TMPE policies to the roundabout";
            setupTmpe.isChecked = SavedSetupTmpe;
            setupTmpe.relativePosition = new Vector3(8, 0);
            setupTmpe.eventCheckChanged += (c, state) =>
            {
                SavedSetupTmpe.value = state;
            };
            var tmpeButton = CreateButton(m_setupTmpeSection);
            tmpeButton.text = "...";
            tmpeButton.tooltip = "TMPE settings";
            tmpeButton.height = setupTmpe.height;
            tmpeButton.width = 30;
            tmpeButton.relativePosition = new Vector2(width - tmpeButton.width - 8, 0);
            tmpeButton.eventClick += (c, p) =>
            {
                bool holder = this.keepOpen;
                this.keepOpen = true;
                toolOnUI.enabled = false;
                this.keepOpen = holder;
                SwitchWindow(P_TmpeSetupPanel);
            };
            m_setupTmpeSection.height = setupTmpe.height + 8;
            //cummulativeHeight += keepOpen.height + 8;

            cummulativeHeight = 0;
            m_bottomSection = AddUIComponent<UIPanel>();

            var keepOpen = CreateCheckBox(m_bottomSection);
            keepOpen.name = "RAB_keepOpen";
            keepOpen.label.text = "Keep open";
            keepOpen.tooltip = "Window won't close automatically when the tool is unselected";
            keepOpen.isChecked = true;
            keepOpen.relativePosition = new Vector3(8, cummulativeHeight);
            keepOpen.width = 196; // width - padding
            keepOpen.eventCheckChanged += (c, state) =>
            {
                this.keepOpen = state;
            };
            cummulativeHeight += keepOpen.height + 8;

            // Back button
            backButton = CreateButton(m_bottomSection);
            backButton.text = "Back";
            backButton.relativePosition = new Vector2(8, cummulativeHeight);
            backButton.width = width - 16;
            backButton.eventClick += (c, p) =>
            {
                if (backButton.isVisible)
                {
                    if (m_panelOnUI.IsSpecialWindow)
                    {
                        SwitchWindow(m_lastStandardPanel);
                        toolOnUI.enabled = true;
                    }
                    else
                    {
                        toolOnUI.GoToFirstStage();
                        SwitchTool(RoundaboutTool.Instance);
                    }
                }
            };

            cummulativeHeight += backButton.height + 8;

            closeButton = CreateButton(m_bottomSection);
            closeButton.text = "Close";
            closeButton.relativePosition = new Vector2(8, cummulativeHeight);
            closeButton.eventClick += (c, p) =>
            {
                enabled = false;
                if (toolOnUI != null)
                {
                    toolOnUI.enabled = false;
                }
            };

            undoButton = CreateButton(m_bottomSection);
            undoButton.text = "Undo";
            undoButton.tooltip = "Remove last built roundabout (CTRL+Z). Warning: Use only right after the roundabout has been built";
            undoButton.relativePosition = new Vector2(16 + closeButton.width, cummulativeHeight);
            undoButton.isEnabled = false;
            undoButton.eventClick += (c, p) =>
            {
                ModThreading.UndoAction();
            };
            
            cummulativeHeight += closeButton.height + 8;

            m_bottomSection.height = cummulativeHeight;
            m_bottomSection.width = width;

            m_hoveringLabel = AddUIComponent<HoveringLabel>();
            m_hoveringLabel.isVisible = false;

            /* Enable roundabout tool as default */
            SwitchTool(RoundaboutTool.Instance);

            // Is done by modthreading from 1.5.3 on
            /*try
            {
                if(RoundAboutBuilder.ShowUIButton)
                   UIPanelButton.CreateButton();
            }
            catch(Exception e)
            {
                Debug.LogWarning("Failed to create UI button.");
                Debug.LogWarning(e);
            }*/
            

            enabled = false;
        }

        internal void GoBack()
        {
            if (toolOnUI != null)
            {
                toolOnUI.GoToFirstStage();
                toolOnUI.enabled = false;
            }      
            SwitchTool( RoundaboutTool.Instance );
        }

        public void SwitchTool(ToolBaseExtended tool)
        {
            if (tool == toolOnUI) return;

            bool holder = this.keepOpen;
            this.keepOpen = true;
            toolOnUI = tool;

            if (tool is EllipseTool)
            {
                SwitchWindow( P_EllipsePanel_1 );
            }
            else if(tool is RoundaboutTool)
            {
                SwitchWindow( P_RoundAboutPanel );
            }else
            {
                SwitchWindow( P_FreeToolPanel );
            }

            tool.enabled = true;
            this.keepOpen = holder;
        }

        public void SwitchWindow(AbstractPanel panel)
        {
            if (m_panelOnUI != null)
            {
                m_panelOnUI.isVisible = false;
                if (!m_panelOnUI.IsSpecialWindow) m_lastStandardPanel = m_panelOnUI;
            }           

            panel.isVisible = true;
            m_panelOnUI = panel;

            float cumulativeHeight = m_topSection.height;

            if(panel.ShowDropDown)
            {
                dropDown.relativePosition = new Vector2(8, cumulativeHeight);
                cumulativeHeight += dropDown.height + 8;
                dropDown.isVisible = true;
            }
            else
            {
                dropDown.isVisible = false;
            }
            dropDown.Populate(true);

            panel.relativePosition = new Vector2(0, cumulativeHeight);
            cumulativeHeight += panel.height;

            if (ModLoadingExtension.tmpeDetected && panel.ShowTmpeSetup)
            {
                m_setupTmpeSection.relativePosition = new Vector2(0, cumulativeHeight);
                cumulativeHeight += m_setupTmpeSection.height;
                m_setupTmpeSection.isVisible = true;
            }
            else
            {
                m_setupTmpeSection.isVisible = false;
            }

            // THIS IS HELL ON EARTH
            // Adjust position of other buttons and height of the panel if the back button is visible
            if(panel.ShowBackButton)
            {
                if(!backButton.enabled)
                {
                    m_bottomSection.height += backButton.height + 8;
                    closeButton.relativePosition = new Vector2(closeButton.relativePosition.x, closeButton.relativePosition.y + backButton.height + 8);
                    undoButton.relativePosition = new Vector2(undoButton.relativePosition.x, undoButton.relativePosition.y + backButton.height + 8);
                }
                backButton.enabled = true;
            }
            else
            {
                if (backButton.enabled)
                {
                    m_bottomSection.height -= backButton.height + 8;
                    closeButton.relativePosition = new Vector2(closeButton.relativePosition.x, closeButton.relativePosition.y - backButton.height - 8);
                    undoButton.relativePosition = new Vector2(undoButton.relativePosition.x, undoButton.relativePosition.y - backButton.height - 8);
                }
                backButton.enabled = false;
            }

            m_bottomSection.relativePosition = new Vector2(0, cumulativeHeight);
            cumulativeHeight += m_bottomSection.height;

            height = cumulativeHeight;

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
            /*if (m_panelButton != null)
                m_panelButton.FocusSprites();*/
        }

        public override void OnDisable()
        {
            base.OnDisable();
            isVisible = false;
            if (toolOnUI != null)
                toolOnUI.enabled = false;
            /*if (m_panelButton != null)
                m_panelButton.UnfocusSprites();*/
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

            checkBox.playAudioEvents = true;

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
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.canFocus = false;
            button.playAudioEvents = true;

            return button;
        }

        // Ripped from Elektrix
        public static void SetupButtonStateSprites(ref UIButton button, string spriteName, bool noNormal = false)
        {
            button.normalBgSprite = spriteName + (noNormal ? "" : "Normal");
            button.hoveredBgSprite = spriteName + "Hovered";
            button.focusedBgSprite = spriteName + "Focused";
            button.pressedBgSprite = spriteName + "Pressed";
            button.disabledBgSprite = spriteName + "Disabled";
        }
    }
}
