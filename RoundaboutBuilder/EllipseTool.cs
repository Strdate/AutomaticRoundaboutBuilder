using ColossalFramework.Math;
using RoundaboutBuilder.Tools;
using System;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.1.0 */

namespace RoundaboutBuilder
{
    /* Tool for building elliptic roundabouts */

    class EllipseTool : ToolBaseExtended
    {
        public static EllipseTool Instance;

        private static readonly int DISTANCE_PADDING = 15;
        public static readonly int RADIUS_MAX = 500;
        private static readonly int RADIUS_MIN = 10;
        private static readonly int RADIUS1_DEF = 60;
        private static readonly int RADIUS2_DEF = 30;

        ushort m_hover;
        string radius1String = RADIUS1_DEF.ToString();
        string radius2String = RADIUS2_DEF.ToString();

        ushort centralNode;
        ushort axisNode;

        // Saving radiuses from the last frame so that we don't have to recalculate the ellipse every time
        int prevRadius1 = 0;
        int prevRadius2 = 0;
        Ellipse ellipse;

        // UI:
        int stage = 1; // stage 1 - select central point; stage 2 - select main axis direction; stage 3 - adjust radiuses
        string controlVerticesString = "Control points (On)";
        bool controlVertices = true;

        /* Debug */
        /*public List<Bezier2> debugDraw = new List<Bezier2>();
        public List<Vector3> debugDrawPositions = new List<Vector3>();
        bool yes = true; // :)*/

        private void BuildEllipse()
        {
            Ellipse toBeBuiltEllipse = ellipse;
            Ellipse ellipseWithPadding = ellipse;
            /* When the old snapping algorithm is enabled, we create secondary (bigger) ellipse, so the newly connected roads obtained by the 
             * graph traveller are not too short. They will be at least as long as the padding. */
            if (UIWindow.Instance.OldSnappingAlgorithm)
            {
                ellipseWithPadding = new Ellipse(GetNode(centralNode).m_position, GetNode(axisNode).m_position - GetNode(centralNode).m_position, prevRadius1 + DISTANCE_PADDING, prevRadius2 + DISTANCE_PADDING);
            }

            UIWindow.Instance.LostFocus();
            UIWindow.Instance.GoToMenu();

            GraphTraveller2 traveller = new GraphTraveller2(centralNode,prevRadius1,ellipseWithPadding);
            EdgeIntersections2 intersections = new EdgeIntersections2(traveller, centralNode, toBeBuiltEllipse);
            FinalConnector finalConnector = new FinalConnector(GetNode(centralNode).Info, intersections.Intersections, toBeBuiltEllipse, controlVertices);

        }

