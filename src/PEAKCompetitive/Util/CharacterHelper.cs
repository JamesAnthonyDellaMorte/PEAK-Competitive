using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Photon.Pun;

namespace PEAKCompetitive.Util
{
    /// <summary>
    /// Helper class to interact with PEAK's Character system
    /// </summary>
    public static class CharacterHelper
    {
        // Cache reflection results
        private static MethodInfo _killMethod;
        private static MethodInfo _reviveMethod;
        private static MethodInfo _teleportMethod;
        private static PropertyInfo _healthProperty;
        private static bool _reflected = false;

        static CharacterHelper()
        {
            ReflectCharacterMethods();
        }

        private static void ReflectCharacterMethods()
        {
            if (_reflected) return;

            try
            {
                Type characterType = typeof(Character);

                // Find all methods
                MethodInfo[] methods = characterType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                Plugin.Logger.LogInfo("=== Character Methods ===");
                foreach (var method in methods.Take(50))
                {
                    Plugin.Logger.LogInfo($"Method: {method.Name} - Returns: {method.ReturnType.Name}");
                }

                // Look for death/kill methods
                _killMethod = methods.FirstOrDefault(m =>
                    m.Name.Contains("Die") ||
                    m.Name.Contains("Kill") ||
                    m.Name.Contains("Death"));

                // Look for revive/respawn methods
                _reviveMethod = methods.FirstOrDefault(m =>
                    m.Name.Contains("Revive") ||
                    m.Name.Contains("Respawn") ||
                    m.Name.Contains("Restore"));

                // Look for teleport methods
                _teleportMethod = methods.FirstOrDefault(m =>
                    m.Name.Contains("Teleport") ||
                    m.Name.Contains("SetPosition") ||
                    m.Name.Contains("MoveTo"));

                // Look for health property
                PropertyInfo[] properties = characterType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                _healthProperty = properties.FirstOrDefault(p =>
                    p.Name.Contains("Health") ||
                    p.Name.Contains("HP"));

                Plugin.Logger.LogInfo($"Found Kill Method: {_killMethod?.Name ?? "None"}");
                Plugin.Logger.LogInfo($"Found Revive Method: {_reviveMethod?.Name ?? "None"}");
                Plugin.Logger.LogInfo($"Found Teleport Method: {_teleportMethod?.Name ?? "None"}");
                Plugin.Logger.LogInfo($"Found Health Property: {_healthProperty?.Name ?? "None"}");

                _reflected = true;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to reflect Character methods: {ex.Message}");
            }
        }

        /// <summary>
        /// Kill a character (set to ghost/dead state)
        /// </summary>
        public static void KillCharacter(Character character)
        {
            if (character == null || !character.view.IsMine) return;

            try
            {
                // Try using CharacterAfflictions to set health to 0
                if (character.refs?.afflictions != null)
                {
                    // Set injury/death status
                    character.refs.afflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, 100f);
                    Plugin.Logger.LogInfo($"Set injury status to 100 for character");
                }

                // If we found a kill method via reflection, call it
                if (_killMethod != null)
                {
                    _killMethod.Invoke(character, null);
                    Plugin.Logger.LogInfo($"Called {_killMethod.Name} on character");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to kill character: {ex.Message}");
            }
        }

        /// <summary>
        /// Revive a character (restore to alive state)
        /// </summary>
        public static void ReviveCharacter(Character character)
        {
            if (character == null || !character.view.IsMine) return;

            try
            {
                // Clear all negative statuses
                if (character.refs?.afflictions != null)
                {
                    character.refs.afflictions.ClearAllStatus(false);
                    Plugin.Logger.LogInfo($"Cleared all statuses for character");
                }

                // If we found a revive method via reflection, call it
                if (_reviveMethod != null)
                {
                    _reviveMethod.Invoke(character, null);
                    Plugin.Logger.LogInfo($"Called {_reviveMethod.Name} on character");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to revive character: {ex.Message}");
            }
        }

        /// <summary>
        /// Teleport character to a position
        /// </summary>
        public static void TeleportCharacter(Character character, Vector3 position)
        {
            if (character == null || !character.view.IsMine) return;

            try
            {
                // Direct transform manipulation
                if (character.transform != null)
                {
                    character.transform.position = position;
                    Plugin.Logger.LogInfo($"Teleported character to {position}");
                }

                // If we found a teleport method, use it instead
                if (_teleportMethod != null)
                {
                    _teleportMethod.Invoke(character, new object[] { position });
                    Plugin.Logger.LogInfo($"Called {_teleportMethod.Name} on character");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to teleport character: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the local player's character
        /// </summary>
        public static Character GetLocalCharacter()
        {
            try
            {
                // Find the character that belongs to the local player
                foreach (var character in Character.AllCharacters)
                {
                    if (character != null && character.view != null && character.view.IsMine)
                    {
                        return character;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to get local character: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get a character by Photon player
        /// </summary>
        public static Character GetCharacterByPlayer(Photon.Realtime.Player player)
        {
            if (player == null) return null;

            try
            {
                foreach (var character in Character.AllCharacters)
                {
                    if (character != null && character.view != null &&
                        character.view.Owner != null &&
                        character.view.Owner.ActorNumber == player.ActorNumber)
                    {
                        return character;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to get character for player: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get the next campfire position for teleportation
        /// </summary>
        public static Vector3? GetNextCampfirePosition()
        {
            try
            {
                // Find all campfires in the scene
                var campfires = UnityEngine.Object.FindObjectsByType<Campfire>(FindObjectsSortMode.None);

                if (campfires != null && campfires.Length > 0)
                {
                    // Get the campfire with the highest segment (next biome)
                    var nextCampfire = campfires.OrderByDescending(c => c.advanceToSegment).FirstOrDefault();

                    if (nextCampfire != null)
                    {
                        // Return position near the campfire
                        return nextCampfire.transform.position + Vector3.up * 2f; // Spawn 2m above
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to get next campfire position: {ex.Message}");
            }

            return null;
        }
    }
}
