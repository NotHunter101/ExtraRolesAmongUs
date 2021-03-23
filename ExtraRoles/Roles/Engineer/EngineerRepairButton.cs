using Essentials.UI;
using ExtraRolesMod.Rpc;
using Reactor;
using System;
using UnityEngine;


namespace ExtraRolesMod.Roles.Engineer
{
    public static class EngineerRepairButton
    {
        public static GameplayButton Button { get; private set; }


        public static void AddEngineerButton()
        {
            Button = new GameplayButton(ExtraRoles.Assets.repairIco, new Vector2(6.5f, 0f));
            Button.OnClick += EngineerButton_OnClick;
            Button.OnUpdate += Button_OnUpdate;
        }

        private static void Button_OnUpdate(object sender, EventArgs e)
        {
            Button.Visible = !PlayerControl.LocalPlayer.Data.IsDead;
            Button.Visible &= PlayerControl.LocalPlayer.HasRole(Role.Engineer);
            Button.Visible &= PlayerTools.CanEngineerUseAbility();
        }

        private static void EngineerButton_OnClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ExtraRoles.Logic.CurrentSabotage == null)
                return;


            switch (ExtraRoles.Logic.CurrentSabotage.TaskType)
            {
                case TaskTypes.FixComms:
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    break;
                case TaskTypes.FixLights:
                    Rpc<FixLightsRpc>.Instance.Send(data: true, immediately: true);
                    break;
                case  TaskTypes.RestoreOxy:
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                    break;
                case TaskTypes.ResetReactor:
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                    break;

                case TaskTypes.ResetSeismic:
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                    break;
                default:
                    return;
            }
            PlayerControl.LocalPlayer.GetModdedControl().UsedAbility = true;

        }
    }
}
