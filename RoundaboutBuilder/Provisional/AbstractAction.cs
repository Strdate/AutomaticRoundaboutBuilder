using System;
using System.Collections.Generic;

namespace Provisional.Actions
{
    public class ActionGroup : Action
    {
        public List<Action> Actions = new List<Action>();

        public ActionGroup(string name, bool redoable=false) : base(name)
        {
        }

        protected override void DoImplementation()
        {
            foreach(Action action in Actions)
            {
                action.Do();
            }
        }

        protected override void RedoImplementation()
        {
            foreach (Action action in Actions)
            {
                action.Redo();
            }
        }

        protected override void UndoImplementation()
        {
            for (int i = Actions.Count; i >= 0; i--)
            {
                Actions[i].Undo();
            }
        }
    }

    public abstract class Action
    {
        public bool IsRedoable { get; protected set; }
        public bool IsDone { get; protected set; }
        public bool EverDone { get; protected set; }
        public int TimesDone { get; protected set; }

        public string Name { get; protected set; }

        public Action(string name, bool redoable = false)
        {
            Name = name;
            IsRedoable = redoable;
        }

        public void Do()
        {
            if (IsDone)
                throw new ActionException("Is already done",this);

            EverDone = true;
            IsDone = true;
            TimesDone++;

            DoImplementation();
        }

        public void Undo()
        {
            if (!IsDone)
                throw new ActionException("Is not done", this);

            IsDone = false;

            UndoImplementation();
        }

        public void Redo()
        {
            // TODO
        }

        public override string ToString()
        {
            return Name + " " + base.ToString();
        }

        protected abstract void DoImplementation();
        protected abstract void UndoImplementation();
        protected abstract void RedoImplementation();
    }

    public class ActionException : Exception
    {
        public Action Action { get; private set; }

        public ActionException(string description, Action action) : base(ModifyMessage(description,action))
        {
            this.Action = action;
        }

        private static string ModifyMessage(string description, Action action)
        {
            return "Action " + action.Name + ": " + description;
        }
    }
}
