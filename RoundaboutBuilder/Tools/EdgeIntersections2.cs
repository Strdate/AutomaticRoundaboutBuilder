using ColossalFramework;
using ColossalFramework.Math;
using SharedEnvironment;
using System;
using System.Collections.Generic;
using UnityEngine;

/* By Strad, 01/2019 */

/* Version RELEASE 1.4.0+ */

/* This class takes the edge segments obrained by GraphTraveller2, creates a node where they intersect with the future roundabout and
 * connects that node with the outer node of the segment. */

namespace RoundaboutBuilder.Tools
{
    public class EdgeIntersections2
    {
        public static readonly int ITERATIONS = 8;
        public static readonly int MIN_BEZIER_LENGTH = 16;

        private ushort CenterNodeId;
        private NetNode CenterNode;
        private GraphTraveller2 traveller;
        private Ellipse ellipse;
        private bool m_followTerrain;

        public ActionGroup ActionGroupTMPE { get; private set; } = new ActionGroup("TMPE action group");
        public ActionGroup ActionGroupRoads { get; private set; } = new ActionGroup("Build roundabout");

        public List<RoundaboutNode> Intersections { get; private set; } = new List<RoundaboutNode>();
        
        private List<WrappedNode> ToBeReleasedNodes = new List<WrappedNode>();
        private List<WrappedSegment> ToBeReleasedSegments = new List<WrappedSegment>();

        public WrappersDictionary networkDictionary = new WrappersDictionary();

        public EdgeIntersections2(GraphTraveller2 traveller, ushort centerNodeId, Ellipse ellipse, bool followTerrain)
        {
            CenterNodeId = centerNodeId;
            CenterNode = NetUtil.Node(centerNodeId);
            this.traveller = traveller;
            this.ellipse = ellipse;
            m_followTerrain = followTerrain;

            if(RoundAboutBuilder.UseOldSnappingAlgorithm.value)
            {
                SnappingAlgorithmOld();
            }
            else
            {
                SnappingAlgorithmNew();
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
                        Vector2 outerNodePos = new Vector2(NetUtil.Node(traveller.OuterNodes[i]).m_position.x, NetUtil.Node(traveller.OuterNodes[i]).m_position.z);
                        bool invert = false;
                        // outerBezier - the bezier outside the ellipse (not the one inside)
                        if (segementBezier1.Position(0f) == outerNodePos || segementBezier1.Position(1f) == outerNodePos)
                        {
                            //Debug.Log("first is outer");
                            outerBezier = segementBezier1.Invert();
                            invert = true;
                        }
                        else if (segementBezier2.Position(0f) == outerNodePos || segementBezier2.Position(1f) == outerNodePos)
                        {
                            //Debug.Log("second is probably outer");
                            outerBezier = segementBezier2;
                            invert = false;
                        }
                        else
                        {
                            throw new Exception("Error - Failed to determine segment geometry.");
                        }

                        //debug:
                        //EllipseTool.Instance.debugDraw.Add(outerBezier);

                        /* We create a node at the intersection. */
                        WrappedNode newNode = new WrappedNode();
                        newNode.Position = new Vector3(intersection.x, m_followTerrain ? NetUtil.TerrainHeight(intersection) : intersection.y, intersection.z);
                        newNode.NetInfo = CenterNode.Info;
                        RoundaboutNode raNode = new RoundaboutNode(newNode);
                        raNode.Create(ActionGroupRoads);
                        Intersections.Add(raNode);

                        WrappedNode outerNode = networkDictionary.RegisterNode(traveller.OuterNodes[i]);
                        WrappedSegment outerSegment = networkDictionary.RegisterSegment(traveller.OuterSegments[i]);

                        BezierToSegment(outerBezier, outerSegment, newNode, outerNode, invert);
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
                NetNode curNode = NetUtil.Node(traveller.OuterNodes[i]);
                Vector3 circleIntersection = new Vector3();

                float directionX = (curNode.m_position.x - centerX) / VectorDistance(CenterNode.m_position, curNode.m_position);
                float directionZ = (curNode.m_position.z - centerZ) / VectorDistance(CenterNode.m_position, curNode.m_position);

                float radius = (float)ellipse.RadiusAtAbsoluteAngle(Math.Abs(Ellipse.VectorsAngle(curNode.m_position - ellipse.Center)));
                if (radius > 10000)
                {
                    throw new Exception("Algortithm error");
                }

                circleIntersection.x = (directionX * radius + centerX);
                circleIntersection.y = centerY;
                circleIntersection.z = (directionZ * radius + centerZ);

                WrappedNode newNode;
                WrappedSegment newSegment;

                newNode = new WrappedNode();
                newNode.NetInfo = CenterNode.Info;
                newNode.Position = circleIntersection;
                newNode.Position = new Vector3(circleIntersection.x, m_followTerrain ? NetUtil.TerrainHeight(circleIntersection) : circleIntersection.y, circleIntersection.z);
                RoundaboutNode raNode = new RoundaboutNode(newNode);
                raNode.Create(ActionGroupRoads);
                Intersections.Add(raNode);
                //EllipseTool.Instance.debugDrawPositions.Add(Intersections.Last().vector);

                NetSegment curSegment = NetUtil.Segment(traveller.OuterSegments[i]);

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

                newSegment = new WrappedSegment();
                newSegment.StartNode = newNode;
                newSegment.EndNode = networkDictionary.RegisterNode(traveller.OuterNodes[i]);
                newSegment.StartDirection = startDirection;
                newSegment.EndDirection = endDirection;
                newSegment.NetInfo = curSegment.Info;
                newSegment.Invert = invert;
                ActionGroupRoads.Actions.Add(newSegment);

                ActionGroupTMPE.Actions.Add(new EnteringBlockedJunctionAllowedAction(newSegment, true, true));
                ActionGroupTMPE.Actions.Add(new YieldSignAction(newSegment, true));
                //Debug.Log(string.Format("Segment and node created... "));
            }
        }

