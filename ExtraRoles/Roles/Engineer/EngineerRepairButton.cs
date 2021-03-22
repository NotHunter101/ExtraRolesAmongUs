using Essentials.UI;
using ExtraRolesMod;
using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace ExtraRolesMod.Roles.Engineer
{
    public static class EngineerRepairButton
    {
        public static CooldownButton Button { get; private set; }


        public static void AddEngineerButton()
        {
            Button = new CooldownButton(ExtraRoles.Assets.repairIco, new Vector2(7.967f, 0f), 0f, 0f, 0f);
            Button.OnClick += EngineerButton_OnClick;
            Button.OnUpdate += Button_OnUpdate;
            Button.Visible = false;
        }

        private static void Button_OnUpdate(object sender, EventArgs e)
        {
            Button.Visible = PlayerControl.LocalPlayer.IsPlayerRole(Role.Engineer);
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
