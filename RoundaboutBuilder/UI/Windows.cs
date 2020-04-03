using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

/* Version RELEASE 1.4.0+ */

namespace RoundaboutBuilder.UI
{
    public abstract class AbstractPanel : UIPanel
    {
        public const float RADIUS_MAX_VAL = 2000f;
        public const float RADIUS_MAX_VAL_UNLIMITED = 20000f;

        public abstract bool ShowDropDown { get; }
        public abstract bool ShowFollowTerrain { get; }
        public abstract bool ShowTmpeSetup { get; }
        public abstract bool ShowBackButton { get; }
        public abstract bool IsSpecialWindow { get; }

        public AbstractPanel()
        {
            isVisible = false;
            width = 204;
            height = 218;
            clipChildren = true;
        }
    }

    public class RoundAboutPanel : AbstractPanel
    {
        public override bool ShowDropDown => true;
        public override bool ShowFollowTerrain => true;
        public override bool ShowTmpeSetup => true;
        public override bool ShowBackButton => false;
        public override bool IsSpecialWindow => false;

        public NumericTextField RadiusField { get; private set; }
        public UILabel label { get; private set; }

        public override void Start()
        {
            float cumulativeHeight = 0;
            UILabel labelRadius = AddUIComponent<UILabel>();
            labelRadius.textScale = 0.9f;
            labelRadius.text = "Radius:";
            labelRadius.relativePosition = new Vector2(8, cumulativeHeight);
            labelRadius.tooltip = "Press +/- to adjust";
            labelRadius.SendToBack();

            RadiusField = AddUIComponent<NumericTextField>();
            RadiusField.relativePosition = new Vector2(width - RadiusField.width - 8, cumulativeHeight);
            RadiusField.tooltip = "Press +/- to adjust";
            RadiusField.MaxVal = RoundAboutBuilder.UnlimitedRadius.value ? RADIUS_MAX_VAL_UNLIMITED : RADIUS_MAX_VAL;
            cumulativeHeight += RadiusField.height + 8;

            UIButton button = UIUtil.CreateButton(this);
            button.text = "Free Cursor Mode...";
            button.tooltip = "Create roundabouts anywhere (Warning! Roads won't be removed or connected)";
            button.relativePosition = new Vector2(8, cumulativeHeight);
            button.width = width - 16;
            button.eventClick += (c, p) =>
            {
                UIWindow.instance.SwitchTool(FreeCursorTool.Instance);
            };
            cumulativeHeight += button.height + 8;

            if(RoundAboutBuilder.LegacyEllipticRoundabouts.value)
            {
                button = UIUtil.CreateButton(this);
                button.text = "Elliptic Roundabout...";
                button.relativePosition = new Vector2(8, cumulativeHeight);
                button.width = width - 16;
                button.eventClick += (c, p) =>
                {
                    UIWindow.instance.SwitchTool(EllipseTool.Instance);
                };
                cumulativeHeight += button.height + 8;
            }            

            label = AddUIComponent<UILabel>();
            label.text = "Tip: Use Fine Road Tool for elevated roads";
            label.wordWrap = true;
            label.textScale = 0.9f;
            label.autoSize = false;
            label.width = width - 16;
            label.height = 48;
            label.relativePosition = new Vector2(8, cumulativeHeight);
            label.SendToBack();
            cumulativeHeight += label.height;

            height = cumulativeHeight;

            UIWindow.instance.SwitchWindow(this);
        }
    }

    public class FreeToolPanel : AbstractPanel
    {
        public override bool ShowDropDown => true;
        public override bool ShowFollowTerrain => true;
        public override bool ShowTmpeSetup => false;
        public override bool ShowBackButton => true;
        public override bool IsSpecialWindow => false;

        public NumericTextField RadiusField { get; private set; }
        public NumericTextField ElevationField { get; private set; }

