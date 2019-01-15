using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.0.0 */

namespace RoundaboutBuilder.Tools
{
    /* This class takes the edge segments obrained by GraphTraveller, creates a node where they intersect with the future roundabout and
     * connects that node with the outer node of the segment. */

    /* For now, the intersection isn't created at the exact point where the segment crosses the circle, but rather on the intersection of
     * the circle and straight line, which goes from origin and ends at outer node of that segment. That could be unfortunately very
     * inaccurate, as the first note outside the circle could be quite far away. */

    public class CircleIntersectionsKit
    {
        private List<ushort> circleIntersections = new List<ushort>();
        
        public CircleIntersectionsKit(ushort centerNodeID,GraphTraveller traveller, int radius)
        {
            NetNode centerNode = GetNode(centerNodeID);
            float centerX = centerNode.m_position.x;
            float centerY = centerNode.m_position.y;
            float centerZ = centerNode.m_position.z;

            var randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            bool leftHandTraffic = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic ==
                                    SimulationMetaData.MetaBool.True;

            /* Iterating over all edge nodes and segments */
            for (int i = 0; i < traveller.returnBoundaryNodesArray().Count ;i++)
            {
                NetNode curNode = GetNode(traveller.returnBoundaryNodesArray()[i]);
                Vector3 circleIntersection = new Vector3();

                float directionX = (curNode.m_position.x - centerX) / NodeDistance(centerNode, curNode);
                float directionZ = (curNode.m_position.z - centerZ) / NodeDistance(centerNode, curNode);

                circleIntersection.x = (directionX * radius + centerX);
                circleIntersection.y = centerY;
                circleIntersection.z = (directionZ * radius + centerZ);

                ushort newNodeId;
                ushort newSegmentId;

                NetManager.instance.CreateNode(out newNodeId, ref randomizer, centerNode.Info, circleIntersection,
                Singleton<SimulationManager>.instance.m_currentBuildIndex + 1);

                circleIntersections.Add(newNodeId);

                NetSegment curSegment = GetSegment(traveller.returnBoundarySegmentsArray()[i]);

                /* For now ignoring anything regarding Y coordinate */
                //float directionY2 = (GetNode(newNodeId).m_position.y - curNode.m_position.z) / NodeDistance(GetNode(newNodeId), curNode); 
                float directionY2 = 0f;

                Vector3 startDirection = new Vector3();
                startDirection.x = (directionX /** NodeDistance( GetNode( newNodeId ), curNode ) / 2*/);
                startDirection.y = directionY2;
                startDirection.z = (directionZ /** NodeDistance(GetNode(newNodeId), curNode) / 2*/);
                Vector3 endDirection = new Vector3();
                endDirection.x = -startDirection.x;
                endDirection.y = -startDirection.y;
                endDirection.z = -startDirection.z;

                /*if(curSegment.m_startNode == traveller.returnBoundaryNodesArray()[i])
                {
                    curSegment.m_endNode = newNodeId;
                    curSegment.m_startDirection = endDirection;
                    curSegment.m_endDirection = startDirection;
                }
                else
                {
                    curSegment.m_startNode = newNodeId;
                    curSegment.m_endDirection = endDirection;
                    curSegment.m_startDirection = startDirection;
                }
                NetManager.instance.UpdateSegment(traveller.returnBoundarySegmentsArray()[i]);
                NetManager.instance.UpdateSegmentRenderer(traveller.returnBoundarySegmentsArray()[i], true);*/

                /* Known bug: adjacent segments time to time are not built in the right direction. I don't know why the code
                 below doesn't work, the first part of the XOR clause gives sometimes false although it should give true.*/

                bool invert;
                //Debug.Log(string.Format("same node: {0}, invert: {1}", curSegment.m_startNode == traveller.returnBoundaryNodesArray()[i], curSegment.m_flags.IsFlagSet(NetSegment.Flags.Invert)));
                if(curSegment.m_startNode == traveller.returnBoundaryNodesArray()[i] ^ curSegment.m_flags.IsFlagSet(NetSegment.Flags.Invert))
                {
                    invert = true;
                }
                else
                {
                    invert = false;
                }
                

                NetManager.instance.CreateSegment(out newSegmentId, ref randomizer, curSegment.Info, newNodeId, traveller.returnBoundaryNodesArray()[i],
            startDirection, endDirection, Singleton<SimulationManager>.instance.m_currentBuildIndex + 1,
                    Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);

                //Debug.Log(string.Format("Segment and node created... "));
            }

            /* If the list of edge nodes is empty, we add one default intersection */
            if (circleIntersections.Count == 0)
            {
                Vector3 defaultIntersection = new Vector3(radius, 0, 0) + centerNode.m_position;
                NetManager.instance.CreateNode(out ushort newNodeId, ref randomizer, centerNode.Info, defaultIntersection,
                Singleton<SimulationManager>.instance.m_currentBuildIndex + 1);
                circleIntersections.Add(newNodeId);
            }

            /* Release all nodes and segments inside the roundabout */
            releaseNodesAndSegments(traveller);
        }

        public List<ushort> ReturnIntersectionCircles()
        {
            return circleIntersections;
        }

        private void releaseNodesAndSegments(GraphTraveller traveller)
        {
            for (int i = 0; i < traveller.returnInnerSegmentsArray().Count; i++)
            {
                NetManager.instance.ReleaseSegment(traveller.returnInnerSegmentsArray()[i],true);
            }
            for (int i = 0; i < traveller.returnBoundarySegmentsArray().Count; i++)
            {
                NetManager.instance.ReleaseSegment(traveller.returnBoundarySegmentsArray()[i], true);
            }
            for (int i = 0; i < traveller.returnInnerNodesArray().Count; i++)
            {
                NetManager.instance.ReleaseNode(traveller.returnInnerNodesArray()[i]);
            }
        }

        /* Utility */

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
