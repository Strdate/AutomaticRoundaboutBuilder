using ColossalFramework.UI;
using ICities;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.1.0 */

/* Most of this is copied from Elektrix's Segment Slope Smoother.
 * The oter part is copied from somewhere as well, but unfortunately I don't remeber from where. */

namespace RoundaboutBuilder
{
    public class ModLoadingExtension : ILoadingExtension
    {

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
            if (UIWindow.Instance == null)
            {
                UIView.GetAView().AddUIComponent(typeof(UIWindow));
            } 
        }

        // called when unloading begins
        public void OnLevelUnloading()
        {
            UIWindow.Instance.enabled = false;
        }

        // called when unloading finished
        public void OnReleased()
        {
        }
    }
}
