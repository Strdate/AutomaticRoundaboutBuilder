using ColossalFramework;
using ICities;
using RoundaboutBuilder.UI;
using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version RELEASE 1.4.0+ */

/* This was shamelessly stolen from bomormer's tutorial on Simtropolis. He takes the credit.
 * https://community.simtropolis.com/forums/topic/73490-modding-tutorial-3-show-limits/ */

namespace RoundaboutBuilder
{
    public class ModThreading : ThreadingExtensionBase
    {
        private bool _processed = false;

        public static ModThreading instance;

        public ModThreading()
        {
            instance = this;
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.O) )
            {
                // cancel if they key input was already processed in a previous frame

                if (_processed)
                    return;

                _processed = true;

                if (UIWindow2.instance == null) return;

                //Activating/deactivating tool & UI
                //UIWindow.Instance.enabled = !UIWindow.Instance.enabled;
                UIWindow2.instance.enabled = !UIWindow2.instance.enabled;
                //NodeSelection.instance.enabled = UIWindow.Instance.enabled;

            }
            else if(UIWindow2.instance.enabled)
            {
                _processed = KeysPressed();
            } else
            {
                _processed = false;
            }

            /* Check for UI panel button */
            if(UIWindow2.instance && !UIPanelButton.Instance && RoundAboutBuilder.ShowUIButton.value) // If UIPanel has been already initialized && button missing
            {
                UIPanelButton.CreateButton();
            }

            /* Delayed setup */
            if(timeUp)
            {
                timeUp = false;
                
                try { actionStatic.Do(); }
                catch(Exception e)
                {
                    Debug.LogWarning(e);
                }
                
            }            
        }

        private bool KeysPressed()
        {
            if (Input.GetKey("[+]") || (RoundAboutBuilder.UseExtraKeys.value && Input.GetKey(ExtraKeyIncrease.value)))
            {
                if (_processed)
                    return true;
                UIWindow2.instance.toolOnUI?.IncreaseButton();
                return true;
            }

            if (Input.GetKey("[-]") || (RoundAboutBuilder.UseExtraKeys.value && Input.GetKey(ExtraKeyDecrease.value)))
            {
                if (_processed)
                    return true;
                UIWindow2.instance.toolOnUI?.DecreaseButton();
                return true;
            }
            if(Input.GetKey(KeyCode.PageUp))
            {
                if (_processed)
                    return true;
                UIWindow2.instance.toolOnUI?.PgUpButton();
                return true;
            }
            if (Input.GetKey(KeyCode.PageDown))
            {
                if (_processed)
                    return true;
                UIWindow2.instance.toolOnUI?.PgDnButton();
                return true;
            }
            return false;
        }

        public static Provisional.Actions.Action actionStatic;
        private static Timer aTimer;
        private static bool timeUp = false;
        public static void Timer(Provisional.Actions.Action action)
        {
            actionStatic = action;
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 500;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += (a,b) =>
            {
                timeUp = true;
                aTimer.Stop();
                aTimer.Dispose();
            };

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = false;
            
            // Start the timer
            aTimer.Enabled = true;
        }

    }
}