        protected override void OnToolUpdate()
        {
            base.OnToolUpdate();

            m_hover = SelcetNode();

            if (m_hover != 0)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    switch(stage)
                    {
                        case 1:
                            centralNode = m_hover;
                            stage = 2; break;
                        case 2: if(m_hover == centralNode)
                            {
                                UIWindow.Instance.ThrowErrorMsg("You selected the same node!");
                            }
                            else
                            {
                                axisNode = m_hover;
                                stage = 3;
                            } break;
                    }
                }
            }
        }

        /* UI methods */

        public override void UIWindowMethod()
        {
            switch(stage)
            {
                case 1:
                    GUILayout.Label("Step 1/3:\nSelect center of the elliptic roundabout. (Nothing will be built yet)");
                    GUILayout.Space(1.5f*UIWindow.BUTTON_HEIGHT);
                    break;
                case 2:
                    GUILayout.Label("Step 2/3:\nSelect any intersection on the main axis of the roundabout. (Nothing will be built yet)");
                    GUILayout.Space(UIWindow.BUTTON_HEIGHT);
                    break;
                case 3:
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Main axis:");
                    radius1String = GUILayout.TextField(radius1String);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Minor axis:");
                    radius2String = GUILayout.TextField(radius2String);
                    GUILayout.EndHorizontal();
                    GUILayout.Label("Press +/- to adjust");
                    if (GUILayout.Button("Build"))
                    {
                        if (ellipse != null)
                            BuildEllipse();
                        else
                            UIWindow.Instance.ThrowErrorMsg("Invalid radiuses!");
                    } else if (GUILayout.Button(controlVerticesString))
                    {
                        controlVertices = !controlVertices;
                        if (controlVertices)
                        {
                            controlVerticesString = "Control points (On)";
                        }
                        else
                        {
                            controlVerticesString = "Control points (Off)";
                        }
                    }
                    break;
            }
        }

        /* Returns radiuses */
        private bool Radius(out int radius1, out int radius2)
        {
            radius1 = radius2 = -1;
            if (!Int32.TryParse(radius1String, out radius1) || !IsInBounds(radius1) || !Int32.TryParse(radius2String, out radius2) || !IsInBounds(radius2))
            {
                return false;
            }
            if(radius2 > radius1)
            {
                return false;
            }
            return true;
        }

        private static bool IsInBounds(int radius)
        {
            return radius >= RADIUS_MIN && radius <= RADIUS_MAX;
        }

        public override void IncreaseButton()
        {
            int newRadius1 = 0;
            int newRadius2 = 0;
            if (!Radius(out int radius1, out int radius2))
            {
                radius1String = RADIUS1_DEF.ToString();
                radius2String = RADIUS2_DEF.ToString();
                return;
            }
            else
            {
                double ratio = (double)radius2 / (double)radius1;
                newRadius1 = Convert.ToInt32(Math.Ceiling(new decimal(radius1 + 1) / new decimal(5))) * 5;
                newRadius2 = Convert.ToInt32(ratio* newRadius1);
            }
            if (IsInBounds(newRadius1) && IsInBounds(newRadius2) && newRadius1 >= newRadius2)
            {
                radius1String = newRadius1.ToString();
                radius2String = newRadius2.ToString();
            }
        }

        public override void DecreaseButton()
        {
            int newRadius1 = 0;
            int newRadius2 = 0;
            if (!Radius(out int radius1, out int radius2))
            {
                radius1String = RADIUS1_DEF.ToString();
                radius2String = RADIUS2_DEF.ToString();
                return;
            }
            else
            {
                double ratio = (double)radius2 / (double)radius1;
                newRadius1 = Convert.ToInt32(Math.Floor(new decimal(radius1 - 1) / new decimal(5))) * 5;
                newRadius2 = Convert.ToInt32(ratio * newRadius1);
                //Debug.Log(string.Format("decimal {0} int {1}", ((new decimal((value + 1) / 5)) * 5), newValue));
            }
            if (IsInBounds(newRadius1) && IsInBounds(newRadius2) && newRadius1 >= newRadius2)
            {
                radius1String = newRadius1.ToString();
                radius2String = newRadius2.ToString();
            }

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            stage = 1;
            ellipse = null;
        }

        /* This draws UI shapes on the map. */
        protected override void RenderOverlayExtended(RenderManager.CameraInfo cameraInfo)
        {
            //debugDrawMethod(cameraInfo);
            try
            {
                if (m_hover != 0 && (stage == 1 || stage == 2))
                {
                    NetNode hoveredNode = GetNode(m_hover);
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, hoveredNode.m_position, 15f, hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);

                }
                if (stage == 2 || stage == 3)
                {
                    NetNode centralNodeDraw = GetNode(centralNode);
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, centralNodeDraw.m_position, 15f, centralNodeDraw.m_position.y - 1f, centralNodeDraw.m_position.y + 1f, true, true);
                }
                if (stage == 3)
                {
                    NetNode axisNodeDraw = GetNode(axisNode);
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.green, axisNodeDraw.m_position, 15f, axisNodeDraw.m_position.y - 1f, axisNodeDraw.m_position.y + 1f, true, true);

                    if(Radius(out int radius1, out int radius2))
                    {
                        /* If the radiuses didn't change, we don't have to generate new ellipse. */
                        if(radius1 == prevRadius1 && radius2 == prevRadius2 && ellipse != null)
                        {
                            DrawEllipse(cameraInfo);
                        }
                        else
                        {
                            prevRadius1 = radius1;
                            prevRadius2 = radius2;
                            ellipse = new Ellipse(GetNode(centralNode).m_position, GetNode(axisNode).m_position - GetNode(centralNode).m_position, radius1, radius2);
                            DrawEllipse(cameraInfo);
                        }
                    }
                    else
                    {
                        ellipse = null;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.Log("Catched exception while rendering ellipse UI.");
                Debug.Log(e);
                stage = 1;
                enabled = false; // ???
            }
        }

        private void DrawEllipse(RenderManager.CameraInfo cameraInfo)
        {
            foreach(Bezier2 bezier in ellipse.Beziers)
            {
                RenderManager.instance.OverlayEffect.DrawBezier(cameraInfo, Color.red, ShiftTo3D(bezier), 0.1f, 0, 0, -1f, 1280f, false, true);
            }
        }

        public static Bezier3 ShiftTo3D(Bezier2 bezier)
        {
            Vector3 v1 = new Vector3(bezier.a.x, 0, bezier.a.y);
            Vector3 v2 = new Vector3(bezier.b.x, 0, bezier.b.y);
            Vector3 v3 = new Vector3(bezier.c.x, 0, bezier.c.y);
            Vector3 v4 = new Vector3(bezier.d.x, 0, bezier.d.y);
            return new Bezier3(v1, v2, v3, v4);
        }

        /* Debug methods */

        /* Allow me tho draw vectors and points on the map */

        /*private void debugDrawMethod(RenderManager.CameraInfo cameraInfo)
        {
            if (debugDraw == null) return;
            foreach (Bezier2 bezier in debugDraw)
            {
                if(yes)
                    RenderManager.instance.OverlayEffect.DrawBezier(cameraInfo, Color.yellow, ShiftTo3D(bezier), 0.1f, 0, 0, -1f, 1280f, false, true);
                else
                    RenderManager.instance.OverlayEffect.DrawBezier(cameraInfo, Color.blue, ShiftTo3D(bezier), 0.1f, 0, 0, -1f, 1280f, false, true);

                yes = !yes;
            }
            foreach (Vector3 vector in debugDrawPositions)
            {
                RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.cyan, vector, 15f, vector.y - 1f, vector.y + 1f, true, true);
            }
        }

        public void debugDrawVector(Vector3 vector, Vector3 startPos)
        {
            Vector2 vector2d = new Vector2(vector.x, vector.z);
            Vector2 a = new Vector2(startPos.x, startPos.z);
            Vector2 b = a + (vector2d * (1 / 3));
            Vector2 c = a + (vector2d * (2 / 3));
            Vector2 d = a + vector2d;
            debugDraw.Add(new Bezier2(a, b, c, d));
        }

        public void debugDrawPositionVector(Vector3 vector)
        {
            debugDrawPositions.Add(vector);
        }*/

        /*private float debugDistanceXZ (Vector3 v1, Vector3 v2)
        {
            return (float) Math.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.z - v2.z)*(v1.z - v2.z));
        }*/
    }    
}