        public override void Start()
        {
            float cumulativeHeight = 0;
            UILabel labelRadius = AddUIComponent<UILabel>();
            labelRadius.textScale = 0.9f;
            labelRadius.text = "Radius:";
            labelRadius.relativePosition = new Vector2(8, cumulativeHeight);
            labelRadius.tooltip = "Press +/- to adjust";
            labelRadius.SendToBack();

            RadiusField = AddUIComponent<NumericTextField>();
            RadiusField.relativePosition = new Vector2(width - RadiusField.width - 8, cumulativeHeight);
            RadiusField.tooltip = "Press +/- to adjust";
            RadiusField.MaxVal = RoundAboutBuilder.UnlimitedRadius.value ? RADIUS_MAX_VAL_UNLIMITED : RADIUS_MAX_VAL;
            cumulativeHeight += RadiusField.height + 8;

            UILabel labelElevation = AddUIComponent<UILabel>();
            labelElevation.textScale = 0.9f;
            labelElevation.text = "Elevation:";
            labelElevation.relativePosition = new Vector2(8, cumulativeHeight);
            labelElevation.tooltip = "Press PgUp/PgDn to adjust";
            labelElevation.SendToBack();

            ElevationField = AddUIComponent<NumericTextField>();
            ElevationField.relativePosition = new Vector2(width - ElevationField.width - 8, cumulativeHeight);
            ElevationField.tooltip = "Press PgUp/PgDn to adjust";
            ElevationField.MinVal = -500f;
            ElevationField.MaxVal = 1000f;
            ElevationField.Increment = 3;
            ElevationField.DefaultVal = 0;
            ElevationField.text = "0";
            cumulativeHeight += ElevationField.height + 8;

            UILabel label = AddUIComponent<UILabel>();
            label.text = "Roads won't be removed or connected\nUse Fine Road Tool for elevated roads";
            label.wordWrap = true;
            label.textScale = 0.9f;
            label.autoSize = false;
            label.width = width - 16;
            label.height = 96;
            label.relativePosition = new Vector2(8, cumulativeHeight);
            label.SendToBack();
            cumulativeHeight += label.height;

            var absoluteElevation = UIUtil.CreateCheckBox(this);
            absoluteElevation.name = "RAB_absoluteElevation";
            absoluteElevation.label.text = "Absolute elevation";
            absoluteElevation.tooltip = "Elevation will be measured from zero level instead of terrain level";
            absoluteElevation.isChecked = EllipseTool.Instance.ControlVertices;
            absoluteElevation.relativePosition = new Vector3(8, cumulativeHeight);
            absoluteElevation.isChecked = false;
            absoluteElevation.eventCheckChanged += (c, state) =>
            {
                FreeCursorTool.Instance.AbsoluteElevation = state;
            };
            cumulativeHeight += absoluteElevation.height + 8;

            height = cumulativeHeight;
        }
    }

    public class EllipsePanel_1 : AbstractPanel
    {
        public override bool ShowDropDown => true;
        public override bool ShowFollowTerrain => false;
        public override bool ShowTmpeSetup => false;
        public override bool ShowBackButton => true;
        public override bool IsSpecialWindow => false;

        public override void Start()
        {
            UILabel label = AddUIComponent<UILabel>();
            label.text = "Step 1/3:\nSelect center of the elliptic roundabout (Nothing will be built yet)";
            label.wordWrap = true;
            label.textScale = 0.9f;
            label.autoSize = false;
            label.width = width - 16;
            label.relativePosition = new Vector2(8, 0);
            label.SendToBack();
            height = 150;
        }
    }

    public class EllipsePanel_2 : AbstractPanel
    {
        public override bool ShowDropDown => true;
        public override bool ShowFollowTerrain => false;
        public override bool ShowTmpeSetup => false;
        public override bool ShowBackButton => true;
        public override bool IsSpecialWindow => false;

        public override void Start()
        {
            UILabel label = AddUIComponent<UILabel>();
            label.text = "Step 2/3:\nSelect any intersection on the main axis of the roundabout (Nothing will be built yet)";
            label.wordWrap = true;
            label.textScale = 0.9f;
            label.autoSize = false;
            label.width = width - 16;
            label.relativePosition = new Vector2(8, 0);
            label.SendToBack();
            height = 150;
        }
    }

    public class EllipsePanel_3 : AbstractPanel
    {
        public override bool ShowDropDown => true;
        public override bool ShowFollowTerrain => true;
        public override bool ShowTmpeSetup => true;
        public override bool ShowBackButton => true;
        public override bool IsSpecialWindow => false;

        public static readonly int RADIUS1_DEF = 60;
        public static readonly int RADIUS2_DEF = 40;

