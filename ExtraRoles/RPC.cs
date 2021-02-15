using HarmonyLib;
using Hazel;
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
            switch (packetId)
            {
                case (byte)CustomRPC.AttemptSound:
                    BreakShield(false);
                    break;
                case (byte)CustomRPC.FixLights:
                    var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    break;
                case (byte)CustomRPC.SetLocalPlayers:
                    localPlayers.Clear();
                    localPlayer = PlayerControl.LocalPlayer;
                    var localPlayerBytes = ALMCIJKELCP.ReadBytesAndSize();
                    foreach (var id in localPlayerBytes)
                        foreach (var player in PlayerControl.AllPlayerControls)
                            if (player.PlayerId == id)
                                localPlayers.Add(player);
                    break;
                case (byte)CustomRPC.ResetVaribles:
                    MedicSettings.ClearSettings();
                    OfficerSettings.ClearSettings();
                    EngineerSettings.ClearSettings();
                    JokerSettings.ClearSettings();
                    MedicSettings.SetConfigSettings();
                    OfficerSettings.SetConfigSettings();
                    EngineerSettings.SetConfigSettings();
                    JokerSettings.SetConfigSettings();
                    killedPlayers.Clear();
                    break;
                case (byte)CustomRPC.SetMedic:
                    ConsoleTools.Info("Medic Set Through RPC!");
                    var MedicId = ALMCIJKELCP.ReadByte();
                    foreach (var player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == MedicId)
                            MedicSettings.Medic = player;
                    break;
                case (byte)CustomRPC.SetProtected:
                    var ProtectedId = ALMCIJKELCP.ReadByte();
                    foreach (var player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == ProtectedId)
                            MedicSettings.Protected = player;
                    break;
                case (byte)CustomRPC.SetOfficer:
                    ConsoleTools.Info("Officer Set Through RPC!");
                    var OfficerId = ALMCIJKELCP.ReadByte();
                    foreach (var player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == OfficerId)
                            OfficerSettings.Officer = player;
                    break;
                case (byte)CustomRPC.OfficerKill:
                    var killerid = ALMCIJKELCP.ReadByte();
                    var targetid = ALMCIJKELCP.ReadByte();
                    var killer = PlayerTools.getPlayerById(killerid);
                    var target = PlayerTools.getPlayerById(targetid);
                    killer.MurderPlayer(target);
                    if (target == MedicSettings.Protected)
                        BreakShield(false);
                    break;
                case (byte)CustomRPC.SetEngineer:
                    ConsoleTools.Info("Engineer Set Through RPC!");
                    var EngineerId = ALMCIJKELCP.ReadByte();
                    foreach (var player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == EngineerId)
                            EngineerSettings.Engineer = player;
                    break;
                case (byte)CustomRPC.SetJoker:
                    ConsoleTools.Info("Joker Set Through RPC!");
                    var JokerId = ALMCIJKELCP.ReadByte();
                    foreach (var player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == JokerId)
                            JokerSettings.Joker = player;
                    break;
                case (byte)CustomRPC.JokerWin:
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player == JokerSettings.Joker)
                            continue;
                        
                        player.RemoveInfected();
                        player.Die(DeathReason.Exile);
                        player.Data.IsDead = true;
                        player.Data.IsImpostor = false;
                    }
                    JokerSettings.Joker.Revive();
                    JokerSettings.Joker.Data.IsDead = false;
                    JokerSettings.Joker.Data.IsImpostor = true;
                    break;
            }
        }
    }
}
