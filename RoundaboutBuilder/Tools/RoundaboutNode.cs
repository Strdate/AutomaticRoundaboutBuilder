using ColossalFramework;
using ColossalFramework.Math;
using SharedEnvironment;
using System;
using UnityEngine;

/* By Strad, 01/2019, 10/219 */

namespace RoundaboutBuilder.Tools
{
    /* In the end not a struct. Whatever. */
    public class RoundaboutNode : IComparable<RoundaboutNode>
    {
        public WrappedNode wrappedNode;
        public double angle = 0;
        public GameActionExtended action;

        public void Create(ActionGroup group, NetInfo info = null)
        {
            if(action == null)
            {
                if(info != null)
                {
                    wrappedNode.NetInfo = info;
                }
                group.Actions.Add(wrappedNode);
            }
        }

        /* Constructors */

        public RoundaboutNode(Vector3 vector)
        {
            wrappedNode = new WrappedNode();
            wrappedNode.Position = vector;
        }

        public RoundaboutNode(WrappedNode wrappedNode)
        {
            this.wrappedNode = wrappedNode;
        }

        /* Utility */

        public int CompareTo(RoundaboutNode other)
        {
            if (angle - other.angle < 0.0001f)
                return 0;
            else if (angle > other.angle)
                return -1;

            return 1;
        }

    }
}
