using Essentials.UI;
using ExtraRolesMod;
using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod.Roles.Engineer
{
    public static class EngineerRepairButton
    {
        public static CooldownButton Button { get; private set; }


        public static void AddEngineerButton()
        {
            if (Button == null)
            {
                Button = new CooldownButton(Main.Assets.repairIco, new Vector2(7.967f, 0f), 0f, 0f, 0f);
                Button.OnClick += EngineerButton_OnClick;
            }
            Button.Visible = false;
        }

        private static void EngineerButton_OnClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DestroyableSingleton<HudManager>.Instance.ShowMap((Action<MapBehaviour>)delegate (MapBehaviour m)
            {
                m.ShowInfectedMap();
                m.ColorControl.baseColor = Main.Logic.sabotageActive ? Color.gray : Main.Palette.engineerColor;
            });
        }
    }
}
