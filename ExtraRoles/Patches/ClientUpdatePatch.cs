using ExtraRoles2.Classes;
using HarmonyLib;
using InnerNet;

namespace ExtraRoles2.Patches
{
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Update))]
    public class ClientUpdatePatch
    {
        static void Postfix()
        {
            Main.Instance?.Update();
        }
    }
}