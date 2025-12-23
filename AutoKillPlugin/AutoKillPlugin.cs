using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ModdingTales;
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

        // Deletion management
        internal static Dictionary<CreatureGuid, CreatureBoardAsset> creaturesToDelete = new Dictionary<CreatureGuid, CreatureBoardAsset>();
        internal static Queue<CreatureBoardAsset> creaturesPendingDeletion = new Queue<CreatureBoardAsset>();

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Debug.Log("Auto Kill loaded");

            trigger = Config.Bind("Hotkeys", "Delete Asset", new KeyboardShortcut(KeyCode.Delete, KeyCode.RightControl));

            ModdingUtils.AddPluginToMenuList(this);
            Harmony harmony = new Harmony(Guid);
            harmony.PatchAll();
        }

        void Update()
        {
            // Only allow in GM mode
            if (!LocalClient.IsInGmMode)
                return;

            if (StrictKeyCheck(trigger.Value))
                DeleteCreatures();

            // Process pending deletions
            if (creaturesPendingDeletion.Count > 0)
            {
                var creature = creaturesPendingDeletion.Dequeue();
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

            // Add to deletion list
            foreach (CreatureGuid selectedAssetId in selectedAssets.Where(c => !creaturesToDelete.ContainsKey(c)))
            {
                if (CreaturePresenter.TryGetAsset(selectedAssetId, out var creature))
                {
                    if (PlayMode.Instance.CurrentState.Is<PlayMode.TurnBased>()) {
                        creaturesToDelete.Add(selectedAssetId, creature);
                        // Knock prone
                        creature.ToggleStatusEmote(ActionTimelineDatabase.GetActionPrefab("TLA_Action_Knockdown"));
                    }
                    else
                    {
                        creaturesPendingDeletion.Enqueue(creature);
                    }
                }
            }
        }
    }
}
