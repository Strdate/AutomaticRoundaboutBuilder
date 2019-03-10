using ColossalFramework;
using ColossalFramework.Math;
using Provisional.Actions;
using SharedEnvironment.Public.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version RELEASE 1.1.0+ */

namespace RoundaboutBuilder.Tools
{
    /* This class takes the nodes obtained from EdgeIntersections2 and connects them together to create the final roundabout. */

    /* Warning! This is not the most efficient algorithm on Earth, as you will see... */

    /* This is all such garbage with old algortithms mixing with new and other stuff... Not the nicest piece of code. */

    /* To make this faster, you could use some pre-sorted list of nodes or something like that. I am not implementing that. (for now) */

    public class FinalConnector
    {
        //Since RELEASE 1.1.0 this variable is dependent on radius
        //private static readonly double MAX_ANGULAR_DISTANCE = Math.PI / 2d + 0.1d; // Maximal angular distance between two nodes on the circle
        //private static readonly double INTERMEDIATE_NODE_DISTANCE = Math.PI / 3d;
        private static readonly int DISTANCE_MIN = 20; // Min distance from other nodes when inserting controlling node

        private List<VectorNodeStruct> intersections;
        private bool leftHandTraffic;
        private Ellipse ellipse;
        private NetInfo centerNodeNetInfo;
        private ActionGroup m_group;
        private double m_maxAngDistance;

        // Time to time something goes wrong. Let's make sure that we don't get stuck in infinite recursion.
        // Didn't happen to me since I degbugged it, but one never knows for sure.
        private int pleasenoinfiniterecursion;

        public FinalConnector(NetInfo centerNodeNetInfo, List<VectorNodeStruct> intersections, Ellipse ellipse, bool insertControllingVertices, ActionGroup tmpeActionGroup)
        {
            this.ellipse = ellipse;
            pleasenoinfiniterecursion = 0;
            this.intersections = intersections;
            this.centerNodeNetInfo = centerNodeNetInfo;
            leftHandTraffic = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic ==
                                    SimulationMetaData.MetaBool.True;
            m_group = tmpeActionGroup;

            // For circles only
            m_maxAngDistance = Math.Min(Math.PI * 25 / ellipse.RadiusMain , Math.PI/2 + 0.1d);

            if(!ellipse.IsCircle() && insertControllingVertices)
            {
                /* See doc in the method below */
                InsertIntermediateNodes();
            }

            /* Goes over all the nodes and conntets each of them to the angulary closest neighbour. (In a given direction) */
            for (int i = 0; i < intersections.Count; i++)
            {
                FindClosestAndConnect(intersections[i]);
            }

            ModThreading.Timer(m_group);
        }

        private void FindClosestAndConnect(VectorNodeStruct intersection)
        {
            recursionGuard();

            VectorNodeStruct closestNode = getClosestNode(intersection.vector, out double angDistance);

            /* If the distance between two nodes is too great, we put in an intermediate node inbetween */
            // Old method, now using only for circles. Ellipses have control points instead
            if (ellipse.IsCircle() && angDistance > m_maxAngDistance)
            {
                closestNode = AddIntermediateNodeCircle(intersection.nodeId,angDistance);
            }
            ConnectNodes(intersection, closestNode, angDistance);
        }

        /*private VectorNodeStruct getNotTooCloseNode(Vector3 vector, out double angDistance)
        {
            VectorNodeStruct closestNode = getClosestNode(vector, out angDistance);

            recursionGuard();

            // if too close and the node actually does not exist
            if (closestNode.nodeId == 0 && VectorDistance(closestNode.vector, vector) < DISTANCE_MIN)
            {
                intersections.Remove(closestNode);
                closestNode = getNotTooCloseNode(vector, out angDistance);
                Debug.Log("Node too close, removing from list");
            }
            return closestNode;
        }*/


