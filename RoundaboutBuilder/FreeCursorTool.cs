using ColossalFramework;
using ColossalFramework.UI;
using RoundaboutBuilder.Tools;
using RoundaboutBuilder.UI;
using SharedEnvironment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

/* By Strad, 06/19 */

namespace RoundaboutBuilder
{
    public class FreeCursorTool : ToolBaseExtended
    {
        public static FreeCursorTool Instance;

        public bool AbsoluteElevation { get; set; }

        public void CreateRoundabout()
        {
            float? radiusQ = UIWindow.instance.P_FreeToolPanel.RadiusField.Value;
            if (radiusQ == null)
            {
                UIWindow.instance.ThrowErrorMsg("Radius out of bounds!");
                return;
            }
            float? elevationFieldQ = UIWindow.instance.P_FreeToolPanel.ElevationField.Value;
            if (elevationFieldQ == null)
            {
                UIWindow.instance.ThrowErrorMsg("Elevation out of bounds!");
                return;
            }

            float radius = (float)radiusQ;
            float elevation = (float)elevationFieldQ;

            if (!UIWindow.instance.keepOpen)
                UIWindow.instance.LostFocus();

            Vector3 vector = m_hoverPos;
            if (AbsoluteElevation)
            {
                vector.y = elevation;
            }
            else
            {
                vector.y = vector.y + elevation;
            }

            if(vector.y < 0 || vector.y > 1000)
            {
                UIWindow.instance.ThrowErrorMsg("Elevation out of bounds!");
                return;
            }

            /* These lines of code do all the work. See documentation in respective classes. */
            /* When the old snapping algorithm is enabled, we create secondary (bigger) ellipse, so the newly connected roads obtained by the 
             * graph traveller are not too short. They will be at least as long as the padding. */
            Ellipse ellipse = new Ellipse(vector, new Vector3(0f, 0f, 0f), radius, radius);
            bool reverseDirection = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && RoundAboutBuilder.CtrlToReverseDirection.value;
            try
            {
                FinalConnector finalConnector = new FinalConnector(UI.UIWindow.instance.dropDown.Value, null, ellipse, false, elevation == 0 ? GetFollowTerrain() : false,reverseDirection);
                finalConnector.Build();
                // Easter egg
                RoundAboutBuilder.EasterEggToggle();
            }
            catch (ActionException e)
            {
                UIWindow.instance.ThrowErrorMsg(e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UIWindow.instance.ThrowErrorMsg(e.ToString(), true);
            }
        }

        protected override void OnClick()
        {
            base.OnClick();
            CreateRoundabout();
        }

        protected override void RenderOverlayExtended(RenderManager.CameraInfo cameraInfo)
        {
            if (UIView.IsInsideUI() || !Cursor.visible) return;

            try
            {
                float? radiusQ = UIWindow.instance.P_FreeToolPanel.RadiusField.Value;
                float? elevationFieldQ = UIWindow.instance.P_FreeToolPanel.ElevationField.Value;
                if (radiusQ == null || elevationFieldQ == null)
                {
                    return;
                }

                float radius = (float)radiusQ;
                float elevation = (float)elevationFieldQ;

                Vector3 vector2 = m_hoverPos;
                if (AbsoluteElevation)
                {
                    vector2.y = elevation;
                }
                else
                {
                    vector2.y = vector2.y + elevation;
                }

                float roadWidth = UIWindow.instance.dropDown.Value.m_halfWidth; // There is a slight chance that this will throw an exception
                float innerCirleRadius = radius - roadWidth > 0 ? 2 * ((float)radius - roadWidth) : 2 * (float)radius;

                RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, vector2, innerCirleRadius, vector2.y - 2f, vector2.y + 2f, true, true);
                RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, vector2, 2 * ((float)radius + roadWidth /*DISTANCE_PADDING - 5*/), vector2.y - 1f, vector2.y + 1f, true, true);
                RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, vector2, 15f, vector2.y - 1f, vector2.y + 1f, true, true);
                //RenderDirectionVectors(cameraInfo);

                float terrainHeight = m_hoverPos.y;

                // If the preview is not on the ground, we create a shadow
                if ((AbsoluteElevation && Mathf.Abs(terrainHeight - elevation) > 0.01f) || (!AbsoluteElevation && Mathf.Abs(elevation) > 0.01f))
                {
                    Vector3 vector = m_hoverPos;
                    vector.y = terrainHeight;
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, vector, 15f, vector.y - 2f, vector.y + 2f, true, true);

                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, vector, 2 * ((float)radius + roadWidth /*DISTANCE_PADDING - 5*/), vector.y - 1f, vector.y + 1f, true, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public override void IncreaseButton()
        {
            UIWindow.instance.P_FreeToolPanel.RadiusField.Increase();
        }

        public override void DecreaseButton()
        {
            UIWindow.instance.P_FreeToolPanel.RadiusField.Decrease();
        }

        public override void PgUpButton()
        {
            if (enabled)
            {
                UIWindow.instance.P_FreeToolPanel.ElevationField.Increment = GetElevationStep();
                UIWindow.instance.P_FreeToolPanel.ElevationField.Increase();
            }
        }

        public override void PgDnButton()
        {
            if (enabled)
            {
                UIWindow.instance.P_FreeToolPanel.ElevationField.Increment = GetElevationStep();
                UIWindow.instance.P_FreeToolPanel.ElevationField.Decrease();
            }
        }
        
        public override void HomeButton()
        {
            if (enabled)
                UIWindow.instance.P_FreeToolPanel.ElevationField.Reset();
        }

        const int DEFAULT_ELEVATTION = 3;

        private int GetElevationStep()
        {
            int ret = 0;
            if (ModLoadingExtension.fineRoadToolDetected)
            {
                ret = GetFRTElevation();
            }

            if (ret == 0)
            {
                switch (Singleton<NetTool>.instance.m_elevationDivider)
                {
                    case 1: return 12;
                    case 2: return 6;
                    case 4: return 3;
                    default:
                        Debug.LogWarning($"RoundaboutBuilder: Unreachable code. NetToool.m_elvationDivider={Singleton<NetTool>.instance.m_elevationDivider}");
                        return DEFAULT_ELEVATTION;
                }
            }

            return ret;
        }

        /// <summary>
        /// Precondition: FRT must be enabled 
        /// Note: Must not be inlined.
        /// Throws exception if FineRoadTool dll was not found.
        /// </summary>
        /// <returns>value of FRT elevation slide or DEFAULT_ELEVATTION if slider is uninitialized
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)] 
        private int GetFRTElevation()
        {
            // The method including the follwoing line throws exceptipn if FineRoadTool dll is not present.
            // this line must not be inlined.
            var ret = FineRoadTool.FineRoadTool.instance?.elevationStep ?? 0;

            // if user has never clicked on Road Tool, FRT slider is uninitialized.
            return ret != 0 ? ret : DEFAULT_ELEVATTION; 

        }


    }
}
