using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

/* Version RELEASE 1.0.2+ */

namespace RoundaboutBuilder.UI
{
    public abstract class AbstractPanel : UIPanel
    {
        public abstract bool ShowDropDown { get; }
        public abstract bool ShowTmpeSetup { get; }
        public abstract bool ShowBackButton { get; }
        public abstract bool IsSpecialWindow { get; }

        public AbstractPanel()
        {
            isVisible = false;
            width = 204;
            height = 188;
            clipChildren = true;
        }
    }

    public class RoundAboutPanel : AbstractPanel
    {
        public override bool ShowDropDown => true;
        public override bool ShowTmpeSetup => true;
        public override bool ShowBackButton => false;
        public override bool IsSpecialWindow => false;

        public NumericTextField RadiusField { get; }

        public RoundAboutPanel()
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
            cumulativeHeight += RadiusField.height + 8;

            UIButton button = UIWindow2.CreateButton(this);
            button.text = "Elliptic Roundabout...";
            button.relativePosition = new Vector2(8, cumulativeHeight);
            button.width = width - 16;
            button.eventClick += (c, p) =>
            {
                UIWindow2.instance.SwitchTool(EllipseTool.Instance);
            };
            cumulativeHeight += button.height + 8;

            height = cumulativeHeight;
        }
    }

    public class EllipsePanel_1 : AbstractPanel
    {
        public override bool ShowDropDown => true;
        public override bool ShowTmpeSetup => false;
        public override bool ShowBackButton => true;
        public override bool IsSpecialWindow => false;

        public EllipsePanel_1()
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
        public override bool ShowTmpeSetup => false;
        public override bool ShowBackButton => true;
        public override bool IsSpecialWindow => false;

        public EllipsePanel_2()
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
        public override bool ShowTmpeSetup => true;
        public override bool ShowBackButton => true;
        public override bool IsSpecialWindow => false;

        public static readonly int RADIUS1_DEF = 60;
        public static readonly int RADIUS2_DEF = 40;

        public NumericTextField Radius1tf { get;  }
        public NumericTextField Radius2tf { get; }

        public EllipsePanel_3()
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
            Radius1tf.DefaultRadius = RADIUS1_DEF;
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
            Radius2tf.DefaultRadius = RADIUS2_DEF;
            Radius2tf.text = RADIUS2_DEF.ToString();
            cumulativeHeight += Radius2tf.height + 8;

            var buildButton = UIWindow2.CreateButton(this);
            buildButton.text = "Build";
            buildButton.relativePosition = new Vector2(8, cumulativeHeight);
            buildButton.playAudioEvents = false;
            buildButton.eventClick += (c, p) =>
            {
                EllipseTool.Instance.BuildEllipse();
            };
            cumulativeHeight += buildButton.height + 8;

            var controlVertices = UIWindow2.CreateCheckBox(this);
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
        public override bool ShowTmpeSetup => false;
        public override bool ShowBackButton => true;
        public override bool IsSpecialWindow => true;

        public static readonly SavedBool SavedEnterBlockedJunction = new SavedBool("tmpeEnterBlockedJunction", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool SavedNoParking = new SavedBool("tmpeNoParking", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool SavedPrioritySigns = new SavedBool("tmpePrioritySigns", RoundAboutBuilder.settingsFileName, false, true);
        public static readonly SavedBool SavedNoCrossings = new SavedBool("tmpeNoCrossings", RoundAboutBuilder.settingsFileName, false, true);

        public TmpeSetupPanel()
        {
            float cumulativeHeight = 8;

            UILabel label = AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.width = width;
            label.text = "TMPE settings";
            label.relativePosition = new Vector2(8, cumulativeHeight);
            label.SendToBack();
            cumulativeHeight += label.height + 8;

            var checkBox = UIWindow2.CreateCheckBox(this);
            checkBox.name = "RAB_enterBlockedJunction";
            checkBox.label.text = "Enter blocked junct.";
            checkBox.tooltip = "Allow vehicles to enter blocked junctions (TMPE policy)";
            checkBox.isChecked = SavedEnterBlockedJunction;
            checkBox.relativePosition = new Vector3(8, cumulativeHeight);
            checkBox.eventCheckChanged += (c, state) =>
            {
                SavedEnterBlockedJunction.value = state;
            };
            cumulativeHeight += checkBox.height + 8;

            checkBox = UIWindow2.CreateCheckBox(this);
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

            checkBox = UIWindow2.CreateCheckBox(this);
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

            checkBox = UIWindow2.CreateCheckBox(this);
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