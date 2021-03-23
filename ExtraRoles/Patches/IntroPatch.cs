using System.Linq;
using ExtraRoles2.Classes;
using HarmonyLib;
using InnerNet;

namespace ExtraRoles2.Patches
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    public class IntroPatch
    {
        static void Prefix(IntroCutscene.CoBegin__d __instance)
        {
            Player moddedPlayer = PlayerControl.LocalPlayer.GetModdedPlayer();
            if (moddedPlayer.Role == null) return;
            
            if (moddedPlayer.Role.Teamless)
            {
                __instance.yourTeam.Clear();
                __instance.yourTeam.Add(moddedPlayer.Owner);
            }
        }
        
        static void Postfix(IntroCutscene.CoBegin__d __instance)
        {
            Player moddedPlayer = PlayerControl.LocalPlayer.GetModdedPlayer();
            if (moddedPlayer.Role == null) return;
            
            moddedPlayer.Role.ResetButtons();

            __instance.__this.Title.Text = moddedPlayer.Role.Name;
            __instance.__this.Title.Color = moddedPlayer.Role.Color;
            __instance.__this.BackgroundBar.material.color = moddedPlayer.Role.Color;
            if (moddedPlayer.Role.IntroString != null)
            {
                if (moddedPlayer.Owner.Data.IsImpostor)
                {
                    __instance.__this.ImpostorText.gameObject.SetActive(true);
                    __instance.__this.ImpostorText.scale = 0.5f;
                }
                __instance.__this.ImpostorText.Text = moddedPlayer.Role.IntroString;
            }
        }
    }
}