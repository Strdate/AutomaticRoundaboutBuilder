using ColossalFramework.UI;
using ICities;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.0.0 */

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

            //instatiate tool
            if (NodeSelection.instance == null)
            {
                ToolController toolController = GameObject.FindObjectOfType<ToolController>();

                NodeSelection.instance = toolController.gameObject.AddComponent<NodeSelection>();
                NodeSelection.instance.enabled = false;
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
