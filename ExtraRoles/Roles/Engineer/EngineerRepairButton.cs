using Essentials.UI;
using System;
using UnityEngine;


namespace ExtraRolesMod.Roles.Engineer
{
    public static class EngineerRepairButton
    {
        public static GameplayButton Button { get; private set; }


        public static void AddEngineerButton()
        {
            Button = new GameplayButton(ExtraRoles.Assets.repairIco, new Vector2(7.967f, 0f));
            Button.OnClick += EngineerButton_OnClick;
            Button.OnUpdate += Button_OnUpdate;
        }

        private static void Button_OnUpdate(object sender, EventArgs e)
        {
            Button.Visible = !PlayerControl.LocalPlayer.Data.IsDead;
            Button.Visible &= PlayerControl.LocalPlayer.HasRole(Role.Engineer);
            Button.Visible &= !PlayerControl.LocalPlayer.GetModdedControl().UsedAbility;
        }

        private static void EngineerButton_OnClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DestroyableSingleton<HudManager>.Instance.ShowMap((Action<MapBehaviour>)delegate (MapBehaviour m)
            {
                m.ShowInfectedMap();
                m.ColorControl.baseColor = ExtraRoles.Logic.sabotageActive ? Color.gray : Colors.engineerColor;
            });
        }
    }
}