        public NumericTextField Radius1tf { get; private set; }
        public NumericTextField Radius2tf { get; private set; }

        public override void Start()
        {
            float cumulativeHeight = 0;

            UILabel labelRadius = AddUIComponent<UILabel>();
            labelRadius.textScale = 0.9f;
            labelRadius.text = "Main axis:";
            labelRadius.relativePosition = new Vector2(8, cumulativeHeight);
            labelRadius.tooltip = "Press SHIFT +/- to adjust";
            labelRadius.SendToBack();

            Radius1tf = AddUIComponent<NumericTextField>();
            Radius1tf.relativePosition = new Vector2(width - Radius1tf.width - 8, cumulativeHeight);
            Radius1tf.tooltip = "Press SHIFT +/- to adjust";
            Radius1tf.DefaultVal = RADIUS1_DEF;
            Radius1tf.MaxVal = RoundAboutBuilder.UnlimitedRadius.value ? RADIUS_MAX_VAL_UNLIMITED : RADIUS_MAX_VAL;
            Radius1tf.text = RADIUS1_DEF.ToString();
            cumulativeHeight += Radius1tf.height + 8;

            labelRadius = AddUIComponent<UILabel>();
            labelRadius.textScale = 0.9f;
            labelRadius.text = "Minor axis:";
            labelRadius.relativePosition = new Vector2(8, cumulativeHeight);
            labelRadius.tooltip = "Press CTRL +/- to adjust";
            labelRadius.SendToBack();

            Radius2tf = AddUIComponent<NumericTextField>();
            Radius2tf.relativePosition = new Vector2(204 - Radius1tf.width - 8, cumulativeHeight);
            Radius2tf.tooltip = "Press CTRL +/- to adjust";
            Radius2tf.DefaultVal = RADIUS2_DEF;
            Radius2tf.MaxVal = RoundAboutBuilder.UnlimitedRadius.value ? RADIUS_MAX_VAL_UNLIMITED : RADIUS_MAX_VAL;
            Radius2tf.text = RADIUS2_DEF.ToString();
            cumulativeHeight += Radius2tf.height + 8;

            var buildButton = UIUtil.CreateButton(this);
            buildButton.text = "Build";
            buildButton.relativePosition = new Vector2(8, cumulativeHeight);
            buildButton.playAudioEvents = false;
            buildButton.eventClick += (c, p) =>
            {
                EllipseTool.Instance.BuildEllipse();
            };
            cumulativeHeight += buildButton.height + 8;

            var controlVertices = UIUtil.CreateCheckBox(this);
            controlVertices.name = "RAB_controlVertices";
            controlVertices.label.text = "Insert control points";
            controlVertices.tooltip = "Control points are inserted on main axes to keep the ellipse in shape. See workshop page";
            controlVertices.isChecked = EllipseTool.Instance.ControlVertices;
            controlVertices.relativePosition = new Vector3(8, cumulativeHeight);
            controlVertices.eventCheckChanged += (c, state) =>
            {
                EllipseTool.Instance.ControlVertices = state;
            };
            cumulativeHeight += controlVertices.height + 8;

            height = cumulativeHeight;
        }
    }

    public class TmpeSetupPanel : AbstractPanel
    {
        public override bool ShowDropDown => false;
        public override bool ShowFollowTerrain => false;
        public override bool ShowTmpeSetup => false;
        public override bool ShowBackButton => true;
        public override bool IsSpecialWindow => true;

