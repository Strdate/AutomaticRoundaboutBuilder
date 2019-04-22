using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/* Version RELEASE 1.0.1+ */

/* By Strad, 2019 */

/* Credit to SamsamTS a T__S from whom I copied big chunks of code */

namespace RoundaboutBuilder.UI
{
    public class UINetInfoDropDown : UIDropDown
    {
        //StringBuilder sb = new StringBuilder();
        private NetInfo[] m_netInfos;
        private SortedDictionary<string, NetInfo> m_dictionary;

        private NetInfo m_lastToolInfo;

        public UINetInfoDropDown()
        {
            atlas = ResourceLoader.GetAtlas("Ingame");
            size = new Vector2(90f, 30f);
            listBackground = "GenericPanelLight";
            itemHeight = 25;
            itemHover = "ListItemHover";
            itemHighlight = "ListItemHighlight";
            normalBgSprite = "ButtonMenu";
            disabledBgSprite = "ButtonMenuDisabled";
            hoveredBgSprite = "ButtonMenuHovered";
            focusedBgSprite = "ButtonMenu";
            listWidth = 90;
            listHeight = 1000;
            listPosition = UIDropDown.PopupListPosition.Below;
            clampListToScreen = true;
            builtinKeyNavigation = true;
            foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            popupColor = new Color32(45, 52, 61, 255);
            popupTextColor = new Color32(170, 170, 170, 255);
            zOrder = 1;
            textScale = 0.8f;
            verticalAlignment = UIVerticalAlignment.Middle;
            horizontalAlignment = UIHorizontalAlignment.Left;
            selectedIndex = 0;
            textFieldPadding = new RectOffset(8, 0, 8, 0);
            itemPadding = new RectOffset(14, 0, 8, 0);
            
            UIButton button = AddUIComponent<UIButton>();
            triggerButton = button;
            button.atlas = ResourceLoader.GetAtlas("Ingame");
            button.text = "";
            button.size = size;
            button.relativePosition = new Vector3(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.focusedFgSprite = "IconDownArrowFocused";
            button.disabledFgSprite = "IconDownArrowDisabled";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.zOrder = 0;
            button.textScale = 0.8f;

            eventSizeChanged += new PropertyChangedEventHandler<Vector2>((c, t) =>
            {
                button.size = t; listWidth = (int)t.x;
            });
            
            Populate();
        }

        protected override void OnSelectedIndexChanged()
        {
            UIWindow2.instance.MouseInWindowCheat = true;
            //UIWindow2.instance.ChangeSizeDebug();
            //ModThreading.ReenableToolsTimer();
        }

        public NetInfo Value
        {
            get
            {
                return m_netInfos[selectedIndex];
            }
        }

        /* Load road netinfos */
        private void Populate()
        {
            var count = PrefabCollection<NetInfo>.PrefabCount();
            m_dictionary = new SortedDictionary<string, NetInfo>();
            for (uint i = 0; i < count; i++)
            {
                var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                if (prefab != null)
                {
                    //Debug.Log($"Prefab {prefab.GetUncheckedLocalizedTitle()}, fl {prefab.m_hasBackwardVehicleLanes}, bl {prefab.m_hasForwardVehicleLanes}, car flag {(prefab.m_vehicleTypes & VehicleInfo.VehicleType.Car) != 0}");
                    if( IsOneWay(prefab) )
                    {
                        string beautified = GenerateBeautifiedNetName(prefab);
                        //sb.Append(beautified + " ");
                        m_dictionary[beautified] = prefab;
                    }
                }
            }

            UpdateListWithPrefab(null);
        }

        /* Is one-way road? */
        private static bool IsOneWay(NetInfo prefab)
        {
            return (prefab.m_hasBackwardVehicleLanes ^ prefab.m_hasForwardVehicleLanes) && (prefab.m_vehicleTypes & VehicleInfo.VehicleType.Car) != 0
                        && (prefab.m_laneTypes & NetInfo.LaneType.Vehicle) != 0;
        }

        private static bool IsRoad(NetInfo prefab)
        {
            return (prefab.m_vehicleTypes & VehicleInfo.VehicleType.Car) != 0
                        && (prefab.m_laneTypes & NetInfo.LaneType.Vehicle) != 0;
        }

        public static string GenerateBeautifiedNetName(NetInfo prefab)
        {
            string itemName;

            itemName = prefab.GetUncheckedLocalizedTitle();

            // Issue!!! This works only for English!!!
            itemName = Regex.Replace(itemName, "oneway", "", RegexOptions.IgnoreCase);
            itemName = Regex.Replace(itemName, "one-way", "", RegexOptions.IgnoreCase);
            itemName = Regex.Replace(itemName, "with", "", RegexOptions.IgnoreCase);
            itemName = Regex.Replace(itemName, "road", "Rd", RegexOptions.IgnoreCase);
            itemName = Regex.Replace(itemName, "highway", "Hway", RegexOptions.IgnoreCase);
            itemName = Regex.Replace(itemName, "decorative", "Decor", RegexOptions.IgnoreCase);

            itemName = Regex.Replace(itemName, "Einbahnstraße", "", RegexOptions.IgnoreCase);

            // replace spaces at start and end
            itemName = itemName.Trim();

            // replace multiple spaces with one
            itemName = Regex.Replace(itemName, " {2,}", " ");

            //itemName = AddSpacesToSentence(itemName);
            return itemName;
        }

        /* The extraInfo is a prefab which user has selected in the roads menu and does not have to be in the list (It might not be one-way road)
         * In that case we add it temporatily to the list */
        private void UpdateListWithPrefab(NetInfo extraInfo)
        {
            if(m_lastToolInfo != null && !IsOneWay(m_lastToolInfo))
            {
                m_dictionary.Remove("[S] " + GenerateBeautifiedNetName(m_lastToolInfo));
            }

            if(extraInfo != null && !IsOneWay(extraInfo))
            {
                m_dictionary.Add("[S] " + GenerateBeautifiedNetName(extraInfo), extraInfo);
            }

            items = m_dictionary.Keys.ToArray();
            m_netInfos = m_dictionary.Values.ToArray();

            if(extraInfo != null)
            {
                selectedIndex = Array.IndexOf(m_netInfos, extraInfo);
            }
            else
            {
                /* Set strandard oneway as default */
                selectedIndex = Array.IndexOf(m_netInfos, PrefabCollection<NetInfo>.FindLoaded("Oneway Road"));
            }

            //Debug.Log($"Loaded {items.Length} netinfos.");
        }

        /* Checks if user selected new road type, then changes the road in the dropdown 
         * Code inspired by boformer's Network Skins. Thanks! */
        public override void Update()
        {
            base.Update();

            if(RoundAboutBuilder.FollowRoadToolSelection.value && UIWindow2.instance.enabled)
            {
                ToolBase currentTool = ToolsModifierControl.toolController.CurrentTool;
                NetTool netTool = currentTool as NetTool;
                if(netTool?.Prefab != null && (IsOneWay(netTool.Prefab) || ( IsRoad(netTool.Prefab) && RoundAboutBuilder.SelectTwoWayRoads.value) ))
                {
                    if(m_lastToolInfo == null || m_lastToolInfo != netTool.Prefab)
                    {
                        UpdateListWithPrefab(netTool.Prefab);
                        m_lastToolInfo = netTool.Prefab;
                    }
                }
            }
        }
    }
}
