using HarmonyLib;
using UnityEngine;
using PEAKCompetitive.Configuration;
using PEAKCompetitive.Util;
using PEAKCompetitive.Model;

namespace PEAKCompetitive.Patches
{
    /// <summary>
    /// Patches the grab/reach mechanic to push enemy players instead of pulling them.
    /// Teammates still get pulled normally.
    /// </summary>
    [HarmonyPatch(typeof(CharacterGrabbing), "Reach")]
    public class PvPGrabPatch
    {
        /// <summary>
        /// Prefix that replaces the entire Reach method to add team-based behavior.
        /// Returns false to skip original method.
        /// </summary>
        static bool Prefix(CharacterGrabbing __instance)
        {
            // If PvP is disabled, let original method run
            if (!ConfigurationHandler.EnablePvP)
            {
                return true;
            }

            // If competitive mode is disabled, let original method run
            if (!ConfigurationHandler.EnableCompetitiveMode)
            {
                return true;
            }

            // If no match is active, let original method run
            if (!MatchState.Instance.IsMatchActive)
            {
                return true;
            }

            // Get the character doing the reaching via reflection
            var characterField = AccessTools.Field(typeof(CharacterGrabbing), "character");
            Character reachingCharacter = (Character)characterField.GetValue(__instance);

            if (reachingCharacter == null) return true;

            // Get the reaching player's team
            var reachingPlayer = reachingCharacter.view?.Owner;
            if (reachingPlayer == null) return true;

            var reachingTeam = TeamManager.GetPlayerTeam(reachingPlayer);

            // Process all characters
            foreach (Character targetCharacter in Character.AllCharacters)
            {
                float distance = Vector3.Distance(reachingCharacter.Center, targetCharacter.Center);

                // Check distance and angle (same as original)
                if (distance > 4f) continue;
                if (Vector3.Angle(reachingCharacter.data.lookDirection,
                    targetCharacter.Center - reachingCharacter.Center) > 60f) continue;

                // Check if target can be helped (use reflection to call private method)
                var canHelpMethod = AccessTools.Method(typeof(CharacterGrabbing), "TargetCanBeHelped");
                bool canBeHelped = (bool)canHelpMethod.Invoke(__instance, new object[] { targetCharacter });

                if (!canBeHelped) continue;

                // Handle stuck/webbed characters (same as original)
                if (targetCharacter.IsStuck() && targetCharacter.IsLocal)
                {
                    targetCharacter.UnStick();
                }
                targetCharacter.refs.afflictions.SubtractStatus(CharacterAfflictions.STATUSTYPE.Web, 1f, false, false);

                // Update grab distance tracking
                if (distance < reachingCharacter.data.grabFriendDistance)
                {
                    reachingCharacter.data.grabFriendDistance = distance;
                    reachingCharacter.data.sinceGrabFriend = 0f;
                }

                // Show grasp UI for reaching player
                if (reachingCharacter.refs.view.IsMine)
                {
                    GUIManager.instance.Grasp();
                }

                // This is the key part - check if target is on same team or enemy team
                if (targetCharacter.refs.view.IsMine)
                {
                    var targetPlayer = targetCharacter.view?.Owner;
                    var targetTeam = targetPlayer != null ? TeamManager.GetPlayerTeam(targetPlayer) : null;

                    bool isSameTeam = (reachingTeam != null && targetTeam != null &&
                                       reachingTeam.TeamId == targetTeam.TeamId);

                    if (isSameTeam)
                    {
                        // TEAMMATE: Pull them towards you (normal behavior)
                        targetCharacter.DragTowards(reachingCharacter.Center, 70f);
                        targetCharacter.LimitFalling();
                        GUIManager.instance.Grasp();
                    }
                    else
                    {
                        // ENEMY: Push them away!
                        Vector3 pushDirection = (targetCharacter.Center - reachingCharacter.Center).normalized;
                        float pushForce = ConfigurationHandler.PushForce;

                        // Apply force away from the reaching player
                        targetCharacter.AddForce(pushDirection * pushForce, 0.5f, 1f);

                        Plugin.Logger.LogInfo($"PvP Push! Pushed enemy player with force {pushForce}");
                    }
                }
            }

            // Skip original method - we handled everything
            return false;
        }
    }
}
