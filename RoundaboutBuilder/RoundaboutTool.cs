using ColossalFramework.UI;
using RoundaboutBuilder.Tools;
using RoundaboutBuilder.UI;
using SharedEnvironment;
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

        private FinalConnector m_roundabout;
        private ushort m_nodeID;
        private float m_radius;

        //string m_radiusField.text = RADIUS_DEF.ToString();

        /* Main method , called when user cliks on a node to create a roundabout */
        public void CreateRoundabout(ushort nodeID)
        {
            //Debug.Log(string.Format("Clicked on node ID {0}!", nodeID));

            if (!UIWindow.instance.keepOpen)
                UIWindow.instance.LostFocus();

            try
            {
                if (m_roundabout != null && nodeID == m_nodeID)
                {
                    m_roundabout.Build();
                    // Easter egg
                    RoundAboutBuilder.EasterEggToggle();
                }

                // Debug, don't forget to remove
                /*foreach(VectorNodeStruct intersection in intersections.Intersections)
                {
                    Debug.Log($"May have/Has restrictions: {TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.MayHaveJunctionRestrictions(intersection.nodeId)}, {TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.HasJunctionRestrictions(intersection.nodeId)}");
                }*/

            }
            catch (ActionException e)
            {
                UIWindow.instance.ThrowErrorMsg(e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UIWindow.instance.ThrowErrorMsg(e.ToString(),true);
            }

        }

        private void PreviewRoundabout(float radius)
        {
            /* These lines of code do all the work. See documentation in respective classes. */
            /* When the old snapping algorithm is enabled, we create secondary (bigger) ellipse, so the newly connected roads obtained by the 
             * graph traveller are not too short. They will be at least as long as the padding. */
            bool reverseDirection = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && RoundAboutBuilder.CtrlToReverseDirection.value;
            if (m_radius == radius && m_hoverNode == m_nodeID && m_roundabout != null && m_roundabout.m_reverseDirection == reverseDirection)
            {
                return;
            }
            try
            {
                m_radius = radius;
                m_nodeID = m_hoverNode;
                Ellipse ellipse = new Ellipse(NetUtil.Node(m_nodeID).m_position, new Vector3(0f, 0f, 0f), radius, radius);
                Ellipse ellipseWithPadding = ellipse;
                if (RoundAboutBuilder.UseOldSnappingAlgorithm.value)
                {
                    ellipseWithPadding = new Ellipse(NetUtil.Node(m_nodeID).m_position, new Vector3(0f, 0f, 0f), radius + DISTANCE_PADDING, radius + DISTANCE_PADDING);
                }
                GraphTraveller2 traveller;
                EdgeIntersections2 intersections = null;
                if (!RoundAboutBuilder.DoNotRemoveAnyRoads)
                {
                    traveller = new GraphTraveller2(m_nodeID, ellipse);
                    intersections = new EdgeIntersections2(traveller, m_nodeID, ellipse, GetFollowTerrain());
                }
                m_roundabout = new FinalConnector(NetUtil.Node(m_nodeID).Info, intersections, ellipse, true, GetFollowTerrain(), reverseDirection);
            } catch(Exception e) { Debug.LogError(e); }
            
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
                UIWindow.instance.P_RoundAboutPanel.label.text = "Click inside the window to reactivate the tool";
            }
            catch(NullReferenceException) { }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            try
            {
                System.Random rand = new System.Random();
                string text = ""; // a little bit of advertising never hurt anyone
                switch(rand.Next(6))
                {
                    case 0: case 1: text =  "Tip: Use Fine Road Tool for elevated roads"; break;
                    case 2: case 3: text =  "Tip: Use this with any network (see options)"; break;
                    case 4: text =          "Tip: Check out Smart Intersection Builder!"; break;
                    case 5: text =          "Tip: Check out Adjust Pathfinding mod!"; break;
                    default: text =         "Tip: Use Fine Road Tool for elevated roads"; break;
                }
                UIWindow.instance.P_RoundAboutPanel.label.text = text;
            }
            catch (NullReferenceException) { }
        }

        /* UI methods */

        public override void IncreaseButton()
        {
            UIWindow.instance.P_RoundAboutPanel.RadiusField.Increase();
        }

        public override void DecreaseButton()
        {
            UIWindow.instance.P_RoundAboutPanel.RadiusField.Decrease();
        }

        /* This draws the UI circles on the map */
        protected override void RenderOverlayExtended(RenderManager.CameraInfo cameraInfo)
        {
            try
            {
                if (insideUI)
                    return;

                if (m_hoverNode != 0)
                {
                    NetNode hoveredNode = NetUtil.Node(m_hoverNode);

                    // kinda stole this color from Move It!
                    // thanks to SamsamTS because they're a UI god
                    // ..and then Strad stole it from all of you!!
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, hoveredNode.m_position, 15f, hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);
                    float? radius = UIWindow.instance.P_RoundAboutPanel.RadiusField.Value;
                    if (radius != null)
                    {
                        PreviewRoundabout((float)radius);
                        float roadWidth = UIWindow.instance.dropDown.Value.m_halfWidth; // There is a slight chance that this will throw an exception
                        float innerCirleRadius = radius - roadWidth > 0 ? 2 * ((float)radius - roadWidth) : 2 * (float)radius;
                        RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, innerCirleRadius, hoveredNode.m_position.y - 2f, hoveredNode.m_position.y + 2f, true, true);
                        RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, 2 * ((float)radius + roadWidth /*DISTANCE_PADDING - 5*/), hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);
                        RenderHoveringLabel("Cost: " + (m_roundabout.Cost / 100) + "\nClick to build\nPress +/- to adjust radius");
                    }
                    else
                    {
                        m_roundabout = null;
                        RenderHoveringLabel("Invalid radius\nPress +/- to adjust radius");
                    }
                    //RenderDirectionVectors(cameraInfo);
                }
                else
                {
                    m_roundabout = null;
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
