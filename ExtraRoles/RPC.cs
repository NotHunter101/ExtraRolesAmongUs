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
        ShieldBreak = 45,
        SetOfficer = 46,
        OfficerKill = 47,
        SetEngineer = 48,
        FixLights = 49,
        SetJoker = 50,
        ResetVaribles = 51,
        SetLocalPlayers = 56,
        JokerWin = 57,
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class HandleRpcPatch
    {
        static void Postfix(byte __0, MessageReader __1)
        {
            byte packetId = __0;
            MessageReader reader = __1;
            switch (packetId)
            {
                case (byte)CustomRPC.ShieldBreak:
                    if (MedicSettings.Protected != null)
                    {
                        MedicSettings.Protected.myRend.material.SetColor("_VisorColor", Palette.VisorColor);
                        MedicSettings.Protected.myRend.material.SetFloat("_Outline", 0f);
                    }    
                    MedicSettings.Protected = null;
                    break;
                case (byte)CustomRPC.FixLights:
                    SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    break;
                case (byte)CustomRPC.SetLocalPlayers:
                    ConsoleTools.Info("Setting Local Players...");
                    localPlayers.Clear();
                    localPlayer = PlayerControl.LocalPlayer;
                    var localPlayerBytes = reader.ReadBytesAndSize();

                    foreach (byte id in localPlayerBytes)
                    {
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == id)
                                localPlayers.Add(player);
                        }
                    }
                    break;
                case (byte)RPC.SetInfected:
                    {
                        ConsoleTools.Info("set infected.");
                        break;
                    }
                case (byte)CustomRPC.ResetVaribles:
                    {
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
                    }
                case (byte)CustomRPC.SetMedic:
                    {
                        ConsoleTools.Info("Medic Set Through RPC!");
                        byte MedicId = reader.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == MedicId)
                            {
                                MedicSettings.Medic = player;
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.SetProtected:
                    {
                        byte ProtectedId = reader.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == ProtectedId)
                            {
                                MedicSettings.Protected = player;
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.SetOfficer:
                    {
                        ConsoleTools.Info("Officer Set Through RPC!");
                        byte OfficerId = reader.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == OfficerId)
                            {
                                OfficerSettings.Officer = player;
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.OfficerKill:
                    {
                        var killerid = reader.ReadByte();
                        var targetid = reader.ReadByte();
                        PlayerControl killer = PlayerTools.getPlayerById(killerid);
                        PlayerControl target = PlayerTools.getPlayerById(targetid);
                        killer.MurderPlayer(target);
                        break;
                    }
                case (byte)CustomRPC.SetEngineer:
                    {
                        ConsoleTools.Info("Engineer Set Through RPC!");
                        byte EngineerId = reader.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == EngineerId)
                            {
                                EngineerSettings.Engineer = player;
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.SetJoker:
                    {
                        ConsoleTools.Info("Joker Set Through RPC!");
                        byte JokerId = reader.ReadByte();
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.PlayerId == JokerId)
                            {
                                JokerSettings.Joker = player;
                            }
                        }
                        break;
                    }
                case (byte)CustomRPC.JokerWin:
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
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
