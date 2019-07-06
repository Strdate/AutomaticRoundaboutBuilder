using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/* Version RELEASE 1.4.0+ */

/* By Strad, 2019 */

/* Credit to SamsamTS a T__S from whom I copied big chunks of code */

namespace RoundaboutBuilder.UI
{
    public class UINetInfoDropDown : UIDropDown
    {
        /*public enum PopulationMode
        {
            Standard,
            FreeTool,
            Unfiltered
        }*/

        //StringBuilder sb = new StringBuilder();
        private NetInfo[] m_netInfos;
        private SortedDictionary<StringWithLaneCount, NetInfo> m_dictionary;

        private NetInfo m_lastToolInfo;
        //private PopulationMode m_populationMode;

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

        public NetInfo Value
        {
            get
            {
                NetInfo prefab = m_netInfos[selectedIndex];
                try
                {
                    if(ModLoadingExtension.fineRoadToolDetected)
                        prefab = FineRoadToolSelection(prefab);
                }
                catch { }
                return prefab;
            }
        }

        /* We change the road mode according to Fine Road Tool */
        private static NetInfo FineRoadToolSelection(NetInfo prefab)
        {
            RoadAI roadAI = prefab.m_netAI as RoadAI;
            if(roadAI != null)
            {
                // If the user has manually selected underground/overground mode, we let it be
                if(!roadAI.IsUnderground() && !roadAI.IsOverground())
                {
                    switch(FineRoadTool.FineRoadTool.instance.mode)
                    {
                        case FineRoadTool.Mode.Ground:
                            return roadAI.m_info;
                        case FineRoadTool.Mode.Elevated:    
                        case FineRoadTool.Mode.Bridge:
                            if (roadAI.m_elevatedInfo != null)
                            {
                                return roadAI.m_elevatedInfo;
                            }
                            break;
                        case FineRoadTool.Mode.Tunnel:
                            if (roadAI.m_tunnelInfo != null)
                            {
                                return roadAI.m_tunnelInfo;
                            }
                            break;
                        case FineRoadTool.Mode.Normal:
                        case FineRoadTool.Mode.Single:
                            break;
                    }
                }
            }

            PedestrianPathAI pedestrianAI = prefab.m_netAI as PedestrianPathAI;
            if (pedestrianAI != null)
            {
                // If the user has manually selected underground/overground mode, we let it be
                if (!pedestrianAI.IsUnderground() && !pedestrianAI.IsOverground())
                {
                    switch (FineRoadTool.FineRoadTool.instance.mode)
                    {
                        case FineRoadTool.Mode.Ground:
                            return pedestrianAI.m_info;
                        case FineRoadTool.Mode.Elevated:
                        case FineRoadTool.Mode.Bridge:
                            if (pedestrianAI.m_elevatedInfo != null)
                            {
                                return pedestrianAI.m_elevatedInfo;
                            }
                            break;
                        case FineRoadTool.Mode.Tunnel:
                            if (pedestrianAI.m_tunnelInfo != null)
                            {
                                return pedestrianAI.m_tunnelInfo;
                            }
                            break;
                        case FineRoadTool.Mode.Normal:
                        case FineRoadTool.Mode.Single:
                            break;
                    }
                }
            }

            return prefab;
        }

        /* Load road netinfos */
        public void Populate(bool keepSelection = false)
        {
            NetInfo lastSelection = null;
            if(keepSelection)
            {
                try
                {
                    lastSelection = Value;
                }
                catch { keepSelection = false; }
            }

            var count = PrefabCollection<NetInfo>.PrefabCount();
            m_dictionary = new SortedDictionary<StringWithLaneCount, NetInfo>();
            bool freeCursor = UIWindow2.instance?.toolOnUI is FreeCursorTool;
            for (uint i = 0; i < count; i++)
            {
                var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                if (prefab != null)
                {
                    //Debug.Log($"Prefab {prefab.GetUncheckedLocalizedTitle()}, fl {prefab.m_hasBackwardVehicleLanes}, bl {prefab.m_hasForwardVehicleLanes}, car flag {(prefab.m_vehicleTypes & VehicleInfo.VehicleType.Car) != 0}");
                    if( IsEligible(prefab, freeCursor) )
                    {
                        StringWithLaneCount slc = new StringWithLaneCount(prefab);
                        //beautified = (prefab.m_forwardVehicleLaneCount + prefab.m_backwardVehicleLaneCount) + "_" + beautified;
                        if (m_dictionary.ContainsKey(slc))
                            slc.Name += " [" + i + "]";
                        m_dictionary[slc] = prefab;
                        //Debug.Log("Loaded prefab: " + slc.Name + " ,underground: " + prefab.m_netAI.IsUnderground() + " ,overground: " + prefab.m_netAI.IsOverground());
                    }
                }
            }

            UpdateListWithPrefab(null);

            /* It seems that this code is there twice. (In this method and in the UpdateListWithPrefab method.) Yes, basically it is... :( */
            if(keepSelection)
            {
                int i = Array.IndexOf(m_netInfos, lastSelection);
                if (i > -1 && i < m_netInfos.Length)
                {
                    selectedIndex = i;
                }
            }
        }

