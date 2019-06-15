using ColossalFramework.Math;
using ColossalFramework.UI;
using RoundaboutBuilder.Tools;
using RoundaboutBuilder.UI;
using System;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version RELEASE 1.4.0+ */

namespace RoundaboutBuilder
{
    /* Tool for building elliptic roundabouts */

    class EllipseTool : ToolBaseExtended
    {
        public static EllipseTool Instance;

        private static readonly int DISTANCE_PADDING = 15;
        private static readonly int RADIUS1_DEF = 60;
        private static readonly int RADIUS2_DEF = 30;

        ushort m_hover;

        ushort centralNode;
        ushort axisNode;

        // Saving radii from the last frame so that we don't have to recalculate the ellipse every time
        float prevRadius1 = 0;
        float prevRadius2 = 0;
        Ellipse ellipse;

        // UI:
        private enum Stage
        {
            CentralPoint, // First user chooses cental point
            MainAxis, // Then point on main axis
            Final // And then sets radii and stuff
        }
        Stage stage = Stage.CentralPoint;
        public bool ControlVertices = true;

        /* Debug */
        /*public List<Bezier2> debugDraw = new List<Bezier2>();
        public List<Vector3> debugDrawPositions = new List<Vector3>();
        bool yes = true; // :)*/

        public void BuildEllipse()
        {
            if (ellipse == null)
                UIWindow2.instance.ThrowErrorMsg("Invalid radii!");

            Ellipse toBeBuiltEllipse = ellipse;
            Ellipse ellipseWithPadding = ellipse;
            /* When the old snapping algorithm is enabled, we create secondary (bigger) ellipse, so the newly connected roads obtained by the 
             * graph traveller are not too short. They will be at least as long as the padding. */
            if (RoundAboutBuilder.UseOldSnappingAlgorithm.value)
            {
                ellipseWithPadding = new Ellipse(GetNode(centralNode).m_position, GetNode(axisNode).m_position - GetNode(centralNode).m_position, prevRadius1 + DISTANCE_PADDING, prevRadius2 + DISTANCE_PADDING);
            }

            UIWindow2.instance.LostFocus();
            UIWindow2.instance.GoBack();

            try
            {
                GraphTraveller2 traveller = new GraphTraveller2(centralNode, ellipseWithPadding);
                EdgeIntersections2 intersections = new EdgeIntersections2(traveller, centralNode, toBeBuiltEllipse);
                FinalConnector finalConnector = new FinalConnector(GetNode(centralNode).Info, intersections, toBeBuiltEllipse, ControlVertices);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UIWindow2.instance.ThrowErrorMsg(e.ToString(), true);
            }

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
                        case Stage.CentralPoint:
                            centralNode = m_hover;
                            stage = Stage.MainAxis;
                            UIWindow2.instance.SwitchWindow(UIWindow2.instance.P_EllipsePanel_2);
                            break;
                        case Stage.MainAxis: if(m_hover == centralNode)
                            {
                                UIWindow2.instance.ThrowErrorMsg("You selected the same node!");
                            }
                            else
                            {
                                axisNode = m_hover;
                                stage = Stage.Final;
                                UIWindow2.instance.SwitchWindow(UIWindow2.instance.P_EllipsePanel_3);
                            } break;
                    }
                }
            }
        }

        /* Returns radii */
        private bool Radius(out float radius1, out float radius2)
        {
            radius1 = radius2 = -1;
            float? radius1Q = UIWindow2.instance.P_EllipsePanel_3.Radius1tf.Value;
            float? radius2Q = UIWindow2.instance.P_EllipsePanel_3.Radius2tf.Value;
            if (radius1Q == null || radius2Q == null)
            {
                return false;
            }

            radius1 = (float)radius1Q;
            radius2 = (float)radius2Q;

            if (radius2 > radius1)
            {
                return false;
            }
            return true;
        }

        public override void IncreaseButton()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                UIWindow2.instance.P_EllipsePanel_3.Radius1tf.Increase();
            } else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                UIWindow2.instance.P_EllipsePanel_3.Radius2tf.Increase();
            } else
            {

                int newRadius1 = 0;
                int newRadius2 = 0;
                if (!Radius(out float radius1, out float radius2))
                {
                    UIWindow2.instance.P_EllipsePanel_3.Radius1tf.text = RADIUS1_DEF.ToString();
                    UIWindow2.instance.P_EllipsePanel_3.Radius1tf.text = RADIUS2_DEF.ToString();
                    return;
                }
                else
                {
                    double ratio = (double)radius2 / (double)radius1;
                    newRadius1 = Convert.ToInt32(Math.Ceiling(new decimal(radius1 + 1) / new decimal(5))) * 5;
                    newRadius2 = Convert.ToInt32(ratio * newRadius1);
                }
                if (UIWindow2.instance.P_EllipsePanel_3.Radius1tf.IsValid(newRadius1) && UIWindow2.instance.P_EllipsePanel_3.Radius2tf.IsValid(newRadius2) && newRadius1 >= newRadius2)
                {
                    UIWindow2.instance.P_EllipsePanel_3.Radius1tf.text = newRadius1.ToString();
                    UIWindow2.instance.P_EllipsePanel_3.Radius2tf.text = newRadius2.ToString();
                }

            }
        }

        public override void DecreaseButton()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                UIWindow2.instance.P_EllipsePanel_3.Radius1tf.Decrease();
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                UIWindow2.instance.P_EllipsePanel_3.Radius2tf.Decrease();
            }
            else
            {

                int newRadius1 = 0;
                int newRadius2 = 0;
                if (!Radius(out float radius1, out float radius2))
                {
                    UIWindow2.instance.P_EllipsePanel_3.Radius1tf.text = RADIUS1_DEF.ToString();
                    UIWindow2.instance.P_EllipsePanel_3.Radius1tf.text = RADIUS2_DEF.ToString();
                    return;
                }
                else
                {
                    double ratio = (double)radius2 / (double)radius1;
                    newRadius1 = Convert.ToInt32(Math.Floor(new decimal(radius1 - 1) / new decimal(5))) * 5;
                    newRadius2 = Convert.ToInt32(ratio * newRadius1);
                }
                if (UIWindow2.instance.P_EllipsePanel_3.Radius1tf.IsValid(newRadius1) && UIWindow2.instance.P_EllipsePanel_3.Radius2tf.IsValid(newRadius2) && newRadius1 >= newRadius2)
                {
                    UIWindow2.instance.P_EllipsePanel_3.Radius1tf.text = newRadius1.ToString();
                    UIWindow2.instance.P_EllipsePanel_3.Radius2tf.text = newRadius2.ToString();
                }

            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ellipse = null;
        }

        public override void GoToFirstStage()
        {
            stage = Stage.CentralPoint;
        }

        /* This draws UI shapes on the map. */
        protected override void RenderOverlayExtended(RenderManager.CameraInfo cameraInfo)
        {
            //debugDrawMethod(cameraInfo);
            try
            {
                if (m_hover != 0 && (stage == Stage.CentralPoint || stage == Stage.MainAxis))
                {
                    NetNode hoveredNode = GetNode(m_hover);
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, hoveredNode.m_position, 15f, hoveredNode.m_position.y - 1f, hoveredNode.m_position.y + 1f, true, true);

                }
                if (stage == Stage.MainAxis || stage == Stage.Final)
                {
                    NetNode centralNodeDraw = GetNode(centralNode);
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, centralNodeDraw.m_position, 15f, centralNodeDraw.m_position.y - 1f, centralNodeDraw.m_position.y + 1f, true, true);
                }
                if (stage == Stage.Final)
                {
                    NetNode axisNodeDraw = GetNode(axisNode);
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.green, axisNodeDraw.m_position, 15f, axisNodeDraw.m_position.y - 1f, axisNodeDraw.m_position.y + 1f, true, true);

                    if(Radius(out float radius1, out float radius2))
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
                stage = Stage.CentralPoint;
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
