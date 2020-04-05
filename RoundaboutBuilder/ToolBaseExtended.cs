using ColossalFramework;
using ColossalFramework.UI;
using RoundaboutBuilder.Tools;
using RoundaboutBuilder.UI;
using System;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.2.0 */

/* This method etends standard tool base. It adds UI environment, which can be called by the main UI class. The subclasses can override
 * the +/- button methods to register keyboard input. */

namespace RoundaboutBuilder
{
    public abstract class ToolBaseExtended : ToolBase
    {
        protected bool insideUI;

        protected ushort m_hoverNode;
        public Vector3 HoverPosition { get; protected set; }

        protected override void OnToolUpdate()
        {
            base.OnToolUpdate();

        /* This last part was more or less copied from Elektrix's Segment Slope Smoother. He takes the credit. 
         * https://github.com/CosignCosine/CS-SegmentSlopeSmoother
         * https://steamcommunity.com/sharedfiles/filedetails/?id=1597198847 */

            if (/*UIWindow2.instance.containsMouse || */(UIView.IsInsideUI() || !Cursor.visible))
            {
                m_hoverNode = 0;
                insideUI = true;
                return;
            }

            insideUI = false;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastInput input = new RaycastInput(ray, Camera.main.farClipPlane);

            input.m_ignoreNodeFlags = NetNode.Flags.None;
            input.m_ignoreSegmentFlags = NetSegment.Flags.All;
            input.m_ignoreParkFlags = DistrictPark.Flags.All;
            input.m_ignorePropFlags = PropInstance.Flags.All;
            input.m_ignoreTreeFlags = TreeInstance.Flags.All;
            input.m_ignoreCitizenFlags = CitizenInstance.Flags.All;
            input.m_ignoreVehicleFlags = Vehicle.Flags.Created;
            input.m_ignoreBuildingFlags = Building.Flags.All;
            input.m_ignoreDisasterFlags = DisasterData.Flags.All;
            input.m_ignoreTransportFlags = TransportLine.Flags.All;
            input.m_ignoreParkedVehicleFlags = VehicleParked.Flags.All;
            input.m_ignoreTerrain = false;

            RayCast(input, out RaycastOutput output);
            m_hoverNode = output.m_netNode;
            HoverPosition = output.m_hitPos;

            if (Input.GetMouseButtonUp(0))
            {
                OnClick();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if(UIWindow.instance != null)
            {
                UIWindow.instance.LostFocus();
                if (UIWindow.instance.m_hoveringLabel != null)
                    UIWindow.instance.m_hoveringLabel.isVisible = false;
            }
            ToolsModifierControl.SetTool<DefaultTool>(); // Thanks to Elektrix for pointing this out
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);
            if (enabled == true)
            {
                RenderOverlayExtended(cameraInfo);
            } else
            {
                try
                {
                    UIWindow.instance.m_hoveringLabel.isVisible = false;
                } catch { }
            }
        }

        protected virtual void OnClick()
        {

        }

        protected virtual void RenderOverlayExtended(RenderManager.CameraInfo cameraInfo)
        {

        }

        public virtual void IncreaseButton()
        {

        }

        public virtual void DecreaseButton()
        {

        }

        public virtual void PgUpButton()
        {

        }

        public virtual void PgDnButton()
        {

        }

        public virtual void HomeButton()
        {

        }

        public virtual void GoToFirstStage()
        {

        }

        protected static bool GetFollowTerrain()
        {
            return UIWindow.SavedFollowTerrain.value;
        }

        protected static void RenderHoveringLabel(string text)
        {
            UIWindow.instance.m_hoveringLabel.absolutePosition = UIView.GetAView().ScreenPointToGUI(Input.mousePosition) + new Vector2(50, 30);
            UIWindow.instance.m_hoveringLabel.SetValue(text);
            UIWindow.instance.m_hoveringLabel.isVisible = true;
        }

        protected void RenderMousePositionCircle(RenderManager.CameraInfo cameraInfo)
        {
            RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, HoverPosition, 15f, HoverPosition.y - 1f, HoverPosition.y + 1f, true, true);
        }

        /* I need this code on multiple places, so I implement it as a static method... Return the node over which the mouse is hovering. */
        /* Copied from Elektrix's Segment Slope Smoother. Credit goes to him. */
        /*protected static ushort SelcetNode()
        {
            if (UIWindow2.instance.containsMouse || (UIView.IsInsideUI() || !Cursor.visible))
                return 0;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastInput input = new RaycastInput(ray, Camera.main.farClipPlane);
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            
            input.m_ignoreSegmentFlags = NetSegment.Flags.All;
            input.m_ignoreParkFlags = DistrictPark.Flags.All;
            input.m_ignorePropFlags = PropInstance.Flags.All;
            input.m_ignoreTreeFlags = TreeInstance.Flags.All;
            input.m_ignoreCitizenFlags = CitizenInstance.Flags.All;
            input.m_ignoreVehicleFlags = Vehicle.Flags.Created;
            input.m_ignoreBuildingFlags = Building.Flags.All;
            input.m_ignoreDisasterFlags = DisasterData.Flags.All;
            input.m_ignoreTransportFlags = TransportLine.Flags.All;
            input.m_ignoreParkedVehicleFlags = VehicleParked.Flags.All;
            input.m_ignoreTerrain = true;

            RayCast(input, out RaycastOutput output);
            
            return output.m_netNode;
        }*/
    }
}
