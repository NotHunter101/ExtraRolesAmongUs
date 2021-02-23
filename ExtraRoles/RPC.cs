using System;
using HarmonyLib;
using Hazel;
using System.Linq;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    enum RPC
    {
        PlayAnimation = 0,
        CompleteTask = 1,
        SyncSettings = 2,
        SetInfected = 3,
        Exiled = 4,
        CheckName = 5,
        SetName = 6,
        CheckColor = 7,
        SetColor = 8,
        SetHat = 9,
        SetSkin = 10,
        ReportDeadBody = 11,
        MurderPlayer = 12,
        SendChat = 13,
        StartMeeting = 14,
        SetScanner = 15,
        SendChatNote = 16,
        SetPet = 17,
        SetStartCounter = 18,
        EnterVent = 19,
        ExitVent = 20,
        SnapTo = 21,
        Close = 22,
        VotingComplete = 23,
        CastVote = 24,
        ClearVote = 25,
        AddVote = 26,
        CloseDoorsOfType = 27,
        RepairSystem = 28,
        SetTasks = 29,
        UpdateGameData = 30,
    }

    enum CustomRPC
    {
        SetMedic = 43,
        SetProtected = 44,
        SetOfficer = 46,
        OfficerKill = 47,
        SetEngineer = 48,
        FixLights = 49,
        SetJoker = 50,
        ResetVaribles = 51,
        SetLocalPlayers = 56,
        JokerWin = 57,
        AttemptSound = 58
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class HandleRpcPatch
    {
        static void Postfix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
        {
            var packetId = HKHMBLJFLMC;
            var reader = ALMCIJKELCP;

            var setRole = new Action<string>(roleName =>
            {
                var roleId = ALMCIJKELCP.ReadByte();
                foreach (var player in PlayerControl.AllPlayerControls)
                    if (player.PlayerId == roleId)
                        player.getModdedControl().Role = roleName;
            });

            switch (packetId)
            {
                case (byte) CustomRPC.AttemptSound:
                    BreakShield(false);
                    break;
                case (byte) CustomRPC.FixLights:
                    var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    break;
                case (byte) CustomRPC.SetLocalPlayers:
                    localPlayers.Clear();
                    localPlayer = PlayerControl.LocalPlayer;
                    var localPlayerBytes = ALMCIJKELCP.ReadBytesAndSize();
                    foreach (var id in localPlayerBytes)
                    foreach (var player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == id)
                            localPlayers.Add(player);
                    break;
                case (byte) CustomRPC.ResetVaribles:
                    Main.Config.SetConfigSettings();
                    Main.Logic.AllModPlayerControl.Clear();
                    killedPlayers.Clear();
                    var crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
                    foreach (var plr in crewmates)
                        Main.Logic.AllModPlayerControl.Add(new ModPlayerControl
                        {
                            PlayerControl = plr, Role = "Impostor", UsedAbility = false, LastAbilityTime = null,
                            Immortal = false
                        });
                    crewmates.RemoveAll(x => x.Data.IsImpostor);
                    foreach (var plr in crewmates)
                        plr.getModdedControl().Role = "Crewmate";
                    break;
                case (byte) CustomRPC.SetMedic:
                    setRole("Medic");
                    break;
                case (byte) CustomRPC.SetProtected:
                    var protectedId = ALMCIJKELCP.ReadByte();
                    foreach (var player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == protectedId)
                            player.getModdedControl().Immortal = true;
                    break;
                case (byte) CustomRPC.SetOfficer:
                    setRole("Officer");
                    break;
                case (byte) CustomRPC.OfficerKill:
                    var killerId = ALMCIJKELCP.ReadByte();
                    var killer = PlayerTools.getPlayerById(killerId);

                    var targetId = ALMCIJKELCP.ReadByte();
                    var target = PlayerTools.getPlayerById(targetId);

                    killer.MurderPlayer(target);
                    if (target.isPlayerImmortal())
                        BreakShield(false);
                    break;
                case (byte) CustomRPC.SetEngineer:
                    setRole("Engineer");
                    break;
                case (byte) CustomRPC.SetJoker:
                    setRole("Joker");
                    break;
                case (byte) CustomRPC.JokerWin:
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player.isPlayerRole("Joker")) 
                            continue;
                        
                        player.RemoveInfected();
                        player.Die(DeathReason.Exile);
                        player.Data.IsDead = true;
                        player.Data.IsImpostor = false;
                    }

                    var joker = Main.Logic.getRolePlayer("Joker").PlayerControl;
                    joker.Revive();
                    joker.Data.IsDead = false;
                    joker.Data.IsImpostor = true;
                    break;
            }
        }
    }
}