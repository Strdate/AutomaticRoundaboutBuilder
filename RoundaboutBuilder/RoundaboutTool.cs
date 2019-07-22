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

        //string m_radiusField.text = RADIUS_DEF.ToString();

        /* Main method , called when user cliks on a node to create a roundabout */
        public void CreateRoundabout(ushort nodeID)
        {
            //Debug.Log(string.Format("Clicked on node ID {0}!", nodeID));

            float? radiusQ = UIWindow2.instance.P_RoundAboutPanel.RadiusField.Value;
            if (radiusQ == null)
            {
                UIWindow2.instance.ThrowErrorMsg("Radius out of bounds!");
                return;
            }

            float radius = (float)radiusQ;

            if (!UIWindow2.instance.keepOpen)
                UIWindow2.instance.LostFocus();

            /* These lines of code do all the work. See documentation in respective classes. */
            /* When the old snapping algorithm is enabled, we create secondary (bigger) ellipse, so the newly connected roads obtained by the 
             * graph traveller are not too short. They will be at least as long as the padding. */
            Ellipse ellipse = new Ellipse(NetAccess.Node(nodeID).m_position, new Vector3(0f, 0f, 0f), radius, radius);
            Ellipse ellipseWithPadding = ellipse;
            if (RoundAboutBuilder.UseOldSnappingAlgorithm.value)
            {
                ellipseWithPadding = new Ellipse(NetAccess.Node(nodeID).m_position, new Vector3(0f, 0f, 0f), radius + DISTANCE_PADDING, radius + DISTANCE_PADDING);
            }


            try
            {
                GraphTraveller2 traveller;
                EdgeIntersections2 intersections = null;
                if (!RoundAboutBuilder.DoNotRemoveAnyRoads)
                {
                    traveller = new GraphTraveller2(nodeID, ellipse);
                    intersections = new EdgeIntersections2(traveller, nodeID, ellipse);
                }
                FinalConnector finalConnector = new FinalConnector(NetAccess.Node(nodeID).Info, intersections, ellipse, true);

                // Easter egg
                RoundAboutBuilder.EasterEggToggle();

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

        protected override void OnClick()
        {
            base.OnClick();
            if(m_hoverNode != 0)
            {
                CreateRoundabout(m_hoverNode);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            try
            {
                UIWindow2.instance.P_RoundAboutPanel.label.text = "Click inside the window to reactivate the tool";
            }
            catch(NullReferenceException) { }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            try
            {
                UIWindow2.instance.P_RoundAboutPanel.label.text = "Tip: Use Fine Road Tool for elevated roads";
            }
            catch (NullReferenceException) { }
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
            try
            {
                if (m_hoverNode != 0)
                {
                    NetNode hoveredNode = NetAccess.Node(m_hoverNode);

                    // kinda stole this color from Move It!
                    // thanks to SamsamTS because they're a UI god
                    // ..and then Strad stole it from all of you!!
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, hoveredNode.m_position, 15f, hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);
                    float? radius = UIWindow2.instance.P_RoundAboutPanel.RadiusField.Value;
                    if (radius != null)
                    {
                        float roadWidth = UIWindow2.instance.dropDown.Value.m_halfWidth; // There is a slight chance that this will throw an exception
                        float innerCirleRadius = radius - roadWidth > 0 ? 2 * ((float)radius - roadWidth) : 2 * (float)radius;
                        RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, innerCirleRadius, hoveredNode.m_position.y - 2f, hoveredNode.m_position.y + 2f, true, true);
                        RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, 2 * ((float)radius + roadWidth /*DISTANCE_PADDING - 5*/), hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);
                    }
                    //RenderDirectionVectors(cameraInfo);
                    RenderHoveringLabel("Click to build\nPress +/- to adjust radius");
                }
                else
                {
                    RenderMousePositionCircle(cameraInfo);
                    RenderHoveringLabel("Hover mouse over intersection");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
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
