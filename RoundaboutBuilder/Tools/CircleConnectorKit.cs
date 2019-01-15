using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.0.0 */

namespace RoundaboutBuilder.Tools
{
    /* This class takes the nodes obtained from CircleIntersectionsKit and connects them together to create the final roundabout. */

    /* Warning! This is not the most efficient algorithm on Earth, as you will see... */

    public class CircleConnectorKit
    {
        private static readonly double MAX_ANGULAR_DISTANCE = Math.PI / 2d + 0.1d; // Maximal angular distance between two nodes on the circle
        private static readonly double INTERMEDIATE_NODE_DISTANCE = Math.PI / 3d;
        private List<ushort> intersections;
        private ushort centerNodeID;
        private bool leftHandTraffic;

        // Time to time something goes wrong. Let's make sure that we don't get stuck in infinite recursion.
        // Didn't happen to me since I degbugged it, but one never knows for sure.
        private int pleasenoinfiniterecursion;

        Randomizer randomizer;

        public CircleConnectorKit(ushort centerNodeID,List<ushort> intersections)
        {
            pleasenoinfiniterecursion = 0;
            this.intersections = intersections;
            this.centerNodeID = centerNodeID;
            leftHandTraffic = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic ==
                                    SimulationMetaData.MetaBool.True;
            randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            NetNode node = GetNode(centerNodeID);

            /* Goes over all the nodes and conntets each of them to the angulary closest neighbour. (In a given direction) */
            for( int i = 0; i < intersections.Count; i++)
            {
                FindClosestAndConnect(intersections[i]);
            }
        }

        private void FindClosestAndConnect(ushort nodeID)
        {
            pleasenoinfiniterecursion++;
            if (pleasenoinfiniterecursion > 10)
            {
                UIWindow.Instance.ThrowErrorMsg("Something went wrong! Mod got stuck in infinite recursion.");
                return;
            }
            NetNode closestNode = getClosestNode(nodeID, out ushort closestNodeID, out double angDistance);
            //Debug.Log(string.Format("Found closest node {0}, angdistance {1} > {2} max ang distance (node {3})", nodeID, angDistance, MAX_ANGULAR_DISTANCE,nodeID));
            /* If the distance between two nodes is too great, we put in an intermediate node inbetween */
            if (angDistance > MAX_ANGULAR_DISTANCE)
            {
                closestNodeID = AddIntermediateNode(nodeID);
            }
            ConnectNodes(nodeID, closestNodeID, angDistance);
        }

        private ushort AddIntermediateNode(ushort prevNodeID)
        {
            NetNode oldNetNode = GetNode(prevNodeID);
            Vector3 pivot = GetNode(centerNodeID).m_position;

            NetManager.instance.CreateNode(out ushort newNodeId, ref randomizer, oldNetNode.Info, RotatePoint(pivot,oldNetNode.m_position,INTERMEDIATE_NODE_DISTANCE),
                Singleton<SimulationManager>.instance.m_currentBuildIndex + 1);

            /* Recursively repeat this action */
            FindClosestAndConnect(newNodeId);

            return newNodeId;
        }

