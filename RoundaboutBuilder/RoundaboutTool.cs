using ColossalFramework.UI;
using RoundaboutBuilder.Tools;
using RoundaboutBuilder.UI;
using System;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.2.0 */

namespace RoundaboutBuilder
{
    /* Tool for building standard non-elliptic roundabouts */

    class RoundaboutTool : ToolBaseExtended
    {
        public static RoundaboutTool Instance;

        private static readonly int DISTANCE_PADDING = 15;

        ushort m_hover;
        //string m_radiusField.text = RADIUS_DEF.ToString();

        /* Main method , called when user cliks on a node to create a roundabout */
        public void CreateRoundabout(ushort nodeID)
        {
            //Debug.Log(string.Format("Clicked on node ID {0}!", nodeID));

            int radius = UIWindow2.instance.P_RoundAboutPanel.RadiusField.Value;
            if (!NumericTextField.IsValid(radius))
            {
                UIWindow2.instance.ThrowErrorMsg("Radius out of bounds!");
                return;
            }

            if (!UIWindow2.instance.keepOpen)
                UIWindow2.instance.LostFocus();

            /* These lines of code do all the work. See documentation in respective classes. */
            /* When the old snapping algorithm is enabled, we create secondary (bigger) ellipse, so the newly connected roads obtained by the 
             * graph traveller are not too short. They will be at least as long as the padding. */
            Ellipse ellipse = new Ellipse(GetNode(nodeID).m_position, new Vector3(0f, 0f, 0f), radius, radius);
            Ellipse ellipseWithPadding = ellipse;
            if (RoundAboutBuilder.UseOldSnappingAlgorithm.value)
            {
                ellipseWithPadding = new Ellipse(GetNode(nodeID).m_position, new Vector3(0f, 0f, 0f), radius + DISTANCE_PADDING, radius + DISTANCE_PADDING);
            }


            try
            {
                GraphTraveller2 traveller = new GraphTraveller2(nodeID, radius, ellipse);
                EdgeIntersections2 intersections = new EdgeIntersections2(traveller, nodeID, ellipse);
                FinalConnector finalConnector = new FinalConnector(GetNode(nodeID).Info, intersections.Intersections, ellipse, true, intersections.TmpeActionGroup());

                // Debug, don't forget to remove
                /*foreach(VectorNodeStruct intersection in intersections.Intersections)
                {
                    Debug.Log($"May have/Has restrictions: {TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.MayHaveJunctionRestrictions(intersection.nodeId)}, {TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.HasJunctionRestrictions(intersection.nodeId)}");
                }*/

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UIWindow2.instance.ThrowErrorMsg(e.ToString(),true);
            }

        }

        /* This last part was more or less copied from Elektrix's Segment Slope Smoother. He takes the credit. 
         * https://github.com/CosignCosine/CS-SegmentSlopeSmoother
         * https://steamcommunity.com/sharedfiles/filedetails/?id=1597198847 */

        protected override void OnToolUpdate()
        {
            base.OnToolUpdate();

            m_hover = SelcetNode();

            if (m_hover != 0)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    CreateRoundabout(m_hover);
                }
            }
        }

        /* UI methods */

        public override void IncreaseButton()
        {
            UIWindow2.instance.P_RoundAboutPanel.RadiusField.Increase();
        }

        public override void DecreaseButton()
        {
            UIWindow2.instance.P_RoundAboutPanel.RadiusField.Decrease();
        }

        /* This draws the UI circles on the map */
        protected override void RenderOverlayExtended(RenderManager.CameraInfo cameraInfo)
        {
            if (m_hover != 0)
            {
                NetNode hoveredNode = GetNode(m_hover);

                // kinda stole this color from Move It!
                // thanks to SamsamTS because they're a UI god
                // ..and then Strad stole it from all of you!!
                RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, hoveredNode.m_position, 15f, hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);
                int radius = UIWindow2.instance.P_RoundAboutPanel.RadiusField.Value;
                if (NumericTextField.IsValid(radius))
                {
                    float roadWidth = UIWindow2.instance.dropDown.Value.m_halfWidth;
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, 2 * radius, hoveredNode.m_position.y - 2f, hoveredNode.m_position.y + 2f, true, true);
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, 2 * (radius + roadWidth /*DISTANCE_PADDING - 5*/), hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);
                }
                //RenderDirectionVectors(cameraInfo);
            }
        }

        // Unrelated debug
        /*private void NetAnalysis()
        {
            
                int errorsZeros = 0;
                int errorsNulls = 0;
            for (int i = 0; i < Singleton<NetManager>.instance.m_segments.m_buffer.Length; i++) 
                //foreach(NetSegment segment in Singleton<NetManager>.instance.m_segments.m_buffer)

                    {
                try
                {
                    NetSegment segment = Singleton<NetManager>.instance.m_segments.m_buffer[i];
                    if (ReferenceEquals(segment, null))
                        continue;
                    if (segment.m_startNode == 0 || segment.m_endNode == 0)
                    {
                        errorsZeros++;
                        NetManager.instance.ReleaseSegment((ushort)i, true);
                    }
                    else if (ReferenceEquals(GetNode(segment.m_startNode), null) || ReferenceEquals(GetNode(segment.m_endNode), null))
                    {
                        errorsNulls++;
                        NetManager.instance.ReleaseSegment((ushort)i, true);
                    }
                    
                }
                catch (Exception)
                {

                    
                }
                    }

                Debug.Log("errorsZeros: " + errorsZeros + ", errorsNulls: " + errorsNulls);
        }*/
    }
}
