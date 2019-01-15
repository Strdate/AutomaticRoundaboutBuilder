
using ColossalFramework;
using RoundaboutBuilder.Tools;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.0.0 */

namespace RoundaboutBuilder
{
    public class NodeSelection : ToolBase
    {
        public static NodeSelection instance;

        ushort m_hover;

        public NetManager Manager
        {
            get { return Singleton<NetManager>.instance; }
        }
        public NetNode GetNode(ushort id)
        {
            return Manager.m_nodes.m_buffer[id];
        }
        public NetSegment GetSegment(ushort id)
        {
            return Manager.m_segments.m_buffer[id];
        }

        /* Main method of mod, called when user cliks on a node to create a roundabout */
        public void CreateRoundabout(ushort nodeID)
        {
            //Debug.Log(string.Format("Clicked on node ID {0}!", nodeID));
            
            int radius = UIWindow.Instance.Radius;
            if (radius == -1)
            {
                UIWindow.Instance.ThrowErrorMsg("Radius out of bounds!");
                return;
            }

            UIWindow.Instance.LostFocus();

            /* These three lines of code do all the work. See documentation in respective classes. */

            GraphTraveller traveller = new GraphTraveller(nodeID, radius);

            CircleIntersectionsKit circleIntersectionsKit = new CircleIntersectionsKit(nodeID, traveller,radius);

            CircleConnectorKit circleConnectorKit = new CircleConnectorKit(nodeID,circleIntersectionsKit.ReturnIntersectionCircles());

        }

        /* This last part was more or less copied from Elektrix's Segment Slope Smoother. He takes the credit. 
         * https://github.com/CosignCosine/CS-SegmentSlopeSmoother
         * https://steamcommunity.com/sharedfiles/filedetails/?id=1597198847 */

        protected override void OnToolUpdate()
        {
            base.OnToolUpdate();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastInput input = new RaycastInput(ray, Camera.main.farClipPlane);
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;

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
            m_hover = output.m_netNode;

            if (m_hover != 0)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    CreateRoundabout(m_hover);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //Debug.Log("Tool on enable");
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            //Debug.Log("Tool on disable");
            UIWindow.Instance.LostFocus();
        }

        /* This draws the UI circles on the map */
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);
            if (enabled == true)
            {
                if (m_hover != 0)
                {
                    NetNode hoveredNode = GetNode(m_hover);

                    // kinda stole this color from Move It!
                    // thanks to SamsamTS because they're a UI god
                    // ..and then Strad stole it from all of you!!
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, hoveredNode.m_position, 15f, hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);
                    int radius = UIWindow.Instance.Radius;
                    if (radius != -1)
                    {
                        RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, 2*radius, hoveredNode.m_position.y - 2f, hoveredNode.m_position.y + 2f, true, true);
                        RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, 2*(radius + GraphTraveller.DISTANCE_PADDING - 5), hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);
                    }
                    //RenderDirectionVectors(cameraInfo);
                }
            }
        }

        /* Completely unrelated */

        /* I used this method to show in game the direction vectors of net segments */
        /*private void RenderDirectionVectors(RenderManager.CameraInfo cameraInfo)
        {
            NetNode netNode = GetNode(m_hover);
            for(int i = 0; i < 10; i++)
            {
                ushort segment = netNode.GetSegment(i);
                if (segment == 0) continue;
                NetSegment netSegment = GetSegment(segment);
                Vector3 pos1 = netNode.m_position;
                Vector3 pos2;
                if( netSegment.m_startNode == m_hover )
                {
                    pos2 = netNode.m_position + netSegment.m_startDirection;
                }
                else
                {
                    pos2 = netNode.m_position + netSegment.m_endDirection;
                }
                RenderManager.instance.OverlayEffect.DrawSegment(cameraInfo, new Color(128f, 128f, 128f), new ColossalFramework.Math.Segment3(pos1, pos2), 0.5f, 0.5f, -1f, 1280f, false, true);

            }
        }*/
    }
}
