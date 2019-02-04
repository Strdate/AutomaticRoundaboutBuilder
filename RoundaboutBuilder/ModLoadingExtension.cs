using ColossalFramework.UI;
using ICities;
using RoundaboutBuilder.UI;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.2.0 */

/* Most of this is copied from Elektrix's Segment Slope Smoother.
 * The oter part is copied from somewhere as well, but unfortunately I don't remeber from where. */

namespace RoundaboutBuilder
{
    public class ModLoadingExtension : ILoadingExtension
    {
        public static bool LevelLoaded = false;

        // called when level loading begins
        public void OnCreated(ILoading loading)
        {
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
            }

            //instatiate UI
            if (UIWindow2.instance == null) // !!
            {
                UIView.GetAView().AddUIComponent(typeof(UIWindow2));
            }

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
            UIWindow2.instance.enabled = false;
            LevelLoaded = false;
        }

        // called when unloading finished
        public void OnReleased()
        {
        }
    }
}
