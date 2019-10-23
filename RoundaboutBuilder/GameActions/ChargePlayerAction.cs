using ColossalFramework;
using RoundaboutBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public class ChargePlayerAction : GameAction
    {
        public int Amount { get; private set; }
        public ItemClass ItemClass { get; private set; }

        public ChargePlayerAction(int amount, ItemClass itemClass)
        {
            Amount = amount;
            ItemClass = itemClass;
        }

        public bool CheckMoney()
        {
            if (!RoundAboutBuilder.NeedMoney || !ModLoadingExtension.appModeGame)
                return true;

            if (Amount <= 0)
                return true;

            if (Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, Amount) == Amount)
            {
                return true;
            }

            return false;
        }

        public override void Do()
        {
            if (!RoundAboutBuilder.NeedMoney || !ModLoadingExtension.appModeGame)
                return;

            if (Amount <= 0)
                return;

            if (Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, Amount) != Amount)
            {
                throw new Exception("Failed to charge player: not enough money");
            }

            Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, Amount, ItemClass);
        }

        public override void Redo()
        {
            Do();
        }

        public override void Undo()
        {
            if (!RoundAboutBuilder.NeedMoney || !ModLoadingExtension.appModeGame)
                return;

            if (Amount <= 0)
                return;

            Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, Amount, ItemClass);
        }
    }
}
