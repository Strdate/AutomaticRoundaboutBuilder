using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using ICities;
using RoundaboutBuilder.Tools;
using RoundaboutBuilder.UI;
using System;
using UnityEngine;

/* By Strad, 02/2019 */

/* Version RELEASE 1.8.0 */

/* Warning: I am lazy thus the version labels across the files may not be updated */

/* There was time when I kept the code more tidy than now. If you are learing how to mod, I suggest you to look at older versions of this mod. (Mayebe even the first one.)
 * Everything is available on Github. */

namespace RoundaboutBuilder
{
    public class RoundAboutBuilder : IUserMod
    {
        public static readonly string VERSION = "RELEASE 1.8.2";
        public static PublishedFileId WORKSHOP_FILE_ID;

        public const string settingsFileName = "RoundaboutBuilder";

        public static readonly SavedInputKey ModShortcut = new SavedInputKey("modShortcut", settingsFileName, SavedInputKey.Encode(KeyCode.O, true, false, false), true);
        public static readonly SavedInputKey IncreaseShortcut = new SavedInputKey("increaseShortcut", settingsFileName, SavedInputKey.Encode(KeyCode.Equals, false, false, false), true);
        public static readonly SavedInputKey DecreaseShortcut = new SavedInputKey("decreaseShortcut", settingsFileName, SavedInputKey.Encode(KeyCode.Minus, false, false, false), true);

        public static readonly Vector2 defWindowPosition = new Vector2(85, 10);
        public static readonly SavedBool SelectTwoWayRoads = new SavedBool("selectTwoWayRoads", RoundAboutBuilder.settingsFileName, false, true);
        public static readonly SavedBool DoNotRemoveAnyRoads = new SavedBool("dontRemoveRoads", RoundAboutBuilder.settingsFileName, false, true);
        public static readonly SavedBool FollowRoadToolSelection = new SavedBool("followRoadToolSelection", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool ShowUIButton = new SavedBool("showUIButton", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool UseExtraKeys = new SavedBool("useExtraPlusMinusKeys", RoundAboutBuilder.settingsFileName, true, true);
        public static readonly SavedBool UseOldSnappingAlgorithm = new SavedBool("useOldSnappingAlgorithm", RoundAboutBuilder.settingsFileName, false, true);
        public static readonly SavedBool DoNotFilterPrefabs = new SavedBool("doNotFilterPrefabs", RoundAboutBuilder.settingsFileName, false, true);
        public static readonly SavedBool NeedMoney = new SavedBool("needMoney", RoundAboutBuilder.settingsFileName, true, true);
        //public static readonly SavedBool SeenUpdateMsg = new SavedBool("seenUpdateMsg170", RoundAboutBuilder.settingsFileName, false, true);
        public static readonly SavedInt savedWindowX = new SavedInt("windowX", settingsFileName, (int)defWindowPosition.x, true);
        public static readonly SavedInt savedWindowY = new SavedInt("windowY", settingsFileName, (int)defWindowPosition.y, true);
        public static readonly SavedInt TotalRoundaboutsBuilt = new SavedInt("totalRoundaboutsBuilt", settingsFileName, 0, true);

        //public static readonly SavedBool ShowUndoItAd = new SavedBool("showUndoItAd", RoundAboutBuilder.settingsFileName, true, true);

        public static bool _settingsFailed = false;

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
                _settingsFailed = true;
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
            // Probably useless
            try
            {
                WORKSHOP_FILE_ID = new PublishedFileId(1625704117uL);
            } catch
            {
                Debug.Log("Error when assigning Workshop File ID");
            }
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

                checkBox = (UICheckBox)group.AddCheckbox("Use the selected road in roads menu as the roundabout road", FollowRoadToolSelection.value, (b) =>
                {
                    FollowRoadToolSelection.value = b;
                });
                checkBox.tooltip = "Your selected road for the roundabout will change as you browse through the roads menu";

                checkBox = (UICheckBox)group.AddCheckbox("Require money", NeedMoney.value, (b) =>
                {
                    NeedMoney.value = b;
                });
                checkBox.tooltip = "Building a roundabout will cost you money";

                checkBox = (UICheckBox)group.AddCheckbox("Use old snapping algorithm", UseOldSnappingAlgorithm.value, (b) =>
                {
                    UseOldSnappingAlgorithm.value = b;
                });
                checkBox.tooltip = "Old snapping algorithm connects roads at 90° angle, but distorts their geometry";

                checkBox = (UICheckBox)group.AddCheckbox("Allow selection of two-way roads", SelectTwoWayRoads.value, (b) =>
                {
                    SelectTwoWayRoads.value = b;
                    UIWindow2.instance.dropDown.Populate(); // Reload dropdown menu
                });
                checkBox.tooltip = "You can select two-way roads for your roundabouts through the roads menu (if that option is enabled)";

                checkBox = (UICheckBox)group.AddCheckbox("Do not remove or connect any roads (experimental)", DoNotRemoveAnyRoads.value, (b) =>
                {
                    DoNotRemoveAnyRoads.value = b;
                });
                checkBox.tooltip = "No roads will be removed or connected when the roundabout is built";

                checkBox = (UICheckBox)group.AddCheckbox("Do not filter prefabs (include all networks in the menu)", DoNotFilterPrefabs.value, (b) =>
                {
                    DoNotFilterPrefabs.value = b;
                    UIWindow2.instance.dropDown.Populate(); // Reload dropdown menu
                });
                checkBox.tooltip = "The dropdown menu will include all prefabs available, not only one-way roads";

                checkBox = (UICheckBox)group.AddCheckbox("Use secondary increase / decrease radius keys", UseExtraKeys.value, (b) =>
                {
                    UseExtraKeys.value = b;
                });
                checkBox.tooltip = "If checked, you can use bound keys from the list below to increase / decrease radius (besides the ones on numpad)";

                group.AddSpace(10);

                panel.gameObject.AddComponent<OptionsKeymapping>();

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

        // Easter egg
        public static void EasterEggToggle()
        {
            try
            {
                int requiredCount = 1024;
                if (TotalRoundaboutsBuilt.value < int.MaxValue)
                    TotalRoundaboutsBuilt.value++;
                if (TotalRoundaboutsBuilt.value == requiredCount)
                {
                    UIWindow2.instance.ThrowErrorMsg("Congratulations! You've built " + requiredCount + " roundabouts in total! Do you live in France?\n" +
                        "Fun fact: There is more than 32000 roundabouts in France, which is about one roundabout every 30 km. But the country with the highest number of roundabouts" +
                        " per km is the United Kingdom (25000 in total/one every 15 km). Thanks to Scott Batson from Quora for this summary (2015)");
                }
            }
            catch { }
        }
    }
}
