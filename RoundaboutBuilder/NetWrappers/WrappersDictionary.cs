using RoundaboutBuilder.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public class WrappersDictionary
    {
        public Dictionary<ushort, WrappedNode> RegisteredNodes { get; private set; } = new Dictionary<ushort, WrappedNode>();

        public Dictionary<ushort, WrappedSegment> RegisteredSegments { get; private set; } = new Dictionary<ushort, WrappedSegment>();

        public WrappedNode RegisterNode(ushort id)
        {
            if (id == 0)
                return null;

            WrappedNode node;
            if(!RegisteredNodes.TryGetValue(id,out node))
            {
                node = new WrappedNode(id);
                RegisteredNodes[id] = node;
            }

            return node;
        }

        public WrappedSegment RegisterSegment(ushort id)
        {
            if (id == 0)
                return null;

            WrappedSegment segment;
            if (!RegisteredSegments.TryGetValue(id, out segment))
            {
                var node1 = RegisterNode(NetUtil.Segment(id).m_startNode);
                var node2 = RegisterNode(NetUtil.Segment(id).m_endNode);
                segment = new WrappedSegment(node1, node2, id);
                RegisteredSegments[id] = segment;
            }

            return segment;
        }

    }
}
