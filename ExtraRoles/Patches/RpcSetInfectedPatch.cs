using ExtraRoles2.Classes;
using HarmonyLib;

namespace ExtraRoles2.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    public class RpcSetInfectedPatch
    {
        static void Postfix()
        {
            RoleHelper.Instance.RpcAssignRoles();
        }
    }
}