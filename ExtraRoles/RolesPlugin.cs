using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;
using ExtraRoles2.Classes;

namespace ExtraRoles2
{
    public enum RoleId
    {
        Engineer = 0,
        Joker = 1,
        Medic = 2,
        Officer = 3
    }
    
    public enum CustomRPC
    {
        SetRoles = 0,
        FixLights = 1,
    }
    
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class RolesPlugin : BasePlugin
    {
        public const string Id = "gg.reactor.extraroles";
        public const byte DataPacket = 116;
        public Harmony Harmony { get; } = new Harmony(Id);
        
        public override void Load()
        {
            Harmony.PatchAll();
            
            Main.Instance = new Main();
            RpcHelper.Instance = new RpcHelper();
            RoleHelper.Instance = new RoleHelper();
        }
    }
    
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static class AmBannedPatch
    {
        static void Postfix(out bool __result)
        {
            __result = false;
        }
    }
}
