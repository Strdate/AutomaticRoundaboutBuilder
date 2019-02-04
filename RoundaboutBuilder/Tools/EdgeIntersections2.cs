using ColossalFramework;
using ColossalFramework.Math;
using RoundaboutBuilder.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version BETA 1.2.0 */

/* This class takes the edge segments obrained by GraphTraveller2, creates a node where they intersect with the future roundabout and
 * connects that node with the outer node of the segment. */

namespace RoundaboutBuilder.Tools
{
    public class EdgeIntersections2
    {
        public static readonly int ITERATIONS = 8;
        public static readonly int MIN_BEZIER_LENGTH = 10;

        Randomizer randomizer;
        ushort CenterNodeId;
        NetNode CenterNode;
        GraphTraveller2 traveller;
        Ellipse ellipse;

        public List<VectorNodeStruct> Intersections { get; private set; } = new List<VectorNodeStruct>();
        private List<ushort> ToBeReleasedNodes = new List<ushort>();
        private List<ushort> ToBeReleasedSegments = new List<ushort>();

        public EdgeIntersections2(GraphTraveller2 traveller, ushort centerNodeId, Ellipse ellipse)
        {
            randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            CenterNodeId = centerNodeId;
            CenterNode = GetNode(centerNodeId);
            this.traveller = traveller;
            this.ellipse = ellipse;

            if(RoundAboutBuilder.UseOldSnappingAlgorithm.value)
            {
                SnappingAlgorithmOld();
            }
            else
            {
                SnappingAlgorithmNew();
            }


            /* If the list of edge nodes is empty, we add one default intersection. Legacy algorithm */
            if (Intersections.Count == 0 && ellipse.IsCircle())
            {
                Vector3 defaultIntersection = new Vector3(ellipse.RadiusMain, 0, 0) + ellipse.Center;
                CreateNode(out ushort newNodeId, CenterNode.Info, defaultIntersection);
                Intersections.Add(new VectorNodeStruct(newNodeId));
            }

            ReleaseNodesAndSegments(traveller);
        }

        private void SnappingAlgorithmNew()
        {
            //Debug
            //EllipseTool.Instance.debugDraw = segmentBeziers;

            List<Bezier2> segmentBeziers = makeBeziers(traveller.OuterSegments);
            List<Bezier2> ellipseBeziers = traveller.Ellipse.Beziers;
            List<ushort> processedSegments = new List<ushort>();

            /* We find all intersections between roads and ellipse beziers */
            for (int i = 0; i < segmentBeziers.Count; i++)
            {
                for (int j = 0; j < ellipseBeziers.Count; j++)
                {
                    if (ellipseBeziers[j].Intersect(segmentBeziers[i], out float t1, out float t2, ITERATIONS))
                    {
                        if (processedSegments.Contains(traveller.OuterSegments[i]))
                            continue;
                        else
                            processedSegments.Add(traveller.OuterSegments[i]);

                        //Debug.Log("Segment " + i.ToString() + " intersects ellipse bezier " + j.ToString());
                        Vector3 intersection = new Vector3(ellipseBeziers[j].Position(t1).x, CenterNode.m_position.y, ellipseBeziers[j].Position(t1).y);
                        segmentBeziers[i].Divide(out Bezier2 segementBezier1, out Bezier2 segementBezier2, t2);
                        Bezier2 outerBezier;
                        Vector2 outerNodePos = new Vector2(GetNode(traveller.OuterNodes[i]).m_position.x, GetNode(traveller.OuterNodes[i]).m_position.z);
                        // outerBezier - the bezier outside the ellipse (not the one inside)
                        if (segementBezier1.Position(0f) == outerNodePos || segementBezier1.Position(1f) == outerNodePos)
                        {
                            //Debug.Log("first is outer");
                            outerBezier = segementBezier1;
                        }
                        else if (segementBezier2.Position(0f) == outerNodePos || segementBezier2.Position(1f) == outerNodePos)
                        {
                            //Debug.Log("second is probably outer");
                            outerBezier = segementBezier2;
                        }
                        else
                        {
                            throw new Exception("Error - Failed to determine segment geometry.");
                        }

                        //debug:
                        //EllipseTool.Instance.debugDraw.Add(outerBezier);

                        /* We create a node at the intersection. */
                        CreateNode(out ushort newNodeId, CenterNode.Info, intersection);
                        Intersections.Add(new VectorNodeStruct(newNodeId));

                        BezierToSegment(outerBezier, traveller.OuterSegments[i], newNodeId, traveller.OuterNodes[i]);
                    }
                }
            }

        }

