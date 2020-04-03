using ColossalFramework;
using RoundaboutBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SharedEnvironment
{
    public class ActionGroup : GameActionExtended
    {
        public List<GameAction> Actions = new List<GameAction>();
        public ItemClass ItemClass { get; set; }

        public ActionGroup(string name, bool redoable=false) : base(name)
        {
        }

        protected override void DoImplementation()
        {
            ChargePlayer();

            foreach(GameAction action in Actions)
            {
                try
                {
                    action.Do();
                } catch(Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        protected override void RedoImplementation()
        {
            ChargePlayer();

            foreach (GameAction action in Actions)
            {
                try
                {
                    action.Redo();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        protected override void UndoImplementation()
        {
            ReturnMoney();

            for (int i = Actions.Count - 1; i >= 0; i--)
            {
                try
                {
                    Actions[i].Undo();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public override int DoCost()
        {
            return Actions.Aggregate(0, (acc, x) => acc += x.DoCost());
        }

        public override int UndoCost()
        {
            return Actions.Aggregate(0, (acc, x) => acc += x.UndoCost());
        }

        public void ChargePlayer()
        {
            int Amount = DoCost();
            if (!RoundAboutBuilder.NeedMoney || !ModLoadingExtension.appModeGame)
                return;

            if (Amount <= 0)
                return;

            if (Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, Amount) != Amount)
            {
                throw new ActionException("Failed to charge player: not enough money",this);
            }

            Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, Amount, ItemClass);
        }

        public void ReturnMoney()
        {
            int Amount = UndoCost();
            if (!RoundAboutBuilder.NeedMoney || !ModLoadingExtension.appModeGame)
                return;

            if (Amount <= 0)
                return;

            Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, Amount, ItemClass);
        }
    }

    public abstract class GameActionExtended : GameAction
    {
        public bool IsRedoable { get; protected set; }
        public bool IsDone { get; protected set; }
        public bool EverDone { get; protected set; }
        public int TimesDone { get; protected set; }

        public string Name { get; protected set; }

        public GameActionExtended(string name, bool redoable = false)
        {
            Name = name;
            IsRedoable = redoable;
        }

        public override void Do()
        {
            if (IsDone)
                throw new ActionException("Is already done",this);

            DoImplementation();

            EverDone = true;
            IsDone = true;
            TimesDone++;
        }

        public override void Undo()
        {
            if (!IsDone)
                throw new ActionException("Is not done", this);

            UndoImplementation();
            IsDone = false;
        }

        public override void Redo()
        {
            if (IsDone)
                throw new ActionException("Is already done", this);

            if (!EverDone)
                throw new ActionException("Was not ever done",this);

            if (!IsRedoable)
                throw new ActionException("Is not redoable", this);

            RedoImplementation();

            IsDone = true;
            TimesDone++;
        }

        public override string ToString()
        {
            return Name + " " + base.ToString();
        }

        protected abstract void DoImplementation();
        protected abstract void UndoImplementation();
        protected virtual void RedoImplementation() { }
    }

    public abstract class GameAction
    {
        public abstract void Do();
        public abstract void Undo();
        public abstract void Redo();

        public virtual int DoCost() { return 0; }
        public virtual int UndoCost() { return 0; }
    }

    public class ActionException : Exception
    {
        public GameActionExtended Action { get; private set; }

        public ActionException(string description, GameActionExtended action) : base(ModifyMessage(description,action))
        {
            this.Action = action;
        }

        private static string ModifyMessage(string description, GameActionExtended action)
        {
            return "Action " + action.Name + ": " + description;
        }
    }
}
