using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RoundaboutBuilder.Tools
{
    public static class NetAccess
    {
        public static bool ReleaseSegment(ushort id)
        {
            if (id > 0 && (NetManager.instance.m_segments.m_buffer[id].m_flags & NetSegment.Flags.Created) != NetSegment.Flags.None)
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

        public static ushort CreateNode(NetInfo info, Vector3 position)
        {
            var randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            bool result = NetManager.instance.CreateNode(out ushort nodeId, ref randomizer, info, position,
                Singleton<SimulationManager>.instance.m_currentBuildIndex + 1);

            if (!result)
                throw new Exception("Failed to create NetNode at " + position.ToString());

            return nodeId;
        }

        public static ushort CreateSegment(ushort startNodeId, ushort endNodeId, Vector3 startDirection, Vector3 endDirection, NetInfo netInfo, bool invert = false, bool switchStartAndEnd = false)
        {
            var randomizer = Singleton<SimulationManager>.instance.m_randomizer;

            if ((GetNode(startNodeId).m_flags & NetNode.Flags.Created) == NetNode.Flags.None || (GetNode(endNodeId).m_flags & NetNode.Flags.Created) == NetNode.Flags.None)
                throw new Exception("Failed to create NetSegment: Invalid node(s)");

            var result = NetManager.instance.CreateSegment(out ushort newSegmentId, ref randomizer, netInfo, switchStartAndEnd ? endNodeId : startNodeId,
                 switchStartAndEnd ? startNodeId : endNodeId,
                 (switchStartAndEnd ? endDirection : startDirection), (switchStartAndEnd ? startDirection : endDirection), Singleton<SimulationManager>.instance.m_currentBuildIndex + 1,
                         Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);

            if (!result)
                throw new Exception("Failed to create NetSegment");

            return newSegmentId;
        }

        /* As always */

        public static NetManager Manager
        {
            get { return Singleton<NetManager>.instance; }
        }
        public static ref NetNode GetNode(ushort id)
        {
            return ref Manager.m_nodes.m_buffer[id];
        }
        public static ref NetSegment GetSegment(ushort id)
        {
            return ref Manager.m_segments.m_buffer[id];
        }
    }
}
