using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ModdingTales;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AutoKill
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(PluginUtilities.SetInjectionFlag.Guid)]
    public class AutoKillPlugin : BaseUnityPlugin
    {
        // constants
        public const string Guid = "org.hollofox.plugins.AutoKillPlugin";
        public const string Name = "Auto Kill Plugin";
        public const string Version = "0.0.0.0";

        // Triggering Key
        private ConfigEntry<KeyboardShortcut> trigger;
        private ConfigEntry<int> ttl;

        // Deletion management
        internal static Dictionary<CreatureGuid, CreatureBoardAsset> creaturesToDelete = new Dictionary<CreatureGuid, CreatureBoardAsset>();
        internal static LinkedList<(CreatureBoardAsset, DateTimeOffset)> creaturesPendingDeletion = new LinkedList<(CreatureBoardAsset, DateTimeOffset)>();

        internal static ActionTimeline knockedDownAction;

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Debug.Log("Auto Kill loaded");

            trigger = Config.Bind("Hotkeys", "Delete Asset", new KeyboardShortcut(KeyCode.Delete, KeyCode.RightControl));
            ttl = Config.Bind("Timer", "Time To Live", 30);

            ModdingUtils.AddPluginToMenuList(this);
            Harmony harmony = new Harmony(Guid);
            harmony.PatchAll();

            // Not the right event, this occurs from GM/Player/Spectator mode swap
            PlayMode.OnStateChange += (mode) =>
            {
                if (mode.Is<PlayMode.TurnBased>())
                {
                    while (creaturesPendingDeletion.Count > 0)
                    {
                        CreatureBoardAsset c = creaturesPendingDeletion.First().Item1;
                        creaturesPendingDeletion.RemoveFirst();
                        creaturesToDelete.Add(c.CreatureId, c);
                    }

                    // Remove the creature now if it starts the initiative round and needs to be deleted
                    if (InitiativeManager.CurrentTurnElement != null && creaturesToDelete.ContainsKey(InitiativeManager.CurrentTurnElement.CreatureGuid))
                    {
                        var key = InitiativeManager.CurrentTurnElement.CreatureGuid;
                        CreatureBoardAsset c = creaturesToDelete[key];
                        creaturesPendingDeletion.AddFirst((c, DateTimeOffset.Now));
                        creaturesToDelete.Remove(key);
                    }
                }
                else
                {
                    CreatureGuid[] keys = creaturesToDelete.Keys.ToArray();
                    foreach (CreatureGuid key in keys)
                    {
                        CreatureBoardAsset c = creaturesToDelete[key];
                        creaturesPendingDeletion.AddFirst((c, DateTimeOffset.Now.AddSeconds(ttl.Value)));
                        creaturesToDelete.Remove(key);
                    }
                }
            };

            knockedDownAction = ActionTimelineDatabase.GetActionPrefab("TLA_Action_Knockdown");
        }

        void Update()
        {
            // Only allow in GM mode
            if (!LocalClient.IsInGmMode)
                return;

            if (StrictKeyCheck(trigger.Value))
                DeleteCreatures();

            // Process pending deletions
            if (creaturesPendingDeletion.Count > 0 && creaturesPendingDeletion.First().Item2 < DateTimeOffset.Now)
            {
                CreatureBoardAsset creature = creaturesPendingDeletion.First().Item1;
                creaturesPendingDeletion.RemoveFirst();
                creature.RequestDelete();
            }
        }

        // Credit to LA for this method
        public bool StrictKeyCheck(KeyboardShortcut check)
        {
            if (!check.IsUp()) { return false; }
            foreach (KeyCode modifier in new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.RightControl, KeyCode.RightShift })
            {
                if (Input.GetKey(modifier) != check.Modifiers.Contains(modifier)) { return false; }
            }
            return true;
        }

        // Delete selected creatures
        private void DeleteCreatures()
        {
            
            // not a ternary due to out parameter
            CreatureGuid[] selectedAssets;
            if (LocalClient.HasLassoedCreatures)
            {
                LocalClient.TryGetLassoedCreatureIds(out selectedAssets);
            }
            else
            {
                selectedAssets = new[] { LocalClient.SelectedCreatureId };
            }

            if (selectedAssets.Any(c => !creaturesToDelete.ContainsKey(c)))
            {

                // Add to deletion list
                foreach (CreatureGuid selectedAssetId in selectedAssets.Where(c => !creaturesToDelete.ContainsKey(c)))
                {
                    if (CreaturePresenter.TryGetAsset(selectedAssetId, out CreatureBoardAsset creature))
                    {
                        if (!creature.IsPersistantEmoteEnabled(knockedDownAction.ActionTimelineId))
                            creature.ToggleStatusEmote(knockedDownAction);
                        if (PlayMode.Instance.CurrentState.Is<PlayMode.TurnBased>())
                        {
                            if (InitiativeManager.CurrentTurnElement != null && InitiativeManager.CurrentTurnElement.CreatureGuid == selectedAssetId)
                            {
                                // Delete it now
                                creaturesPendingDeletion.AddFirst((creature, DateTimeOffset.Now));
                            }
                            else
                            {
                                creaturesToDelete.Add(selectedAssetId, creature);
                            }
                        }
                        else
                        {
                            creaturesPendingDeletion.AddFirst((creature, DateTimeOffset.Now.AddSeconds(ttl.Value)));
                        }
                    }
                }
            }
            else if (selectedAssets.All(c => creaturesToDelete.ContainsKey(c)))
            {
                // Remove from deletion list
                foreach (CreatureGuid selectedAssetId in selectedAssets)
                {
                    CreatureBoardAsset c = creaturesToDelete[selectedAssetId];
                    if (c.IsPersistantEmoteEnabled(knockedDownAction.ActionTimelineId))
                        c.ToggleStatusEmote(knockedDownAction);
                    creaturesToDelete.Remove(selectedAssetId);
                }
            }
        }
    }
}