        /* Ellipse only. Adds nodes where the ellipse intersects its axes to keep it in shape. User can turn this off. Kepp in mind that since we can only
         * approximate the ellipse (maybe I am wrong), every node on its circumference changes its actual shape. */
        private void InsertIntermediateNodes()
        {
            List<VectorNodeStruct> newNodes = new List<VectorNodeStruct>();
            /* Originally I planned to pair every node on the ellipse to make it symmetric... Didn't work, I gave up on making it work. */
            /*foreach(VectorNodeStruct intersection in intersections.ToArray())
            {
                double angle = getAbsoluteAngle(intersection.vector);
                VectorNodeStruct newNode = new VectorNodeStruct(ellipse.VectorAtAbsoluteAngle(getConjugateAngle(angle)));
                intersections.Add( newNode );
                EllipseTool.Instance.debugDrawPositions.Add(newNode.vector);
            }*/
            newNodes.Add(new VectorNodeStruct(ellipse.VectorAtAngle(0)));
            newNodes.Add(new VectorNodeStruct(ellipse.VectorAtAngle(Math.PI / 2)));
            newNodes.Add(new VectorNodeStruct(ellipse.VectorAtAngle(Math.PI)));
            newNodes.Add(new VectorNodeStruct(ellipse.VectorAtAngle(3 * Math.PI / 2)));
            /*EllipseTool.Instance.debugDrawPositions.Add(new VectorNodeStruct(ellipse.VectorAtAngle(0)).vector);
            EllipseTool.Instance.debugDrawPositions.Add(new VectorNodeStruct(ellipse.VectorAtAngle(Math.PI / 2)).vector);
            EllipseTool.Instance.debugDrawPositions.Add(new VectorNodeStruct(ellipse.VectorAtAngle(Math.PI)).vector);
            EllipseTool.Instance.debugDrawPositions.Add(new VectorNodeStruct(ellipse.VectorAtAngle(3 * Math.PI / 2)).vector);*/
            /* Disgusting nested FOR cycle. We don't want to cluster the nodes too close to each other. */
            foreach(VectorNodeStruct vectorNode in newNodes.ToArray())
            {
                foreach(VectorNodeStruct intersection in intersections)
                {
                    if(VectorDistance(vectorNode.vector, intersection.vector) < DISTANCE_MIN)
                    {
                        newNodes.Remove(vectorNode);
                        //Debug.Log("Node too close, removing from list");
                    }
                }
            }
            intersections.AddRange(newNodes);
        }

        /*private double getConjugateAngle(double angle)
        {
            return 2 * Math.PI - angle;
        }*/

        /* See ellipse class */
        private double getAbsoluteAngle(Vector3 absPosition)
        {
            return Ellipse.VectorsAngle(absPosition - ellipse.Center);
        }

        /* For circles only. */
        private VectorNodeStruct AddIntermediateNodeCircle(ushort prevNodeID,double prevAngle)
        {
            NetNode oldNetNode = GetNode(prevNodeID);
            double angle = Ellipse.VectorsAngle(oldNetNode.m_position - ellipse.Center) + prevAngle/Math.Ceiling(prevAngle/m_maxAngDistance);

            ushort newNodeId = NetAccess.CreateNode(oldNetNode.Info, ellipse.VectorAtAbsoluteAngle(angle));

            VectorNodeStruct newNode = new VectorNodeStruct(newNodeId);

            /* Recursively repeat this action */
            FindClosestAndConnect(newNode);

            return newNode;
        }

