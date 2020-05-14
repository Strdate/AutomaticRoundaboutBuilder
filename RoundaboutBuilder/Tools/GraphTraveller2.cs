using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version RELEASE 1.4.0+ */

namespace RoundaboutBuilder.Tools
{
    public class GraphTraveller2
    {
        /* This class collects all segments and nodes inside the future roundabout. The edge segments and nodes are saved separately. */

        // bez paddingu

        //public static readonly int DISTANCE_PADDING = 15;
        public static readonly int MAX_SEGMENTS_PER_NODE = 8;

        public List<ushort> OuterNodes { get; private set; } = new List<ushort>(); // Nodes right behind the edge
        public List<ushort> OuterSegments { get; private set; } = new List<ushort>(); // Segments crossing the edge
        public List<ushort> InnerNodes { get; private set; } = new List<ushort>();
        public List<ushort> InnerSegments { get; private set; } = new List<ushort>();

        List<ushort> visitedNodes = new List<ushort>(); // Well, not the most efficient way how to store visited nodes, but whatever...

        private int noInfiniteRecursion;
        private static readonly int RECURSION_TRESHOLD = 300;

        private ushort m_startNodeId;
        private NetNode m_startNode;
        private Vector3 m_startNodeVector;

        public Ellipse Ellipse { get; private set; }

        /* Main part: */

        public GraphTraveller2(ushort startNodeId, Ellipse ellipse)
        {
            Ellipse = ellipse;
            DFS(startNodeId);
            noInfiniteRecursion = 0;
            m_startNodeId = startNodeId;
            m_startNode = GetNode(startNodeId);
            m_startNodeVector = m_startNode.m_position;
        }

        /* Depth-first search */
        private void DFS(ushort node)
        {
            noInfiniteRecursion++;
            if(noInfiniteRecursion > RECURSION_TRESHOLD)
            {
                UI.UIWindow.instance.ThrowErrorMsg("Error: DFS method exeeded max recursion limit");
                OuterNodes = OuterSegments = InnerNodes = InnerSegments = new List<ushort>();
                return;
            }
            visitedNodes.Add(node);
            InnerNodes.Add(node);
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
                if ( !Ellipse.IsInsideEllipse(newNetNode.m_position))
                {
                    OuterNodes.Add(newNode);
                    OuterSegments.Add(segment);
                }
                else
                {
                    InnerNodes.Add(newNode);
                    InnerSegments.Add(segment);
                    DFS(newNode);
                }

            }
        }

        

        /*private bool IsInside(Vector3 vector)
        {
            if(isCircle)
            {
                if (VectorDistance(vector, m_startNodeVector) > (m_radiusMain/* + DISTANCE_PADDING*//*)) return false;
            }
            else
            {
                if (!Ellipse.IsInsideEllipse(vector)) return false;
            }

            return true;
        }*/

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
        //this method calculates distance between two vectors
        private double VectorDistance(Vector3 vector1, Vector3 vector2)
        {
            return Math.Sqrt(Math.Pow(vector1.x - vector2.x, 2) + Math.Pow(vector1.z - vector2.z, 2));
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
    }
}