        public static readonly SavedBool SavedEnterBlockedYieldingRoad = new SavedBool("tmpeEnterBlockedJunction", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool SavedEnterBlockedMainRoad = new SavedBool("tmpeEnterBlockedJunctionMainRoad", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool SavedNoParking = new SavedBool("tmpeNoParking", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool SavedPrioritySigns = new SavedBool("tmpePrioritySigns", RoundAboutBuilder.settingsFileName, false, true);
        public static readonly SavedBool SavedNoCrossings = new SavedBool("tmpeNoCrossings", RoundAboutBuilder.settingsFileName, false, true);
        public static readonly SavedBool SavedAllowLaneChanging = new SavedBool("tmpeLaneChanging", RoundAboutBuilder.settingsFileName, false, true);

        public override void Start()
        {
            float cumulativeHeight = 8;

            UILabel label = AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.width = width;
            label.text = "TMPE settings";
            label.relativePosition = new Vector2(8, cumulativeHeight);
            label.SendToBack();
            cumulativeHeight += label.height + 8;

            var checkBox = UIUtil.CreateCheckBox(this);
            checkBox.name = "RAB_enterBlockedJunctionMainRoad";
            checkBox.label.text = "Enter junct. main r.";
            checkBox.tooltip = "Allow vehicles on the roundabout to enter blocked junctions (TMPE policy)";
            checkBox.isChecked = SavedEnterBlockedMainRoad;
            checkBox.relativePosition = new Vector3(8, cumulativeHeight);
            checkBox.eventCheckChanged += (c, state) =>
            {
                SavedEnterBlockedMainRoad.value = state;
            };
            cumulativeHeight += checkBox.height + 8;

            checkBox = UIUtil.CreateCheckBox(this);
            checkBox.name = "RAB_enterBlockedJunctionYieldingRoad";
            checkBox.label.text = "Enter junct. yield. r.";
            checkBox.tooltip = "Allow vehicles entering the roundabout to enter blocked junction (TMPE policy)";
            checkBox.isChecked = SavedEnterBlockedYieldingRoad;
            checkBox.relativePosition = new Vector3(8, cumulativeHeight);
            checkBox.eventCheckChanged += (c, state) =>
            {
                SavedEnterBlockedYieldingRoad.value = state;
            };
            cumulativeHeight += checkBox.height + 8;

            checkBox = UIUtil.CreateCheckBox(this);
            checkBox.name = "RAB_noParking";
            checkBox.label.text = "No parking";
            checkBox.tooltip = "Restrict parking on the roundabout (TMPE policy)";
            checkBox.isChecked = SavedNoParking;
            checkBox.relativePosition = new Vector3(8, cumulativeHeight);
            checkBox.eventCheckChanged += (c, state) =>
            {
                SavedNoParking.value = state;
            };
            cumulativeHeight += checkBox.height + 8;

            checkBox = UIUtil.CreateCheckBox(this);
            checkBox.name = "RAB_prioritySigns";
            checkBox.label.text = "Priority signs";
            checkBox.tooltip = "Vehicles on the roundabout will have right-of-way";
            checkBox.isChecked = SavedPrioritySigns;
            checkBox.relativePosition = new Vector3(8, cumulativeHeight);
            checkBox.eventCheckChanged += (c, state) =>
            {
                SavedPrioritySigns.value = state;
            };
            cumulativeHeight += checkBox.height + 8;

            checkBox = UIUtil.CreateCheckBox(this);
            checkBox.name = "RAB_noCrossings";
            checkBox.label.text = "Disable crosswalks";
            checkBox.tooltip = "Disallow pedestrians to cross to inner ring of the roundabout (Does not change visual appearance)";
            checkBox.isChecked = SavedNoCrossings;
            checkBox.relativePosition = new Vector3(8, cumulativeHeight);
            checkBox.eventCheckChanged += (c, state) =>
            {
                SavedNoCrossings.value = state;
            };
            cumulativeHeight += checkBox.height + 8;

            checkBox = UIUtil.CreateCheckBox(this);
            checkBox.name = "RAB_laneChanging";
            checkBox.label.text = "Allow lane changing";
            checkBox.tooltip = "Allow vehicles to change lanes at the junction (both on the main and entering roads)";
            checkBox.isChecked = SavedAllowLaneChanging;
            checkBox.relativePosition = new Vector3(8, cumulativeHeight);
            checkBox.eventCheckChanged += (c, state) =>
            {
                SavedAllowLaneChanging.value = state;
            };
            cumulativeHeight += checkBox.height + 8;

            /*label = AddUIComponent<UILabel>();
            label.text = "Would you like to see more TMPE features (eg. automatic lane connector)? Please visit the workshop page and let me know your usual " +
                "roundabout TMPE setup. Please, be as concrete as possible. ( You can include screenshots ;) )\n- Your Strad. PS: I don't promise anything :D";
            label.wordWrap = true;
            label.textScale = 0.65f;
            label.autoSize = false;
            label.width = width - 16;
            label.relativePosition = new Vector2(8, cumulativeHeight);
            label.SendToBack();
            cumulativeHeight += label.height + 8;*/
        }
    }
}