        private void ConnectNodes(VectorNodeStruct vectorNode1, VectorNodeStruct vectorNode2, double angle)
        {
            bool invert = leftHandTraffic;

            vectorNode1.Create(centerNodeNetInfo);
            vectorNode2.Create(centerNodeNetInfo);

            /* NetNode node1 = GetNode(vectorNode1.nodeId);
             NetNode node2 = GetNode(vectorNode2.nodeId);*/

            double angle1 = getAbsoluteAngle(vectorNode1.vector);
            double angle2 = getAbsoluteAngle(vectorNode2.vector);

            Vector3 vec1 = ellipse.TangentAtAbsoluteAngle(angle1);
            Vector3 vec2 = ellipse.TangentAtAbsoluteAngle(angle2);

            vec1.Normalize();
            vec2.Normalize();
            vec2 = -vec2;

            /*EllipseTool.Instance.debugDrawVector(10*vec1, vectorNode1.vector);
            EllipseTool.Instance.debugDrawVector(10*vec2, vectorNode2.vector);*/

            //NetInfo netPrefab = PrefabCollection<NetInfo>.FindLoaded("Oneway Road");
            NetInfo netPrefab = UI.UIWindow2.instance.dropDown.Value;
            ushort newSegmentId = NetAccess.CreateSegment(vectorNode1.nodeId, vectorNode2.nodeId, vec1, vec2, netPrefab, invert, leftHandTraffic);

            /* Sometime in the future ;) */

            try
            {
                SetupTMPE(newSegmentId);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            //Debug.Log(string.Format("Building segment between nodes {0}, {1}, bezier scale {2}", node1, node2, scale));
        }

        private void SetupTMPE(ushort segment)
        {
            /* None of this below works: */
            /*bool resultPrev1 = TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.IsEnteringBlockedJunctionAllowed(segment,false);
            bool resultPrev2 = TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.IsEnteringBlockedJunctionAllowed(segment,true);
            bool result1 = TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.SetEnteringBlockedJunctionAllowed(segment, false, true);
            bool result2 = TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.SetEnteringBlockedJunctionAllowed(segment, true, true);
            bool resultPost1 = TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.IsEnteringBlockedJunctionAllowed(segment, false);
            bool resultPost2 = TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.IsEnteringBlockedJunctionAllowed(segment, true);*/
            /*Debug.Log($"Setting up tmpe segment {segment}. Result: {resultPrev1}, {resultPrev2}, {result1}, {result2}, {resultPost1}, {resultPost2}");
            ModThreading.Timer(segment);*/
            m_group.Actions.Add(new EnteringBlockedJunctionAllowedAction( segment, true) );
            m_group.Actions.Add(new EnteringBlockedJunctionAllowedAction( segment, false) );
            m_group.Actions.Add(new NoCrossingsAction(segment, true));
            m_group.Actions.Add(new NoCrossingsAction(segment, false));
            m_group.Actions.Add(new NoParkingAction(segment));
        }

        /* Please for your own sake don't look at this method! There definitely exist algorithms that compute this with efficiency
         * better than the two nested FOR cycles, but whatever. Still better than O(n!), isn't it? (The first FOR cycle is in
         * caller of this method) */
        private VectorNodeStruct getClosestNode(Vector3 vector, out double curMinDist)
        {
            VectorNodeStruct curWinner = intersections[0];
            curMinDist = 10d;
            for (int i = 0; i < intersections.Count; i++)
            {
                VectorNodeStruct onLoop = intersections[i];
                //ushort onLoopNodeID = intersections[i];
                //NetNode onLoopNode = GetNode(onLoopNodeID);
                double distance = VectorAngle(vector, ellipse.Center, onLoop.vector);
                //Debug.Log(string.Format("Angular distance between nodes {0} and {1} is {2}.",nodeID,onLoopNodeID,distance));
                if (distance < curMinDist)
                {
                    curWinner = onLoop;
                    curMinDist = distance;
                }
            }
            return curWinner;
        }

        private void recursionGuard()
        {
            pleasenoinfiniterecursion++;
            if (pleasenoinfiniterecursion > 30)
            {
                throw new Exception("Something went wrong! Mod got stuck in infinite recursion.");
            }
        }

        /* Utility */

        /* Computes angular distance of two nodes
         * Thanks to https://stackoverflow.com/a/16544330/5618300 */
        private static double VectorAngle(Vector3 v1, Vector3 center, Vector3 v3)
        {
            //if (node1.m_position == node3.m_position) return 2 * Math.PI;
            if (VectorDistance(v1, v3) < 0.01f) return (2 * Math.PI);
            Vector3 vec1 = v1 - center;
            Vector3 vec2 = v3 - center;
            float dot = vec1.x * vec2.x + vec1.z * vec2.z;
            float det = vec1.x * vec2.z - vec1.z * vec2.x;
            double angle = Math.Atan2(det, dot);
            if (angle > 0) return angle;

            return (2 * Math.PI + angle);
        }
        private static float VectorDistance(Vector3 v1, Vector3 v2) // Without Y coordinate
        {
            return (float)(Math.Sqrt(Math.Pow(v1.x - v2.x, 2) + Math.Pow(v1.z - v2.z, 2)));
        }
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
