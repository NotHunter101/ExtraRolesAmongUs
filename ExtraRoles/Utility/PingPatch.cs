using HarmonyLib;


namespace ExtraRolesMod.Utility
{

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            __instance.text.Text += "\nextraroles.net";
            __instance.text.Text += "\nExtraRoles " + ExtraRoles.Version;
        }
    }
}