        /* Old algorithm. Originally intended only for circles. From older documentation: */
        /* "For now, the intersection isn't created at the exact point where the segment crosses the circle, but rather on the intersection of
         * the circle and straight line, which goes from origin and ends at outer node of that segment. That could be unfortunately very
         * inaccurate, as the first note outside the circle could be quite far away". */
        private void SnappingAlgorithmOld()
        {
            float centerX = CenterNode.m_position.x;
            float centerY = CenterNode.m_position.y;
            float centerZ = CenterNode.m_position.z;

            for (int i = 0; i < traveller.OuterNodes.Count; i++)
            {
                NetNode curNode = GetNode(traveller.OuterNodes[i]);
                Vector3 circleIntersection = new Vector3();

                float directionX = (curNode.m_position.x - centerX) / VectorDistance(CenterNode.m_position, curNode.m_position);
                float directionZ = (curNode.m_position.z - centerZ) / VectorDistance(CenterNode.m_position, curNode.m_position);

                float radius = (float)ellipse.RadiusAtAbsoluteAngle(Math.Abs(Ellipse.VectorsAngle(curNode.m_position - ellipse.Center)));
                if (radius > UI.NumericTextField.RADIUS_MAX)
                {
                    throw new Exception("Algortithm error");
                }

                circleIntersection.x = (directionX * radius + centerX);
                circleIntersection.y = centerY;
                circleIntersection.z = (directionZ * radius + centerZ);

                ushort newNodeId;
                ushort newSegmentId;

                CreateNode(out newNodeId, CenterNode.Info, circleIntersection);

                Intersections.Add(new VectorNodeStruct(newNodeId));
                //EllipseTool.Instance.debugDrawPositions.Add(Intersections.Last().vector);

                NetSegment curSegment = GetSegment(traveller.OuterSegments[i]);

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

                bool invert;
                //Debug.Log(string.Format("same node: {0}, invert: {1}", curSegment.m_startNode == traveller.OuterNodes[i], curSegment.m_flags.IsFlagSet(NetSegment.Flags.Invert)));
                if (curSegment.m_startNode == traveller.OuterNodes[i] ^ curSegment.m_flags.IsFlagSet(NetSegment.Flags.Invert))
                {
                    invert = true;
                }
                else
                {
                    invert = false;
                }


                NetManager.instance.CreateSegment(out newSegmentId, ref randomizer, curSegment.Info, newNodeId, traveller.OuterNodes[i],
            startDirection, endDirection, Singleton<SimulationManager>.instance.m_currentBuildIndex + 1,
                    Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);

                //Debug.Log(string.Format("Segment and node created... "));
            }
        }

