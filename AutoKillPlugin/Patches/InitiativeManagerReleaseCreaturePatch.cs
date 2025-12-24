using HarmonyLib;
using System;

namespace AutoKillPlugin.Patches
{
    [HarmonyPatch(typeof(UI_InitativeManager), "OnTurnSwitch")]
    public class InitiativeManagerReleaseCreaturePatch
    {
        static void Postfix(QueueElement element)
        {
            if (element.CreatureGuid == null)
                return;
            if (!AutoKill.AutoKillPlugin.creaturesToDelete.ContainsKey(element.CreatureGuid))
                return;
            AutoKill.AutoKillPlugin.creaturesPendingDeletion.AddFirst((AutoKill.AutoKillPlugin.creaturesToDelete[element.CreatureGuid], DateTimeOffset.Now));
            AutoKill.AutoKillPlugin.creaturesToDelete.Remove(element.CreatureGuid);
        }
    }
}
