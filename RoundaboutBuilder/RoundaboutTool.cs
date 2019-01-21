using ColossalFramework;
using RoundaboutBuilder.Tools;
using System;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.1.0 */

namespace RoundaboutBuilder
{
    /* Tool for building standard non-elliptic roundabouts */

    class RoundaboutTool : ToolBaseExtended
    {
        public static RoundaboutTool Instance;

        private static readonly int DISTANCE_PADDING = 15;
        private static readonly int RADIUS_MAX = 500;
        private static readonly int RADIUS_MIN = 5;
        private static readonly int RADIUS_DEF = 40;

        ushort m_hover;
        string textAreaString = RADIUS_DEF.ToString();

        /* Main method , called when user cliks on a node to create a roundabout */
        public void CreateRoundabout(ushort nodeID)
        {
            //Debug.Log(string.Format("Clicked on node ID {0}!", nodeID));

            int radius = Radius();
            if (radius == -1)
            {
                UIWindow.Instance.ThrowErrorMsg("Radius out of bounds!");
                return;
            }

            if (!UIWindow.Instance.keepWindowOpen)
                UIWindow.Instance.LostFocus();

            /* These lines of code do all the work. See documentation in respective classes. */
            /* When the old snapping algorithm is enabled, we create secondary (bigger) ellipse, so the newly connected roads obtained by the 
             * graph traveller are not too short. They will be at least as long as the padding. */
            Ellipse ellipse = new Ellipse(GetNode(nodeID).m_position, new Vector3(0f, 0f, 0f), radius, radius);
            Ellipse ellipseWithPadding = ellipse;
            if (UIWindow.Instance.OldSnappingAlgorithm)
            {
                ellipseWithPadding = new Ellipse(GetNode(nodeID).m_position, new Vector3(0f, 0f, 0f), radius + DISTANCE_PADDING, radius + DISTANCE_PADDING);
            }
            GraphTraveller2 traveller = new GraphTraveller2(nodeID, radius, ellipse);
            EdgeIntersections2 intersections = new EdgeIntersections2(traveller, nodeID, ellipse);
            FinalConnector finalConnector = new FinalConnector(GetNode(nodeID).Info, intersections.Intersections, ellipse, true);

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

        public override void UIWindowMethod()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius:");
            textAreaString = GUILayout.TextField(textAreaString);
            GUILayout.EndHorizontal();
            GUILayout.Label("Press +/- to adjust");
            GUILayout.Space(3 * UIWindow.BUTTON_HEIGHT);
        }

        private int Radius()
        {
            int i = 0;
            if (!Int32.TryParse(textAreaString, out i) || !IsInBounds(i))
            {
                //Debug.Log(string.Format("parse {0} bounds {1}", Int32.TryParse(textAreaString, out i), IsInBounds(i)));
                return -1;
            }
            return i;
        }

        private static bool IsInBounds(int radius)
        {
            return radius >= RADIUS_MIN && radius <= RADIUS_MAX;
        }

        public override void IncreaseButton()
        {
            int newValue = 0;
            int value = Radius();
            if (!IsInBounds(value))
            {
                textAreaString = RADIUS_DEF.ToString();
                return;
            }
            else
            {
                newValue = Convert.ToInt32(Math.Ceiling(new decimal(value + 1) / new decimal(5))) * 5;
                //Debug.Log(string.Format("decimal {0} int {1}", ((new decimal((value + 1) / 5)) * 5), newValue));
            }
            if (IsInBounds(newValue)) textAreaString = newValue.ToString(); else value.ToString();
        }

        public override void DecreaseButton()
        {
            int newValue = 0;
            int value = Radius();
            if (!IsInBounds(value))
            {
                textAreaString = RADIUS_DEF.ToString();
                return;
            }
            else
            {
                newValue = (int)(Math.Floor(new Decimal((value - 1) / 5)) * 5);
            }
            if (IsInBounds(newValue)) textAreaString = newValue.ToString(); else value.ToString();

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
                int radius = Radius();
                if (radius != -1)
                {
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, 2 * radius, hoveredNode.m_position.y - 2f, hoveredNode.m_position.y + 2f, true, true);
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, hoveredNode.m_position, 2 * (radius + DISTANCE_PADDING - 5), hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);
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
