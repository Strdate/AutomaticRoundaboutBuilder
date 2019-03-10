
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Provisional.Actions;
using RoundaboutBuilder;
using RoundaboutBuilder.UI;

namespace SharedEnvironment.Public.Actions
{
    public class EnteringBlockedJunctionAllowedAction : Provisional.Actions.Action
    {
        private ushort m_segment;
        private bool m_startNode;

        public EnteringBlockedJunctionAllowedAction(ushort segment, bool startNode) : base("TMPE setup", false)
        {
            m_segment = segment;
            m_startNode = startNode;
        }

        protected override void DoImplementation()
        {
            if (IsPolicyAllowed())
                Implementation();
        }

        protected override void RedoImplementation()
        {
            if (IsPolicyAllowed())
                Implementation();
        }

        protected override void UndoImplementation()
        {
            // ignore
        }

        protected void Implementation()
        {
            TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.SetEnteringBlockedJunctionAllowed(m_segment, m_startNode, true);
        }

        protected bool IsPolicyAllowed()
        {
            return ModLoadingExtension.tmpeDetected && UIWindow2.SavedSetupTmpe && TmpeSetupPanel.SavedEnterBlockedJunction;
        }

    }

    public class NoCrossingsAction : Provisional.Actions.Action
    {
        private ushort m_segment;
        private bool m_startNode;

        public NoCrossingsAction(ushort segment, bool startNode) : base("TMPE setup", false)
        {
            m_segment = segment;
            m_startNode = startNode;
        }

        protected override void DoImplementation()
        {
            if (IsPolicyAllowed())
                Implementation();
        }

        protected override void RedoImplementation()
        {
            if (IsPolicyAllowed())
                Implementation();
        }

        protected override void UndoImplementation()
        {
            // ignore
        }

        protected void Implementation()
        {
            TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.SetPedestrianCrossingAllowed(m_segment, m_startNode, false);
        }

        protected bool IsPolicyAllowed()
        {
            return ModLoadingExtension.tmpeDetected && UIWindow2.SavedSetupTmpe && TmpeSetupPanel.SavedNoCrossings;
        }

    }

    public class NoParkingAction : Provisional.Actions.Action
    {
        private ushort m_segment;

        public NoParkingAction(ushort segment) : base("TMPE setup", false)
        {
            m_segment = segment;
        }

        protected override void DoImplementation()
        {
            if (IsPolicyAllowed())
                Implementation();
        }

        protected override void RedoImplementation()
        {
            if (IsPolicyAllowed())
                Implementation();
        }

        protected override void UndoImplementation()
        {
            // ignore
        }

        protected void Implementation()
        {
            TrafficManager.Manager.Impl.ParkingRestrictionsManager.Instance.SetParkingAllowed(m_segment, NetInfo.Direction.Backward, false);
            TrafficManager.Manager.Impl.ParkingRestrictionsManager.Instance.SetParkingAllowed(m_segment, NetInfo.Direction.Forward, false);
        }

        protected bool IsPolicyAllowed()
        {
            return ModLoadingExtension.tmpeDetected && UIWindow2.SavedSetupTmpe && TmpeSetupPanel.SavedNoParking;
        }

    }

    public class YieldSignAction : Provisional.Actions.Action
    {
        private ushort m_segment;
        private bool m_startNode;

        public YieldSignAction(ushort segment, bool startNode) : base("TMPE setup", false)
        {
            m_segment = segment;
            m_startNode = startNode;
        }

        protected override void DoImplementation()
        {
            if (IsPolicyAllowed())
                Implementation();
        }

        protected override void RedoImplementation()
        {
            if (IsPolicyAllowed())
                Implementation();
        }

        protected override void UndoImplementation()
        {
            // ignore
        }

        protected void Implementation()
        {
            ((TrafficManager.UI.SubTools.PrioritySignsTool)TrafficManager.UI.UIBase.GetTrafficManagerTool().GetSubTool(TrafficManager.UI.ToolMode.AddPrioritySigns)).SetPrioritySign(m_segment, m_startNode, TrafficManager.Traffic.Data.PrioritySegment.PriorityType.Yield);
        }

        protected bool IsPolicyAllowed()
        {
            return ModLoadingExtension.tmpeDetected && UIWindow2.SavedSetupTmpe && TmpeSetupPanel.SavedPrioritySigns;
        }

    }
}
