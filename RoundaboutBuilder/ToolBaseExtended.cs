using ColossalFramework;
using ColossalFramework.UI;
using RoundaboutBuilder.UI;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.2.0 */

/* This method etends standard tool base. It adds UI environment, which can be called by the main UI class. The subclasses can override
 * the +/- button methods to register keyboard input. */

namespace RoundaboutBuilder
{
    public abstract class ToolBaseExtended : ToolBase
    {
        protected override void OnDisable()
        {
            base.OnDisable();
            if(UIWindow2.instance != null)
                UIWindow2.instance.LostFocus();
            ToolsModifierControl.SetTool<DefaultTool>(); // Thanks to Elektrix for pointing this out
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);
            if (enabled == true)
            {
                RenderOverlayExtended(cameraInfo);
            }
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

        public virtual void GoToFirstStage()
        {

        }

        /* I need this code on multiple places, so I implement it as a static method... Return the node over which the mouse is hovering. */
        /* Copied from Elektrix's Segment Slope Smoother. Credit goes to him. */
        protected static ushort SelcetNode()
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
        }

        /* Utils */

        public static NetManager Manager
        {
            get { return Singleton<NetManager>.instance; }
        }
        public static NetNode GetNode(ushort id)
        {
            return Manager.m_nodes.m_buffer[id];
        }
        public static NetSegment GetSegment(ushort id)
        {
            return Manager.m_segments.m_buffer[id];
        }
    }
}
