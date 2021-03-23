using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtraRolesMod.Roles.Engineer
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcRepairSystem))]
    public static class SabotagePatch
    {
        public static void Postfix([HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] int amount)
        {
            System.Console.WriteLine("Sabotaged {0} at {1}", systemType, amount);
        }

    }
}
