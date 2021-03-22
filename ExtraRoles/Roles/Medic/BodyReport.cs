using System;
using System.Collections.Generic;


namespace ExtraRolesMod.Roles.Medic
{
    //body report class for when medic reports a body
    public class BodyReport
    {
        public DeathReason DeathReason { get; set; }
        public PlayerControl Killer { get; set; }
        public PlayerControl Reporter { get; set; }
        public float KillAge { get; set; }

        public static string ParseBodyReport(BodyReport br)
        {
            System.Console.WriteLine(br.KillAge);
            if (br.KillAge > ExtraRoles.Config.medicKillerColorDuration * 1000)
            {
                return $"Body Report: The corpse is too old to gain information from. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            else if (br.DeathReason == (DeathReason)3)
            {
                return $"Body Report (Officer): The cause of death appears to be suicide! (Killed {Math.Round(br.KillAge / 1000)}s ago)";

            }
            else if (br.KillAge < ExtraRoles.Config.medicKillerNameDuration * 1000)
            {
                return $"Body Report: The killer appears to be {br.Killer.name}! (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            else
            {
                //TODO (make the type of color be written to chat
                var colors = new Dictionary<byte, string>()
                {
                    {0, "darker"},
                    {1, "darker"},
                    {2, "darker"},
                    {3, "lighter"},
                    {4, "lighter"},
                    {5, "lighter"},
                    {6, "darker"},
                    {7, "lighter"},
                    {8, "darker"},
                    {9, "darker"},
                    {10, "lighter"},
                    {11, "lighter"},
                };
                var typeOfColor = colors[br.Killer.Data.ColorId];
                return $"Body Report: The killer appears to be a {typeOfColor} color. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
        }
    }
}
