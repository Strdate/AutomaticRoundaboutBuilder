using ColossalFramework;
using System;
using System.Collections.Generic;

/* By Strad, 01/2019 */

/* Version BETA 1.0.0 */

namespace RoundaboutBuilder.Tools
{
    public class GraphTraveller
    {
        /* This class collects all segments and nodes inside the future roundabout. The edge segments and nodes are saved separately. */

        public static readonly int DISTANCE_PADDING = 15;
        public static readonly int MAX_SEGMENTS_PER_NODE = 12;

        List<ushort> outerNodes = new List<ushort>();
        List<ushort> outerSegments = new List<ushort>();
        List<ushort> visitedNodes = new List<ushort>(); // Well, not the most efficient way how to store visited nodes, but whatever...
        List<ushort> innerNodes = new List<ushort>();
        List<ushort> innerSegments = new List<ushort>();
        private ushort m_startNode;
        private int m_radius;

        /* Utility */

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
        public bool isVisited(ushort node)
        {
            return visitedNodes.Contains(node);
        }
        //this method calculates distance between two nodes
        private double NodeDistance(NetNode node1, NetNode node2)
        {
            return Math.Sqrt(Math.Pow(node1.m_position.x - node2.m_position.x,2)+ Math.Pow(node1.m_position.z - node2.m_position.z, 2));
        }
        // Returns node of 'segment' which is different from 'node'
        private ushort getOtherNode(ushort node, ushort segment)
        {
            ushort newNode = GetSegment(segment).m_startNode;
            if (node != newNode) return newNode;
            else return GetSegment(segment).m_endNode;
        }

        private bool segmentExists(ushort node)
        {
            return node != 0;
        }

        /* Main part: */

        public GraphTraveller(ushort startNode, int radius)
        {
            m_startNode = startNode;
            m_radius = radius;
            DFS(startNode);
        }

        /* Depth-first search */
        private void DFS(ushort node)
        {
            visitedNodes.Add(node);
            innerNodes.Add(node);
            NetNode netNode = GetNode(node);
            for (int i = 0; i < MAX_SEGMENTS_PER_NODE; i++)
            {
                //Debug.Log(string.Format("NodeID: {0}; segment no. {1}...",node,i));
                ushort segment = netNode.GetSegment(i);
                //Debug.Log(string.Format("ID of that segment is {0}", segment));
                if (!segmentExists(segment)) continue;

                NetSegment curSegment = GetSegment(segment);
                ushort newNode = getOtherNode(node, segment);

                if (isVisited(newNode)) continue;

                NetNode newNetNode = GetNode(newNode);
                visitedNodes.Add(newNode);

                /* Checking, whether the node is inside the roundabout or not. If not, it is added to the edge nodes and the search ends there. */
                if (NodeDistance(GetNode(m_startNode),newNetNode) > (m_radius + DISTANCE_PADDING))
                {
                    //Debug.Log(string.Format("Currently searching from node {0}, segment {1}, new node {2}. Outside radius!!",node,segment,newNode));
                    
                    outerNodes.Add(newNode);
                    outerSegments.Add(segment);
                }
                else
                {
                    /*Debug.Log(string.Format("Currently searching from node {0}, segment {1}, new node {2}. Inside radius.", node, segment, newNode));
                    Debug.Log(string.Format("nodedif: {0}, {1}, {2};direct {3}, {4}, {5}",(netNode.m_position.x- newNetNode.m_position.x), (netNode.m_position.y - newNetNode.m_position.y)
                        , (netNode.m_position.z - newNetNode.m_position.z), curSegment.m_startDirection.x, curSegment.m_startDirection.y, curSegment.m_startDirection.z));*/
                    /*Debug.Log(string.Format("nodepos1 {0},{1},{2}; nodepos1 {3},{4},{5};  segment start dir. {6},{7},{8};end dir. {9},{10},{11}; start+end dir {12},{13},{14};{15},{16},{17};{18},{19},{20}", netNode.m_position.x,
                        netNode.m_position.y, netNode.m_position.z, newNetNode.m_position.x, newNetNode.m_position.y, newNetNode.m_position.z, curSegment.m_startDirection.x,
                        curSegment.m_startDirection.y, curSegment.m_startDirection.z, curSegment.m_endDirection.x, curSegment.m_endDirection.y, curSegment.m_endDirection.z,
                        (curSegment.m_startDirection.x+curSegment.m_endDirection.x+ netNode.m_position.x), (curSegment.m_startDirection.y + curSegment.m_endDirection.y+ netNode.m_position.y), (curSegment.m_startDirection.z + curSegment.m_endDirection.z+ netNode.m_position.z),
                        (curSegment.m_startDirection.x - curSegment.m_endDirection.x + netNode.m_position.x), (curSegment.m_startDirection.y - curSegment.m_endDirection.y + netNode.m_position.y), (curSegment.m_startDirection.z - curSegment.m_endDirection.z + netNode.m_position.z),
                        (-curSegment.m_startDirection.x + curSegment.m_endDirection.x + netNode.m_position.x), (-curSegment.m_startDirection.y + curSegment.m_endDirection.y + netNode.m_position.y), (-curSegment.m_startDirection.z + curSegment.m_endDirection.z + netNode.m_position.z)
                        ));*/
                    innerNodes.Add(newNode);
                    innerSegments.Add(segment);
                    DFS(newNode);
                }

            }
        }

        // Sorry, I grew up on Java so I still cannot get used to C# way of using getters/setters. Deal with it.

        public List<ushort> returnBoundaryNodesArray()
        {
            return outerNodes;
        }

        public List<ushort> returnBoundarySegmentsArray()
        {
            return outerSegments;
        }

        public List<ushort> returnInnerNodesArray()
        {
            return innerNodes;
        }

        public List<ushort> returnInnerSegmentsArray()
        {
            return innerSegments;
        }
    }
}
