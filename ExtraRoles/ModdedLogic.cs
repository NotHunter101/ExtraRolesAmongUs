using ExtraRolesMod.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtraRolesMod
{
    public class ModdedLogic
    {
        public ModPlayerControl GetRolePlayer(Role roleName)
        {
            return ExtraRoles.Logic.AllModPlayerControl.Find(x => x.Role == roleName);
        }
        public void ClearJokerTasks()
        {
            var joker = ExtraRoles.Logic.GetRolePlayer(Role.Joker);
            if (joker == null)
                return;
            var jokerControl = joker.PlayerControl;
            var removeTask = new List<PlayerTask>();

            foreach (var task in jokerControl.myTasks)
                if (!PlayerTools.sabotageTasks.Contains(task.TaskType))
                    removeTask.Add(task);

            foreach (var task in removeTask)
                jokerControl.RemoveTask(task);
        }

        public List<ModPlayerControl> AllModPlayerControl = new List<ModPlayerControl>();
        public bool sabotageActive { get; set; }
    }
}
