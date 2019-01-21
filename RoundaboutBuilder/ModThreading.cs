using ICities;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.1.0 */

/* This was shamelessly stolen from bomormer's tutorial on Simtropolis. He takes the credit.
 * https://community.simtropolis.com/forums/topic/73490-modding-tutorial-3-show-limits/ */

namespace RoundaboutBuilder
{
    public class ModThreading : ThreadingExtensionBase
    {
        private bool _processed = false;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.O))
            {
                // cancel if they key input was already processed in a previous frame
                if (_processed) return;

                _processed = true;

                if (UIWindow.Instance == null) return;

                //Activating/deactivating tool & UI
                UIWindow.Instance.enabled = !UIWindow.Instance.enabled;
                //NodeSelection.instance.enabled = UIWindow.Instance.enabled;

            }
            else if (UIWindow.Instance.enabled && Input.GetKey("[+]"))
            {
                if (_processed) return;
                UIWindow.Instance.IncreaseButton();
                _processed = true;
            }
            else if (UIWindow.Instance.enabled && Input.GetKey("[-]"))
            {
                if (_processed) return;
                UIWindow.Instance.DecreaseButton();
                _processed = true;
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }
        }
    }
}