        private void ConnectNodes(ushort node1, ushort node2, double angle)
        {
            Vector3 vec1;
            Vector3 vec2;
            Vector3 startDirection;
            Vector3 endDirection;

            // Computing direction vectors. See: https://stackoverflow.com/questions/1734745/how-to-create-circle-with-b%C3%A9zier-curves

            // In the end I just ignored the scale besause I didn't get good results.
            //float scale = (float)((4 / 3) * Math.Tan(1 / angle));
            float scale = 1;
            if (leftHandTraffic)
            {
                vec1 = Vector3.Normalize(GetNode(node2).m_position - GetNode(centerNodeID).m_position);
                vec2 = Vector3.Normalize(GetNode(node1).m_position - GetNode(centerNodeID).m_position);
                startDirection = new Vector3(vec1.z * scale, 0, -vec1.x * scale);
                endDirection = new Vector3(-vec2.z * scale, 0, vec2.x * scale);
            }
            else
            {
                vec1 = Vector3.Normalize(GetNode(node1).m_position - GetNode(centerNodeID).m_position);
                vec2 = Vector3.Normalize(GetNode(node2).m_position - GetNode(centerNodeID).m_position);
                startDirection = new Vector3(-vec1.z * scale, 0, vec1.x * scale);
                endDirection = new Vector3(vec2.z * scale, 0, -vec2.x * scale);
            }

            NetInfo netPrefab = PrefabCollection<NetInfo>.FindLoaded("Oneway Road");
            var result = NetManager.instance.CreateSegment(out ushort newSegmentId, ref randomizer, netPrefab, (leftHandTraffic ? node2 : node1),
                (leftHandTraffic ? node1 : node2),
                startDirection, endDirection, Singleton<SimulationManager>.instance.m_currentBuildIndex + 1,
                        Singleton<SimulationManager>.instance.m_currentBuildIndex, leftHandTraffic);

            //Debug.Log(string.Format("Building segment between nodes {0}, {1}, bezier scale {2}", node1, node2, scale));
        }

        /* Please for your own sake don't look at this method! There definitely exist algorithms that compute this with efficiency
         * better than the two nested FOR cycles, but whatever. Still better than O(n!), isn't it? (The first FOR cycle is in
         * caller of this method) */
        private NetNode getClosestNode(ushort nodeID, out ushort winnerID, out double curMinDist)
        {
            winnerID = 0;
            NetNode thisNode = GetNode(nodeID);
            NetNode curWinner = GetNode(intersections[0]);
            NetNode centralNode = GetNode(centerNodeID);
            curMinDist = 10d;
            for (int i = 0; i < intersections.Count; i++)
            {
                ushort onLoopNodeID = intersections[i];
                NetNode onLoopNode = GetNode(onLoopNodeID);
                double distance = NodesAngle(thisNode, centralNode ,onLoopNode);
                //Debug.Log(string.Format("Angular distance between nodes {0} and {1} is {2}.",nodeID,onLoopNodeID,distance));
                if ( distance < curMinDist )
                {
                    curWinner = onLoopNode;
                    winnerID = onLoopNodeID;
                    curMinDist = distance;
                }
            }
            return curWinner;
        }

        /* Utility */

        private Vector3 RotatePoint(Vector3 pivot, Vector3 origPoint, double angle)
        {
            Vector3 difference = origPoint - pivot;

            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            Vector3 newPoint = new Vector3((float)(difference.x * cos - difference.z * sin + pivot.x), pivot.y, (float)(difference.x * sin + difference.z * cos + pivot.z));

            return newPoint;
        }
        /* Computes angular distance of two nodes
         * Thanks to https://stackoverflow.com/a/16544330/5618300 */
        private double NodesAngle(NetNode node1, NetNode centerNode, NetNode node3)
        {
            //if (node1.m_position == node3.m_position) return 2 * Math.PI;
            if(NodeDistance(node1,node3)<0.01f) return (2 * Math.PI);
            Vector3 vec1 = node1.m_position - centerNode.m_position;
            Vector3 vec2 = node3.m_position - centerNode.m_position;
            float dot = vec1.x * vec2.x + vec1.z * vec2.z;
            float det = vec1.x * vec2.z - vec1.z * vec2.x;
            double angle = Math.Atan2(det, dot);
            if (angle > 0) return angle;

            return (2*Math.PI + angle);
        }
        private float NodeDistance(NetNode node1, NetNode node2) // Without Y coordinate
        {
            return (float)(Math.Sqrt(Math.Pow(node1.m_position.x - node2.m_position.x, 2) + Math.Pow(node1.m_position.z - node2.m_position.z, 2)));
        }
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
    }

}