        private void BezierToSegment(Bezier2 bezier2, ushort oldSegmentId, ushort startNodeId, ushort endNodeId)
        {
            NetSegment oldSegment = GetSegment(oldSegmentId);
            bool invert = false;
            Vector2 startDirection2d;
            Vector2 endDirection2d;
            Vector2 nodePos2d = new Vector2(GetNode(startNodeId).m_position.x, GetNode(startNodeId).m_position.z);
            if ( Distance(nodePos2d,bezier2.Position(0f)) < 10e-3d)
            {
                //0f is on the ellipse
            }
            else if(Distance(nodePos2d, bezier2.Position(1f)) < 10e-3d)
            {
                //1f is on the ellipse
                bezier2 = bezier2.Invert();
                invert = true;
            }
            else
            {
                throw new Exception(string.Format("Error - no intersection of bezier and point. Dist: {0}, {1}",Distance(nodePos2d,bezier2.Position(0f)), Distance(nodePos2d, bezier2.Position(1f))));
            }
            startDirection2d = bezier2.Tangent(0f);
            endDirection2d = bezier2.Tangent(1f);

            Vector3 startDirection = (new Vector3(startDirection2d.x, 0, startDirection2d.y));
            Vector3 endDirection = -(new Vector3(endDirection2d.x, 0, endDirection2d.y));

            /* Unlike from the old algorithm, we use no padding when looking for the segments. That means the obtained segments can be arbitrarily short.
             * In that case, we take one more segment away from the ellipse.*/
            if (VectorDistance(bezier2.a, bezier2.d) < MIN_BEZIER_LENGTH)
            {
                Debug.Log("Segment is too short. Launching repair mechainsm." + VectorDistance(bezier2.a, bezier2.d));
                if(nextSegmentInfo(endNodeId, oldSegmentId, out ushort endNodeIdNew, out Vector3 endDirectionNew))
                {
                    endNodeId = endNodeIdNew;
                    endDirection = endDirectionNew;
                    //Debug.Log("The segment length should be " + VectorDistance(GetNode(startNodeId).m_position,GetNode(endNodeId).m_position));
                    //EllipseTool.Instance.debugDrawPositions.Add(GetNode(endNodeIdNew).m_position);
                }
            }

            // Debug
            // EllipseTool.Instance.debugDrawVector(20*startDirection, GetNode(startNodeId).m_position);
            // EllipseTool.Instance.debugDrawVector(20*endDirection, GetNode(endNodeId).m_position);

            startDirection.Normalize();
            endDirection.Normalize();

            if (oldSegment.m_flags.IsFlagSet(NetSegment.Flags.Invert))
            {
                invert = !invert;
            }

            bool result = NetManager.instance.CreateSegment(out ushort newSegmentId, ref randomizer, oldSegment.Info, startNodeId, endNodeId,
            startDirection, endDirection, Singleton<SimulationManager>.instance.m_currentBuildIndex + 1,
                    Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);
            if (!result) UIWindow2.instance.ThrowErrorMsg("The game failed to create one of the road segments.");
        }

        /* Sometimes it happens that we split the road too close to another segment. If that occur, the roads do glitch. In that case 
         * we remove one more segment up the road. This method is still glitchy, would need improvement. */
         /* intersection - node outside the ellipse */
        private bool nextSegmentInfo(ushort intersection, ushort closeSegmentId, out ushort outerNodeId, out Vector3 directions)
        {
            NetSegment closeSegment = GetSegment(closeSegmentId);
            outerNodeId = 0;
            directions = new Vector3(0,0,0);
            NetNode node = GetNode(intersection);
            int segmentcount = node.CountSegments();
            /* If there is an intersection right behind the ellipse, we can't go on as we can merge only segments which are in fact
             * only one road without an intersection. */
            if (segmentcount != 2)
            {
                //Debug.Log("Ambiguous node.");
                return false;
            }
            /*string debugString = "Close segment id: " + closeSegmentId + "; ";
            for(int i = 0; i < 8; i++)
            {
                debugString += node.GetSegment(i) + ", ";
            }
            Debug.Log(debugString);*/
            ushort nextSegmentId = node.GetSegment(0);
            /* We need the segment that goes away from the ellipse, not the one we already have. */
            if(closeSegmentId == nextSegmentId)
            {
                //Debug.Log("Taking the other of the two segments. " + node.GetSegment(1));
                nextSegmentId = node.GetSegment(1);
                if (nextSegmentId == 0)
                    return false;
            }
            NetSegment nextSegment = GetSegment(nextSegmentId);
            nextSegment = GetSegment(nextSegmentId);
            outerNodeId = nextSegment.m_startNode;
            directions = nextSegment.m_startDirection;
            /* We need the node further away */
            if (outerNodeId == intersection)
            {
                //Debug.Log("Taking the other of the nodes.");
                outerNodeId = nextSegment.m_endNode;
                directions = nextSegment.m_endDirection;
                if (outerNodeId == 0)
                    return false;
            }
            /* After merging the roads, we release the segment and intersection inbetween. When I was debugging this method, I tried to release them after 
             * everything is done. It might not be necessary.*/
            ToBeReleasedSegments.Add(nextSegmentId);
            ToBeReleasedNodes.Add(intersection);
            return true;
        }

