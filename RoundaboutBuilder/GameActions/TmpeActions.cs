using RoundaboutBuilder;
using RoundaboutBuilder.UI;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SharedEnvironment
{
    public class EnteringBlockedJunctionAllowedAction : GameActionExtended
    {
        private WrappedSegment m_segment;
        private bool m_startNode;

        // Is it the main roundabout road or road entering the roundabout
        private bool m_yieldingRoad;

        public EnteringBlockedJunctionAllowedAction(WrappedSegment segment, bool startNode, bool yieldingRoad) : base("TMPE setup", false)
        {
            m_segment = segment;
            m_startNode = startNode;
            m_yieldingRoad = yieldingRoad;
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
            TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.SetEnteringBlockedJunctionAllowed(m_segment.Id, m_startNode, true);
        }

        protected bool IsPolicyAllowed()
        {
            return ModLoadingExtension.tmpeDetected && UIWindow.SavedSetupTmpe && (
                (!m_yieldingRoad && TmpeSetupPanel.SavedEnterBlockedMainRoad.value) || (m_yieldingRoad && TmpeSetupPanel.SavedEnterBlockedYieldingRoad.value));
        }

    }

    public class NoCrossingsAction : GameActionExtended
    {
        private WrappedSegment m_segment;
        private bool m_startNode;

        public NoCrossingsAction(WrappedSegment segment, bool startNode) : base("TMPE setup", false)
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
            TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.SetPedestrianCrossingAllowed(m_segment.Id, m_startNode, false);
        }

        protected bool IsPolicyAllowed()
        {
            return ModLoadingExtension.tmpeDetected && UIWindow.SavedSetupTmpe && TmpeSetupPanel.SavedNoCrossings;
        }

    }

    public class NoParkingAction : GameActionExtended
    {
        private WrappedSegment m_segment;

        public NoParkingAction(WrappedSegment segment) : base("TMPE setup", false)
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
            TrafficManager.Manager.Impl.ParkingRestrictionsManager.Instance.SetParkingAllowed(m_segment.Id, NetInfo.Direction.Backward, false);
            TrafficManager.Manager.Impl.ParkingRestrictionsManager.Instance.SetParkingAllowed(m_segment.Id, NetInfo.Direction.Forward, false);
        }

        protected bool IsPolicyAllowed()
        {
            return ModLoadingExtension.tmpeDetected && UIWindow.SavedSetupTmpe && TmpeSetupPanel.SavedNoParking;
        }

    }

    public class YieldSignAction : GameActionExtended
    {
        private WrappedSegment m_segment;
        private bool m_startNode;

        public YieldSignAction(WrappedSegment segment, bool startNode) : base("TMPE setup", false)
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

        protected void Implementation() {
            Exception ex = new Exception();
            bool invoked = false;
            try
            {
                var tmpeTool = Type.GetType("TrafficManager.UI.ModUI, TrafficManager").GetMethod("GetTrafficManagerTool", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Invoke(null, new object[] { true });
                var prioritySignsTool = Type.GetType("TrafficManager.UI.TrafficManagerTool, TrafficManager").GetMethod("GetSubTool", BindingFlags.Public | BindingFlags.Instance).Invoke(tmpeTool, new object[] { 2 });
                var setPrioritySign = prioritySignsTool.GetType().GetMethod("SetPrioritySign", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                setPrioritySign.Invoke(prioritySignsTool, new object[] { m_segment.Id, m_startNode, 3 });
                invoked = true;
            } catch(Exception e) { ex = e; }
            
            if(!invoked)
                try
                {
                    Implementation_preV11();
                    invoked = true;
                } catch { }

            if (!invoked)
                throw ex;
            
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Implementation_preV11()
        {
            /*var prioritySignsTool = (TrafficManager.UI.SubTools.PrioritySignsTool)TrafficManager.UI.UIBase.GetTrafficManagerTool().GetSubTool(TrafficManager.UI.ToolMode.AddPrioritySigns);
            var setPrioritySign = prioritySignsTool.GetType().GetMethod("SetPrioritySign", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            setPrioritySign.Invoke(prioritySignsTool, new object[] { m_segment.Id, m_startNode, 3 });*/
        }

        protected bool IsPolicyAllowed()
        {
            return ModLoadingExtension.tmpeDetected && UIWindow.SavedSetupTmpe && TmpeSetupPanel.SavedPrioritySigns;
        }

    }

    public class LaneChangingAction : GameActionExtended
    {
        private WrappedSegment m_segment;
        private bool m_startNode;

        public LaneChangingAction(WrappedSegment segment, bool startNode) : base("TMPE setup", false)
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
            //TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.SetLaneChangingAllowedWhenGoingStraight(m_segment.Id, m_startNode, true);
        }

        protected bool IsPolicyAllowed()
        {
            return ModLoadingExtension.tmpeDetected && UIWindow.SavedSetupTmpe && TmpeSetupPanel.SavedAllowLaneChanging;
        }

    }
}
