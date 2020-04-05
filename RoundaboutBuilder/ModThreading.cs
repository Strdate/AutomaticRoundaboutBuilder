using ColossalFramework;
using ICities;
using RoundaboutBuilder.Tools;
using RoundaboutBuilder.UI;
using SharedEnvironment;
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
            if (RoundAboutBuilder.ModShortcut.IsPressed())
            {
                // cancel if they key input was already processed in a previous frame

                if (_processed)
                    return;

                _processed = true;

                if (UIWindow.instance == null) return;

                //Activating/deactivating tool & UI
                //UIWindow.Instance.enabled = !UIWindow.Instance.enabled;
                UIWindow.instance.enabled = !UIWindow.instance.enabled;
                //NodeSelection.instance.enabled = UIWindow.Instance.enabled;

            }
            else if(UIWindow.instance.enabled)
            {
                _processed = KeysPressed();
            } else
            {
                _processed = false;
            }

            // Mouse wheel

            /*if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
            {
                UIWindow2.instance.toolOnUI?.IncreaseButton();
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
            {
                UIWindow2.instance.toolOnUI?.DecreaseButton();
            }*/

            /* Check for UI panel button */
            if (UIWindow.instance && !UIPanelButton.Instance && RoundAboutBuilder.ShowUIButton.value) // If UIPanel has been already initialized && button missing
            {
                UIPanelButton.CreateButton();
            }

            /* Delayed setup */
            if(timeUp)
            {
                timeUp = false;
                
                try { m_TMPEaction.Do(); }
                catch(Exception e)
                {
                    Debug.LogWarning(e);
                }
                
            }            
        }

        private bool KeysPressed()
        {
            if (Input.GetKey("[+]") || (RoundAboutBuilder.UseExtraKeys.value && RoundAboutBuilder.IncreaseShortcut.IsPressed()))
            {
                if (_processed)
                    return true;
                UIWindow.instance.toolOnUI?.IncreaseButton();
                return true;
            }

            if (Input.GetKey("[-]") || (RoundAboutBuilder.UseExtraKeys.value && RoundAboutBuilder.DecreaseShortcut.IsPressed()))
            {
                if (_processed)
                    return true;
                UIWindow.instance.toolOnUI?.DecreaseButton();
                return true;
            }

            // Undo last action
            if (UIWindow.instance.toolOnUI != null && UIWindow.instance.toolOnUI.enabled && 
                (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Z))
            {
                if (_processed)
                    return true;
                UndoAction();
                return true;
            }

            if (Input.GetKey(KeyCode.PageUp))
            {
                if (_processed)
                    return true;
                UIWindow.instance.toolOnUI?.PgUpButton();
                return true;
            }
            if (Input.GetKey(KeyCode.PageDown))
            {
                if (_processed)
                    return true;
                UIWindow.instance.toolOnUI?.PgDnButton();
                return true;
            }
            if (Input.GetKey(KeyCode.Home))
            {
                if (_processed)
                    return true;
                UIWindow2.instance.toolOnUI?.HomeButton();
                return true;
            }
            return false;
        }

        public static void PushAction(GameAction actionRoads, GameAction actionTMPE)
        {
            m_lastAction = actionRoads;            
            UIWindow.instance.undoButton.isEnabled = true;
            Singleton<SimulationManager>.instance.AddAction(() => {
                actionRoads.Do();
            });
            TimerTMPE(actionTMPE);
        }

        public static void UndoAction()
        {            
            if(m_lastAction != null)
            {
                UIWindow.instance.undoButton.isEnabled = false;
                Singleton<SimulationManager>.instance.AddAction(() => {
                    m_lastAction.Undo();
                    m_lastAction = null;
                });
            }
        }

        private static GameAction m_TMPEaction;
        private static GameAction m_lastAction;

        private static Timer aTimer;
        private static bool timeUp = false;
        private static void TimerTMPE(GameAction action)
        {
            m_TMPEaction = action;
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
