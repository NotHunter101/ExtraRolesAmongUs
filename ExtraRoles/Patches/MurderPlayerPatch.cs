using Assets.CoreScripts;
using ExtraRoles2.Classes;
using HarmonyLib;
using Il2CppSystem;
using Reactor.Extensions;
using UnhollowerBaseLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ExtraRoles2.Patches.Officer_Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public class MurderPlayerPatch
    {
        static bool Prefix(PlayerControl __instance, PlayerControl __0)
        {
            if (AmongUsClient.Instance.IsGameOver) return false;
	        if (!__0 || __instance.Data.IsDead || __instance.Data.Disconnected)
			{
				int num = __0 ? __0.PlayerId : -1;
				return false;
			}
			GameData.PlayerInfo data = __0.Data;
			if (data == null || data.IsDead) return false;
			if (__instance.AmOwner)
			{
				StatsManager instance = StatsManager.Instance;
				uint num2 = instance.ImpostorKills;
				instance.ImpostorKills = num2 + 1U;
				SoundManager.Instance.PlaySound(__instance.KillSfx, false, 0.8f);
			}
			__instance.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
			DestroyableSingleton<Telemetry>.Instance.WriteMurder(); 
			__0.gameObject.layer = LayerMask.NameToLayer("Ghost");
			if (__0.AmOwner)
			{
				StatsManager instance2 = StatsManager.Instance;
				uint num2 = instance2.TimesMurdered;
				instance2.TimesMurdered = num2 + 1U;
				if (Minigame.Instance)
				{
					try
					{
						Minigame.Instance.Close();
						Minigame.Instance.Close();
					}
					catch
					{
					}
				}
				DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowOne(__instance.Data, data);
				DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
				__0.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
				__0.RpcSetScanner(false);
				ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
				importantTextTask.transform.SetParent(__instance.transform, false);
				if (!PlayerControl.GameOptions.GhostsDoTasks)
				{
					for (int i = 0; i < __0.myTasks.Count; i++)
					{
						PlayerTask playerTask = __0.myTasks[i];
						playerTask.OnRemove();
						Object.Destroy(playerTask.gameObject);
					}
					__0.myTasks.Clear();
					importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostIgnoreTasks, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
				}
				else
					importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostDoTasks, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
				__0.myTasks.Insert(0, importantTextTask);
			}
			__instance.MyPhysics.StartCoroutine(__instance.KillAnimations.Random().CoPerformKill(__instance, __0));
            
            return false;
        }
    }
}