        /* Turns segments into beziers. */
        private List<Bezier2> makeBeziers(List<ushort> netSegmentsIds)
        {
            List<Bezier2> beziers = new List<Bezier2>();
            for( int i = 0; i < netSegmentsIds.Count; i++)
            {
                NetSegment netSegment = GetSegment(netSegmentsIds[i]);
                bool smoothStart = (GetNode(netSegment.m_startNode).m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                bool smoothEnd = (GetNode(netSegment.m_endNode).m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                Bezier3 bezier = new Bezier3();
                bezier.a = GetNode(netSegment.m_startNode).m_position;
                bezier.d = GetNode(netSegment.m_endNode).m_position;
                NetSegment.CalculateMiddlePoints(bezier.a, netSegment.m_startDirection, bezier.d, netSegment.m_endDirection, smoothStart, smoothEnd, out bezier.b, out bezier.c);
                beziers.Add(Bezier2.XZ(bezier));
            }
            return beziers;
        }

        private void ReleaseNodesAndSegments(GraphTraveller2 traveller)
        {
            foreach (ushort segment in traveller.InnerSegments)
            {
                ReleaseSegment(segment);
            }
            foreach (ushort segment in traveller.OuterSegments)
            {
                ReleaseSegment(segment);
            }
            foreach (ushort segment in ToBeReleasedSegments)
            {
                ReleaseSegment(segment);
            }
            foreach (ushort node in traveller.InnerNodes)
            {
                ReleaseNode(node);
            }
            foreach (ushort node in ToBeReleasedNodes)
            {
                ReleaseNode(node);
            }
        }

        public static bool ReleaseSegment(ushort id)
        {
            if(id > 0 && (NetManager.instance.m_segments.m_buffer[id].m_flags & NetSegment.Flags.Created) != NetSegment.Flags.None)
            {
                NetManager.instance.ReleaseSegment(id, true);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to release NetSegment " + id);
                return false;
            }
        }

        public static bool ReleaseNode(ushort id)
        {
            if (GetNode(id).CountSegments() > 0)
            {
                Debug.LogWarning("Failed to release NetNode " + id + ": Has segments");
                return false;
            }

            if (id > 0 && (NetManager.instance.m_nodes.m_buffer[id].m_flags & NetNode.Flags.Created) != NetNode.Flags.None)
            {
                NetManager.instance.ReleaseNode(id);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to release NetNode " + id);
                return false;
            }
        }

        public void CreateNode(out ushort nodeId,NetInfo info, Vector3 position)
        {
            bool result = NetManager.instance.CreateNode(out nodeId, ref randomizer, info, position,
                Singleton<SimulationManager>.instance.m_currentBuildIndex + 1);

            if (!result)
                throw new Exception("Failed to create NetNode at " + position.ToString());
        }

        /* Utility */
        public double Distance(Vector2 vec1,Vector2 vec2)
        {
            double distance = Math.Sqrt((vec1.x - vec2.x) * (vec1.x - vec2.x) + (vec1.y - vec2.y) * (vec1.y - vec2.y));
            return distance;
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