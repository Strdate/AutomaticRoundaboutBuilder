using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoundaboutBuilder.Tools
{
    public static class CheckMoney
    {
        public static bool ChargePlayer(int cost, ItemClass itemClass)
        {
            if (!RoundAboutBuilder.NeedMoney)
                return true;

            if (cost <= 0)
                return true;

            if(Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, cost) != cost)
            {
                return false;
            }
            else
            {
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, cost, itemClass);
                return true;
            }

        }
    }
}
