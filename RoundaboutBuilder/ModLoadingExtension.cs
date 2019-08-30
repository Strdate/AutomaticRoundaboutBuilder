using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using RoundaboutBuilder.UI;
using System;
using System.Linq;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version RELEASE 1.4.0+ */

/* Most of this is copied from Elektrix's Segment Slope Smoother.
 * The oter part is copied from somewhere as well, but unfortunately I don't remeber from where. */

namespace RoundaboutBuilder
{
    public class ModLoadingExtension : ILoadingExtension
    {
        public static bool LevelLoaded = false;
        public static bool tmpeDetected = false;
        public static bool fineRoadToolDetected = false;

        public static readonly UInt64[] TMPE_IDs = { 583429740, 1637663252, 1806963141 };
        public static readonly UInt64[] FINE_ROAD_ANARCHY_IDs = { 651322972, 1844442251 };

        // called when level loading begins
        public void OnCreated(ILoading loading)
        {
            tmpeDetected = false;
            fineRoadToolDetected = false;
            foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo())
            {
                //_string += current.name + " ";
                if (!tmpeDetected && current.isEnabled && (current.name.Contains("TrafficManager") || TMPE_IDs.Contains( current.publishedFileID.AsUInt64 )))
                {
                    tmpeDetected = true;
                    //_string += "[TMPE Detected!]";
                } else if (!fineRoadToolDetected && current.isEnabled &&(current.name.Contains("FineRoadTool") || FINE_ROAD_ANARCHY_IDs.Contains( current.publishedFileID.AsUInt64 )))
                {
                    fineRoadToolDetected = true;
                }
            }
            //Debug.Log(_string);
        }

        // called when level is loaded
        public void OnLevelLoaded(LoadMode mode)
        {

            //instatiate tools
            if (RoundaboutTool.Instance == null || EllipseTool.Instance == null)
            {
                ToolController toolController = GameObject.FindObjectOfType<ToolController>();

                RoundaboutTool.Instance = toolController.gameObject.AddComponent<RoundaboutTool>();
                RoundaboutTool.Instance.enabled = false;
                EllipseTool.Instance = toolController.gameObject.AddComponent<EllipseTool>();
                EllipseTool.Instance.enabled = false;
                FreeCursorTool.Instance = toolController.gameObject.AddComponent<FreeCursorTool>();
                FreeCursorTool.Instance.enabled = false;
            }

            //instatiate UI
            if (UIWindow2.instance == null) // !!
            {
                UIView.GetAView().AddUIComponent(typeof(UIWindow2));
            }

            /*if (NetInfoController.instance == null)
            {
                GameObject gameObject = new GameObject("RoundaboutBuilderNetinfoManager");
                NetInfoController.instance = gameObject.AddComponent<NetInfoController>();
            }*/

            //Debug.Log(_string);
            LevelLoaded = true;
            //debug();
        }

        /*private void debug()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            //Make an array for the list of assemblies.
            Assembly[] assems = currentDomain.GetAssemblies();

            //List the assemblies in the current application domain.
            Console.WriteLine("List of assemblies loaded in current appdomain:");
            foreach (Assembly assem in assems)
                Debug.Log(assem.ToString());
        }*/

        // called when unloading begins
        public void OnLevelUnloading()
        {
            if(UIWindow2.instance != null)
            {
                UIWindow2.instance.enabled = false;
            }
            LevelLoaded = false;
        }

        // called when unloading finished
        public void OnReleased()
        {
        }
    }
}
