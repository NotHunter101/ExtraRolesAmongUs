using HarmonyLib;
using UnityEngine;


namespace ExtraRolesMod.Roles.Engineer
{

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
    class EngineerMapUpdate
    {
        static void Postfix(MapBehaviour __instance)
        {
            if (!PlayerControl.LocalPlayer.IsPlayerRole(Role.Engineer))
                return;
            if (!__instance.IsOpen || !__instance.infectedOverlay.gameObject.active)
                return;
            __instance.ColorControl.baseColor =
                !ExtraRoles.Logic.sabotageActive ? Color.gray : Colors.engineerColor;

            var perc = ExtraRoles.Logic.getRolePlayer(Role.Engineer).UsedAbility ? 1f : 0f;

            foreach (var room in __instance.infectedOverlay.rooms)
            {
                if (room.special == null)
                    continue;
                room.special.material.SetFloat("_Desat", !ExtraRoles.Logic.sabotageActive ? 1f : 0f);

                room.special.enabled = true;
                room.special.gameObject.SetActive(true);
                room.special.gameObject.active = true;
                room.special.material.SetFloat("_Percent", !PlayerControl.LocalPlayer.Data.IsDead ? perc : 1f);
            }
        }
    }

}
