using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public abstract class AbstractNetWrapper : GameAction
    {
        protected ushort _id;
        public ushort Id { get => _id == 0 ? throw new NetWrapperException("Item is not created") : _id; }

        protected bool _isBuildAction = true;
        public bool IsBuildAction { get => _isBuildAction; set => _isBuildAction = value; }

        public bool IsCreated()
        {
            return _id != 0;
        }

        public abstract void Create();

        public abstract bool Release();

        public override void Do()
        {
            if(_isBuildAction)
            {
                Create();
            }
            else
            {
                Release();
            }
        }

        public override void Undo()
        {
            if (_isBuildAction)
            {
                Release();
            }
            else
            {
                Create();
            }
        }

        public override void Redo()
        {
            Do();
        }
    }

    public class NetWrapperException : Exception
    {
        public NetWrapperException(string message) : base(message)
        {

        }
    }
}
