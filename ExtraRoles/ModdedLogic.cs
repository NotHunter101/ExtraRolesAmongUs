using ExtraRolesMod.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtraRolesMod
{
    public class ModdedLogic
    {
        public ModPlayerControl getRolePlayer(Role roleName)
        {
            return ExtraRoles.Logic.AllModPlayerControl.Find(x => x.Role == roleName);
        }

        public ModPlayerControl getImmortalPlayer()
        {
            return ExtraRoles.Logic.AllModPlayerControl.Find(x => x.Immortal);
        }

        public bool anyPlayerImmortal()
        {
            return ExtraRoles.Logic.AllModPlayerControl.FindAll(x => x.Immortal).Count > 0;
        }

        public void ClearJokerTasks()
        {
            var joker = ExtraRoles.Logic.getRolePlayer(Role.Joker);
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