        private void BezierToSegment(Bezier2 bezier2, WrappedSegment oldSegmentW, WrappedNode startNodeW, WrappedNode endNodeW, bool invert)
        {
            NetSegment oldSegment = oldSegmentW.Get;
            Vector2 startDirection2d;
            Vector2 endDirection2d;
            Vector2 nodePos2d = new Vector2(startNodeW.Position.x, startNodeW.Position.z);
            
            startDirection2d = bezier2.Tangent(0f);
            endDirection2d = bezier2.Tangent(1f);

            Vector3 startDirection = (new Vector3(startDirection2d.x, 0, startDirection2d.y));
            Vector3 endDirection = -(new Vector3(endDirection2d.x, 0, endDirection2d.y));

            /* Unlike from the old algorithm, we use no padding when looking for the segments. That means the obtained segments can be arbitrarily short.
             * In that case, we take one more segment away from the ellipse.*/
            if (VectorDistance(bezier2.a, bezier2.d) < MIN_BEZIER_LENGTH)
            {
                //Debug.Log("Segment is too short. Launching repair mechainsm." + VectorDistance(bezier2.a, bezier2.d));
                nextSegmentInfo(oldSegmentW, ref endNodeW, ref endDirection);
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

            WrappedSegment newSegment = new WrappedSegment();
            newSegment.StartNode = startNodeW;
            newSegment.EndNode = endNodeW;
            newSegment.StartDirection = startDirection;
            newSegment.EndDirection = endDirection;
            newSegment.NetInfo = oldSegment.Info;
            newSegment.Invert = invert;

            ActionGroupRoads.Actions.Add(newSegment);
            ActionGroupTMPE.Actions.Add(new EnteringBlockedJunctionAllowedAction(newSegment, true, true));
            ActionGroupTMPE.Actions.Add(new YieldSignAction(newSegment, true));

            /*try
            {
                ushort newSegmentId = NetAccess.CreateSegment(startNodeId, endNodeId,
            startDirection, endDirection, oldSegment.Info, invert);

                
            }
            catch(Exception e)
            {
                UIWindow2.instance.ThrowErrorMsg("The game failed to create one of the road segments.");
                Debug.LogError(e.ToString());
            }*/            
        }

        /* Sometimes it happens that we split the road too close to another segment. If that occur, the roads do glitch. In that case 
         * we remove one more segment up the road. This method is still glitchy, would need improvement. */
         /* intersection - node outside the ellipse */
        private bool nextSegmentInfo(WrappedSegment closeSegmentW, ref WrappedNode outerNodeW, ref Vector3 endDirection)
        {
            NetSegment closeSegment = closeSegmentW.Get;
            //outerNodeId = 0;
            //directions = new Vector3(0,0,0);
            NetNode node = outerNodeW.Get;
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
            ushort nextSegmentId = NetUtil.GetNonzeroSegment(node, 0);
            /* We need the segment that goes away from the ellipse, not the one we already have. */
            if(closeSegmentW.Id == nextSegmentId)
            {
                //Debug.Log("Taking the other of the two segments. " + node.GetSegment(1));
                nextSegmentId = NetUtil.GetNonzeroSegment(node, 1);
                if (nextSegmentId == 0)
                    return false;
            }
            NetSegment nextSegment = NetUtil.Segment(nextSegmentId);

            ushort outerNodeId = nextSegment.m_startNode;
            Vector3 directions = nextSegment.m_startDirection;
            /* We need the node further away */
            if (outerNodeId == outerNodeW.Id)
            {
                //Debug.Log("Taking the other of the nodes.");
                outerNodeId = nextSegment.m_endNode;
                directions = nextSegment.m_endDirection;
                if (outerNodeId == 0)
                    return false;
            }

            WrappedSegment nextSegmentW = networkDictionary.RegisterSegment(nextSegmentId);

            // Release old
            ToBeReleasedNodes.Add(outerNodeW);
            ToBeReleasedSegments.Add(nextSegmentW);

            // Return values
            outerNodeW = networkDictionary.RegisterNode(outerNodeId);
            endDirection = directions;

            /* After merging the roads, we release the segment and intersection inbetween. When I was debugging this method, I tried to release them after 
             * everything is done. It might not be necessary.*/
            return true;
        }

        /* Turns segments into beziers. */
        private List<Bezier2> makeBeziers(List<ushort> netSegmentsIds)
        {
            List<Bezier2> beziers = new List<Bezier2>();
            for( int i = 0; i < netSegmentsIds.Count; i++)
            {
                NetSegment netSegment = NetUtil.Segment(netSegmentsIds[i]);
                bool smoothStart = (NetUtil.Node(netSegment.m_startNode).m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                bool smoothEnd = (NetUtil.Node(netSegment.m_endNode).m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                Bezier3 bezier = new Bezier3();
                bezier.a = NetUtil.Node(netSegment.m_startNode).m_position;
                bezier.d = NetUtil.Node(netSegment.m_endNode).m_position;
                NetSegment.CalculateMiddlePoints(bezier.a, netSegment.m_startDirection, bezier.d, netSegment.m_endDirection, smoothStart, smoothEnd, out bezier.b, out bezier.c);
                beziers.Add(Bezier2.XZ(bezier));
            }
            return beziers;
        }

        private void ReleaseNodesAndSegments(GraphTraveller2 traveller)
        {
            foreach (ushort segment in traveller.InnerSegments)
            {
                AbstractNetWrapper wrapped = networkDictionary.RegisterSegment(segment);
                wrapped.IsBuildAction = false;
                ActionGroupRoads.Actions.Add(wrapped);
            }
            foreach (ushort segment in traveller.OuterSegments)
            {
                AbstractNetWrapper wrapped = networkDictionary.RegisterSegment(segment);
                wrapped.IsBuildAction = false;
                ActionGroupRoads.Actions.Add(wrapped);
            }
            foreach (WrappedSegment segment in ToBeReleasedSegments)
            {
                segment.IsBuildAction = false;
                ActionGroupRoads.Actions.Add(segment);
            }
            foreach (ushort node in traveller.InnerNodes)
            {
                AbstractNetWrapper wrapped = networkDictionary.RegisterNode(node);
                wrapped.IsBuildAction = false;
                ActionGroupRoads.Actions.Add(wrapped);
            }
            foreach (WrappedNode node in ToBeReleasedNodes)
            {
                node.IsBuildAction = false;
                ActionGroupRoads.Actions.Add(node);
            }
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
    }
}