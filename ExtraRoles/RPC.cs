using HarmonyLib;
using Hazel;
using System.Collections.Generic;
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
            byte packetId = HKHMBLJFLMC;
            switch (packetId)
            {
                case (byte)CustomRPC.AttemptSound:
                    BreakShield(false);
                    break;
                case (byte)CustomRPC.FixLights:
                    SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    break;
                case (byte)CustomRPC.SetLocalPlayers:
                    localPlayers.Clear();
                    localPlayer = PlayerControl.LocalPlayer;
                    var localPlayerBytes = ALMCIJKELCP.ReadBytesAndSize();
                    foreach (byte id in localPlayerBytes)
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                            if (player.PlayerId == id)
                                localPlayers.Add(player);
                    break;
                case (byte)CustomRPC.ResetVaribles:
                    Main.Config.SetConfigSettings();
                    Main.Logic.AllModPlayerControl.Clear();
                    killedPlayers.Clear();
                    List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
                    foreach (PlayerControl plr in crewmates)
                        Main.Logic.AllModPlayerControl.Add(new ModPlayerControl { PlayerControl = plr, Role = "Impostor", UsedAbility = false, LastAbilityTime = null, Immortal = false });
                    crewmates.RemoveAll(x => x.Data.IsImpostor);
                    foreach (PlayerControl plr in crewmates)
                        plr.getModdedControl().Role = "Crewmate";
                    break;
                case (byte)CustomRPC.SetMedic:
                    byte MedicId = ALMCIJKELCP.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == MedicId)
                            player.getModdedControl().Role = "Medic";
                    break;
                case (byte)CustomRPC.SetProtected:
                    byte ProtectedId = ALMCIJKELCP.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == ProtectedId)
                            player.getModdedControl().Immortal = true;
                    break;
                case (byte)CustomRPC.SetOfficer:
                    byte OfficerId = ALMCIJKELCP.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == OfficerId)
                            player.getModdedControl().Role = "Officer";
                    break;
                case (byte)CustomRPC.OfficerKill:
                    var killerid = ALMCIJKELCP.ReadByte();
                    var targetid = ALMCIJKELCP.ReadByte();
                    PlayerControl killer = PlayerTools.getPlayerById(killerid);
                    PlayerControl target = PlayerTools.getPlayerById(targetid);
                    killer.MurderPlayer(target);
                    if (target.isPlayerImmortal())
                        BreakShield(false);
                    break;
                case (byte)CustomRPC.SetEngineer:
                    byte EngineerId = ALMCIJKELCP.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == EngineerId)
                            player.getModdedControl().Role = "Engineer";
                    break;
                case (byte)CustomRPC.SetJoker:
                    byte JokerId = ALMCIJKELCP.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.PlayerId == JokerId)
                            player.getModdedControl().Role = "Joker";
                    break;
                case (byte)CustomRPC.JokerWin:
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (!player.isPlayerRole("Joker"))
                        {
                            player.RemoveInfected();
                            player.Die(DeathReason.Exile);
                            player.Data.IsDead = true;
                            player.Data.IsImpostor = false;
                        }
                    }
                    PlayerControl joker = Main.Logic.getRolePlayer("Joker").PlayerControl;
                    joker.Revive();
                    joker.Data.IsDead = false;
                    joker.Data.IsImpostor = true;
                    break;
            }
        }
    }
}
