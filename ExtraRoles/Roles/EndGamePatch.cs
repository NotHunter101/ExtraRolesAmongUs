using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtraRolesMod.Roles
{
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public static class EndGamePatch
    {
        public static void Prefix()
        {
            Engineer.HudUpdatePatch.EngineerButton.Visible = false;
            Officer.HudUpdatePatch.OfficerKillButton.Visible = false;
            Medic.HudUpdatePatch.MedicShieldButton.Visible = false;
        }
    }
}