        /* Is prefab eligible to be in the list? (Must be oneway etc.) */
        private bool IsEligible(NetInfo prefab, bool freeCursor)
        {
            if (RoundAboutBuilder.DoNotFilterPrefabs.value)
                return true;

            if (prefab.m_netAI.IsUnderground() || prefab.m_netAI.IsOverground())
                return false;

            bool hasRoadLanes = prefab.m_hasBackwardVehicleLanes || prefab.m_hasForwardVehicleLanes;
            bool isOneWay = prefab.m_hasBackwardVehicleLanes ^ prefab.m_hasForwardVehicleLanes;

            if(freeCursor)
            {
                return (isOneWay && (prefab.m_vehicleTypes & VehicleInfo.VehicleType.Car) != 0
                        && (prefab.m_laneTypes & NetInfo.LaneType.Vehicle) != 0 )
                        || prefab.m_class.name.IndexOf("pedestrian", StringComparison.OrdinalIgnoreCase) >= 0
                        || prefab.m_class.name.IndexOf("train track", StringComparison.OrdinalIgnoreCase) >= 0
                        || prefab.m_class.name.IndexOf("landscaping", StringComparison.OrdinalIgnoreCase) >= 0
                        || prefab.m_class.name.IndexOf("beautification", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else
            {
                return (isOneWay && (prefab.m_vehicleTypes & VehicleInfo.VehicleType.Car) != 0
                        && (prefab.m_laneTypes & NetInfo.LaneType.Vehicle) != 0 /*&& (!prefab.m_netAI.IsUnderground() && !prefab.m_netAI.IsOverground())*/);
            }
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
            itemName = Regex.Replace(itemName, "one-way", (RoundAboutBuilder.DoNotFilterPrefabs.value ? "ow" : ""), RegexOptions.IgnoreCase);
            itemName = Regex.Replace(itemName, "with", "", RegexOptions.IgnoreCase);
            itemName = Regex.Replace(itemName, "road", "Rd", RegexOptions.IgnoreCase);
            itemName = Regex.Replace(itemName, "highway", "Hway", RegexOptions.IgnoreCase);
            itemName = Regex.Replace(itemName, "decorative", "Decor", RegexOptions.IgnoreCase);

            itemName = Regex.Replace(itemName, "Einbahnstraße", (RoundAboutBuilder.DoNotFilterPrefabs.value ? "ow" : ""), RegexOptions.IgnoreCase);

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
            bool freeCursor = UIWindow2.instance?.toolOnUI is FreeCursorTool;
            if (m_lastToolInfo != null && !IsEligible(m_lastToolInfo, freeCursor))
            {
                m_dictionary.Remove(new StringWithLaneCount(m_lastToolInfo," [S]"));
            }

            if(extraInfo != null && !IsEligible(extraInfo, freeCursor))
            {
                m_dictionary.Add(new StringWithLaneCount(extraInfo, " [S]"), extraInfo);
            }

            // This should never happen, but I will leave there. Some NetInfos could have been missing from the list due to name duplicity, but that was
            // solved in the Populate() method
            if(extraInfo != null && IsEligible(extraInfo, freeCursor) && !m_netInfos.Contains(extraInfo))
            {
                m_dictionary.Add(new StringWithLaneCount(extraInfo, " [E]"), extraInfo);
            }

            items = m_dictionary.Keys.Select(x => x.Name).ToArray();
            m_netInfos = m_dictionary.Values.ToArray();

            if(extraInfo != null)
            {
                int i = Array.IndexOf(m_netInfos, extraInfo);
                selectedIndex = i >= -1 ? i : selectedIndex;
            }
            else
            {
                /* Set strandard oneway as default */
                int i = Array.IndexOf(m_netInfos, PrefabCollection<NetInfo>.FindLoaded("Oneway Road"));
                if (i > -1 && i < m_netInfos.Length)
                {
                    selectedIndex = i;
                }
            }

            //Debug.Log($"Loaded {items.Length} netinfos.");
        }

        /* Checks if user selected new road type, then changes the road in the dropdown 
         * Code inspired by boformer's Network Skins. Thanks! */
        public override void Update()
        {
            base.Update();
            bool freeCursor = UIWindow2.instance?.toolOnUI is FreeCursorTool;
            if (RoundAboutBuilder.FollowRoadToolSelection.value && UIWindow2.instance.enabled)
            {
                ToolBase currentTool = ToolsModifierControl.toolController.CurrentTool;
                NetTool netTool = currentTool as NetTool;
                if(netTool?.Prefab != null && (IsEligible(netTool.Prefab, freeCursor) || ( IsRoad(netTool.Prefab) && RoundAboutBuilder.SelectTwoWayRoads.value) ))
                {
                    if(m_lastToolInfo == null || m_lastToolInfo != netTool.Prefab)
                    {
                        UpdateListWithPrefab(netTool.Prefab);
                        m_lastToolInfo = netTool.Prefab;
                    }
                }
            }
        }

        private struct StringWithLaneCount : IComparable<StringWithLaneCount>
        {
            public string Name { get; set; }
            public int LaneCount { get; set; }

            public StringWithLaneCount(NetInfo prefab, string postfix = "")
            {
                Name = GenerateBeautifiedNetName(prefab) + postfix;
                LaneCount = prefab.m_backwardVehicleLaneCount + prefab.m_forwardVehicleLaneCount;
            }

            public int CompareTo(StringWithLaneCount other)
            {
                int result = other.LaneCount.CompareTo(LaneCount);
                if (result == 0)
                    result = other.Name.CompareTo(Name);
                return -result;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is StringWithLaneCount))
                {
                    return false;
                }

                var count = (StringWithLaneCount)obj;
                return Name == count.Name &&
                       LaneCount == count.LaneCount;
            }

            public override int GetHashCode()
            {
                var hashCode = -352197940;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + LaneCount.GetHashCode();
                return hashCode;
            }
        }
    }
}
