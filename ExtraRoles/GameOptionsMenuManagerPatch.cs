using HarmonyLib;

namespace ExtraRolesMod
{

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_5))]
    class GameOptionsData_ToHudString
    {
        static void Postfix()
        {
            HudManager.Instance.GameSettings.scale = 0.5f;
        }
    }


    [HarmonyPatch]
    class GameOptionsMenuManger
    {
        static float defaultBounds = 0f;

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        class Start
        {
            static void Postfix(ref GameOptionsMenu __instance)
            {
                defaultBounds = __instance.GetComponentInParent<Scroller>().YBounds.max;
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        class Update
        {
            static void Postfix(ref GameOptionsMenu __instance)
            {
                __instance.GetComponentInParent<Scroller>().YBounds.max = 13.5f;
            }
        }
    }
}
