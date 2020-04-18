using ColossalFramework;
using RoundaboutBuilder.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedEnvironment
{
    public class WrappedSegment : AbstractNetWrapper
    {
        private WrappedNode _startNode;
        public WrappedNode StartNode
        {
            get => _startNode;
            set => _startNode = IsCreated() ? throw new NetWrapperException("Cannot modify built segment") : value;
        }

        private WrappedNode _endNode;
        public WrappedNode EndNode
        {
            get => _endNode;
            set => _endNode = IsCreated() ? throw new NetWrapperException("Cannot modify built segment") : value;
        }

        private Vector3 _startDir;
        public Vector3 StartDirection
        {
            get => IsCreated() ? NetUtil.Segment(_id).m_startDirection : _startDir;
            set => _startDir = IsCreated() ? throw new NetWrapperException("Cannot modify built segment") : value;
        }

        private Vector3 _endDir;
        public Vector3 EndDirection
        {
            get => IsCreated() ? NetUtil.Segment(_id).m_endDirection : _endDir;
            set => _endDir = IsCreated() ? throw new NetWrapperException("Cannot modify built segment") : value;
        }

        private bool _invert;
        public bool Invert
        {
            get => IsCreated() ? ((NetUtil.Segment(_id).m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None) : _invert;
            set => _invert = IsCreated() ? throw new NetWrapperException("Cannot modify built segment") : value;
        }

        // confusion intensifies
        private bool _switchStartAndEnd;
        public bool SwitchStartAndEnd
        {
            get => _switchStartAndEnd;
            set => _switchStartAndEnd = IsCreated() ? throw new NetWrapperException("Cannot modify built segment") : value;
        }

        private bool _deployPlacementEffects;
        public bool DeployPlacementEffects
        {
            get => _deployPlacementEffects;
            set => _deployPlacementEffects = IsCreated() ? throw new NetWrapperException("Cannot modify built segment") : value;
        }

        private ushort _netInfoIndex;
        public NetInfo NetInfo
        {
            get => IsCreated() ? NetUtil.Segment(_id).Info : NetUtil.NetinfoFromIndex(_netInfoIndex);
            set => _netInfoIndex = IsCreated() ? throw new NetWrapperException("Cannot modify built segment") : NetUtil.NetinfoToIndex(value);
        }

        public ref NetSegment Get
        {
            get => ref NetUtil.Segment(Id);
        }

        // methods

        public override void Create()
        {
            if (!IsCreated())
            {
                _id = NetUtil.CreateSegment(_startNode.Id, _endNode.Id, _startDir, _endDir, NetUtil.NetinfoFromIndex(_netInfoIndex), _invert, _switchStartAndEnd, _deployPlacementEffects);
            }
        }

        public override bool Release()
        {
            if (IsCreated())
            {
                _startDir = NetUtil.Segment(_id).m_startDirection;
                _endDir = NetUtil.Segment(_id).m_endDirection;
                _netInfoIndex = NetUtil.Segment(_id).m_infoIndex;
                _invert = (NetUtil.Segment(_id).m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;

                NetUtil.ReleaseSegment(_id);
                if (!NetUtil.ExistsSegment(_id))
                {
                    _id = 0;
                    return true;
                }
                return false;
            }
            return true; // ?? true or false
        }

        public int ComputeConstructionCost()
        {
            // See public static ToolBase.ToolErrors NetTool.CreateNode(NetInfo info, NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint, FastList<NetTool.NodePosition> nodeBuffer, int maxSegments, bool test, bool testEnds, bool visualize, bool autoFix, bool needMoney, bool invert, bool switchDir, ushort relocateBuildingID, out ushort firstNode, out ushort lastNode, out ushort segment, out int cost, out int productionRate)
            float elevation1, elevation2;
            if (StartNode.IsCreated() && EndNode.IsCreated())
            {
                // destruction
                elevation1 = StartNode.Get.m_elevation;
                elevation2 = EndNode.Get.m_elevation;
            }
            else
            {
                // construction
                elevation1 = NetUtil.GetElevation(StartNode.Position, StartNode.NetInfo.m_netAI);
                elevation2 = NetUtil.GetElevation(EndNode.Position, EndNode.NetInfo.m_netAI);
            }
            return NetInfo.m_netAI.GetConstructionCost(StartNode.Position, EndNode.Position, elevation1, elevation2);
        }

        public override int DoCost()
        {
            return _isBuildAction ? ComputeConstructionCost() : - ComputeConstructionCost() * 3 / 4;
        }

        public override int UndoCost()
        {
            return _isBuildAction ? - ComputeConstructionCost() : ComputeConstructionCost() * 3 / 4;
        }

        // Constructors

        public WrappedSegment() { }

        public WrappedSegment(WrappedNode startNode, WrappedNode endNode, ushort id)
        {
            if (id != 0 && !NetUtil.ExistsSegment(id))
            {
                throw new NetWrapperException("Cannot wrap nonexisting segment");
            }

            if(NetUtil.Segment(id).m_startNode != startNode.Id || NetUtil.Segment(id).m_endNode != endNode.Id)
            {
                throw new NetWrapperException("Cannot wrap segment - Nodes do not match");
            }

            _startNode = startNode;
            _endNode = endNode;

            _switchStartAndEnd = false;
            _deployPlacementEffects = false;

            _id = id;
        }
    }
}