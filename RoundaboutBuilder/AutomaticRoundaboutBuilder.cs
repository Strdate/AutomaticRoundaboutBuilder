using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using ICities;
using RoundaboutBuilder.Tools;
using RoundaboutBuilder.UI;
using System;
using UnityEngine;

/* By Strad, 02/2019 - 04/2020 */

/* Version RELEASE 1.9.0 */

/* Warning: I am lazy thus the version labels across the files may not be updated */

/* There was time when I kept the code more tidy than now. If you are learing how to mod, I suggest you to look at older versions of this mod. (Mayebe even the first one.)
 * Everything is available on Github. */

namespace RoundaboutBuilder
{
    public class RoundAboutBuilder : IUserMod
    {
        public static readonly string VERSION = "RELEASE 1.9.6";
        public static PublishedFileId WORKSHOP_FILE_ID;

        public const string settingsFileName = "RoundaboutBuilder";

        public static readonly SavedInputKey ModShortcut = new SavedInputKey("modShortcut", settingsFileName, SavedInputKey.Encode(KeyCode.O, true, false, false), true);
        public static readonly SavedInputKey IncreaseShortcut = new SavedInputKey("increaseShortcut", settingsFileName, SavedInputKey.Encode(KeyCode.Equals, false, false, false), true);
        public static readonly SavedInputKey DecreaseShortcut = new SavedInputKey("decreaseShortcut", settingsFileName, SavedInputKey.Encode(KeyCode.Minus, false, false, false), true);

        public static readonly Vector2 defWindowPosition = new Vector2(85, 10);

        public static readonly SettingsBool SelectTwoWayRoads = new SettingsBool("Allow selection of two-way roads", "You can select two-way roads for your roundabouts through the roads menu (if that option is enabled)", "selectTwoWayRoads", false);
        public static readonly SettingsBool DoNotRemoveAnyRoads = new SettingsBool("Do not remove or connect any roads (experimental)", "No roads will be removed or connected when the roundabout is built", "dontRemoveRoads", false);
        public static readonly SettingsBool FollowRoadToolSelection = new SettingsBool("Use the selected road in roads menu as the roundabout road", "Your selected road for the roundabout will change as you browse through the roads menu", "followRoadToolSelection",true);
        public static readonly SettingsBool ShowUIButton = new SettingsBool("Show mod icon in toolbar (needs reload)", "Show the Roundabout Builder icon in road tools panel (You can always use CTRL+O to open the mod menu)","showUIButton",true);
        public static readonly SettingsBool UseExtraKeys = new SettingsBool("Use secondary increase / decrease radius keys", "If checked, you can use bound keys from the list below to increase / decrease radius (besides the ones on numpad)", "useExtraPlusMinusKeys", true);
        public static readonly SettingsBool UseOldSnappingAlgorithm = new SettingsBool("Legacy: Use old snapping algorithm", "Old snapping algorithm connects roads at 90° angle, but distorts their geometry","useOldSnappingAlgorithm", false);
        public static readonly SettingsBool DoNotFilterPrefabs = new SettingsBool("Do not filter prefabs (include all networks in the menu)", "The dropdown menu will include all prefabs available, not only one-way roads","doNotFilterPrefabs", false);
        public static readonly SettingsBool NeedMoney = new SettingsBool("Require money", "Building a roundabout will cost you money", "needMoney",true);
        public static readonly SettingsBool CtrlToReverseDirection = new SettingsBool("Hold CTRL to build in opposite direction", "The roundabout will be built in opposite direction if you hold CTRL", "ctrlToOppositeDir", false);
        public static readonly SettingsBool UnlimitedRadius = new SettingsBool("Unlimited radius", "(Almost) Unlimited roundabout radius - up to 20 000 units", "unlimitedRadius", false);
        public static readonly SettingsBool LegacyEllipticRoundabouts = new SettingsBool("Legacy: Elliptic roundabouts", "Enable elliptic roundabouts. This feature is no longer maintained", "legacyEllipticRoundabouts", false);

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

                FollowRoadToolSelection.Draw(group);
                NeedMoney.Draw(group);
                CtrlToReverseDirection.Draw(group);
                UnlimitedRadius.Draw(group, (b) =>
                {
                    if (UIWindow.instance != null) UIWindow.instance.InitPanels();
                });
                SelectTwoWayRoads.Draw(group, (b) =>
                {
                    if (UIWindow.instance != null) UIWindow.instance.dropDown.Populate(); // Reload dropdown menu
                });
                DoNotFilterPrefabs.Draw(group, (b) =>
                {
                    if (UIWindow.instance != null) UIWindow.instance.dropDown.Populate(); // Reload dropdown menu
                });
                DoNotRemoveAnyRoads.Draw(group);

                group.AddSpace(10);

                ShowUIButton.Draw(group);
                UseExtraKeys.Draw(group);
                UseOldSnappingAlgorithm.Draw(group);
                LegacyEllipticRoundabouts.Draw(group, (b) =>
                {
                    if (UIWindow.instance != null) UIWindow.instance.InitPanels();
                });

                group.AddSpace(10);

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);

                group.AddButton("Reset tool window position", () =>
                {
                    savedWindowX.Delete();
                    savedWindowY.Delete();

                    if (UIWindow.instance)
                        UIWindow.instance.absolutePosition = defWindowPosition;
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
                    UIWindow.instance.ThrowErrorMsg("Congratulations! You've built " + requiredCount + " roundabouts in total! Do you live in France?\n" +
                        "Fun fact: There is more than 32000 roundabouts in France, which is about one roundabout every 30 km. But the country with the highest number of roundabouts" +
                        " per km is the United Kingdom (25000 in total/one every 15 km). Thanks to Scott Batson from Quora for this summary (2015)");
                }
            }
            catch { }
        }
    }
}
