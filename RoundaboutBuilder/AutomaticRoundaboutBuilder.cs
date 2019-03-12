using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using RoundaboutBuilder.UI;
using System;
using UnityEngine;

/* By Strad, 02/2019 */

/* Version RELEASE 1.2.0 */

/* Warning: I am lazy thus the version labels across the files may not be updated */

namespace RoundaboutBuilder
{
    public class RoundAboutBuilder : IUserMod
    {
        public static readonly string VERSION = "RELEASE 1.2.0";
        public bool OldSnappingAlgorithm { get; private set; } = false;

        public const string settingsFileName = "RoundaboutBuilder";

        public static readonly Vector2 defWindowPosition = new Vector2(85, 10);
        public static readonly SavedBool ShowUIButton = new SavedBool("showUIButton", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool UseExtraKeys = new SavedBool("useExtraPlusMinusKeys", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool UseOldSnappingAlgorithm = new SavedBool("useOldSnappingAlgorithm", RoundAboutBuilder.settingsFileName, false, true);
        public static readonly SavedInt savedWindowX = new SavedInt("windowX", settingsFileName, (int)defWindowPosition.x, true);
        public static readonly SavedInt savedWindowY = new SavedInt("windowY", settingsFileName, (int)defWindowPosition.y, true); 

        public RoundAboutBuilder()
        {
            try
            {
                // Creating setting file - from SamsamTS
                if (GameSettings.FindSettingsFileByName(settingsFileName) == null)
                {
                    GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = settingsFileName } });
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load/create the setting file.");
                Debug.LogException(e);
            }
        }

        public string Name
        {
            get { return "Roundabout Builder"; }
        }

        public string Description
        {
            get { return "Press CTRL+O to open mod menu [" + VERSION + "]"; }
            //get { return "Automatically builds roundabouts. [" + VERSION + "]"; }

        }

        public void OnEnabled()
        {
            //Debug.Log("Mod onenabled");
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                UIHelper group = helper.AddGroup(Name) as UIHelper;
                UIPanel panel = group.self as UIPanel;

                UICheckBox checkBox = (UICheckBox)group.AddCheckbox("Show mod icon on toolbar (needs reload)", ShowUIButton.value, (b) =>
                {
                    ShowUIButton.value = b;
                });
                checkBox.tooltip = "Show the Roundabout Builder icon in road tools panel (You can always use CTRL+O to open the mod menu)";

                group.AddSpace(10);

                checkBox = (UICheckBox)group.AddCheckbox("Use +/- keys on main keyboard", UseExtraKeys.value, (b) =>
                {
                    UseExtraKeys.value = b;
                });
                checkBox.tooltip = "If checked, you can use +/- keys on the main keyboard besides the ones on the numpad";

                group.AddSpace(10);

                checkBox = (UICheckBox)group.AddCheckbox("Use old snapping algorithm", UseOldSnappingAlgorithm.value, (b) =>
                {
                    UseOldSnappingAlgorithm.value = b;
                });
                checkBox.tooltip = "Old snapping algorithm connects roads at 90° angle, but distorts their geometry";

                group.AddSpace(10);

                group.AddButton("Reset tool window position", () =>
                {
                    savedWindowX.Delete();
                    savedWindowY.Delete();

                    if (UIWindow2.instance)
                        UIWindow2.instance.absolutePosition = defWindowPosition;
                });

                group.AddSpace(10);

                group.AddButton("Remove glitched roads (Save game inbefore)", () =>
                {
                    Tools.GlitchedRoadsCheck.RemoveGlitchedRoads();
                });

            }
            catch (Exception e)
            {
                Debug.Log("OnSettingsUI failed");
                Debug.LogException(e);
            }
        }
    }
}
