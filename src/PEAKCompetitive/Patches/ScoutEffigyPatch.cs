using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using PEAKCompetitive.Configuration;
using PEAKCompetitive.Model;
using PEAKCompetitive.Util;
using UnityEngine;

namespace PEAKCompetitive.Patches
{
    /// <summary>
    /// Patches ScoutEffigy to only revive teammates, not enemy players.
    /// </summary>
    [HarmonyPatch(typeof(ScoutEffigy), "FinishConstruction")]
    public class ScoutEffigyPatch
    {
        /// <summary>
        /// Prefix that replaces the FinishConstruction to only revive teammates.
        /// </summary>
        static bool Prefix(ScoutEffigy __instance, ref GameObject __result)
        {
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

            // Get private fields via reflection
            var constructingField = AccessTools.Field(typeof(Constructable), "constructing");
            var currentPreviewField = AccessTools.Field(typeof(Constructable), "currentPreview");
            var currentConstructHitField = AccessTools.Field(typeof(Constructable), "currentConstructHit");
            var itemField = AccessTools.Field(typeof(Constructable), "item");

            bool constructing = (bool)constructingField.GetValue(__instance);
            var currentPreview = currentPreviewField.GetValue(__instance);
            var currentConstructHit = (RaycastHit)currentConstructHitField.GetValue(__instance);
            var item = itemField.GetValue(__instance);

            if (!constructing)
            {
                __result = null;
                return false;
            }

            if (currentPreview == null)
            {
                __result = null;
                return false;
            }

            // Get the character using the effigy
            var holderCharacterField = AccessTools.Field(item.GetType(), "holderCharacter");
            Character userCharacter = (Character)holderCharacterField.GetValue(item);

            if (userCharacter == null || userCharacter.view?.Owner == null)
            {
                __result = null;
                return false;
            }

            // Get the user's team
            var userTeam = TeamManager.GetPlayerTeam(userCharacter.view.Owner);
            if (userTeam == null)
            {
                Plugin.Logger.LogWarning("ScoutEffigy user not on a team - allowing normal behavior");
                return true; // Let original run if user isn't on a team
            }

            // Find dead/passed out characters that are on the SAME TEAM
            List<Character> deadTeammates = new List<Character>();
            foreach (Character character in Character.AllCharacters)
            {
                if (character.data.dead || character.data.fullyPassedOut)
                {
                    // Check if this character is on the same team
                    var charOwner = character.view?.Owner;
                    if (charOwner != null)
                    {
                        var charTeam = TeamManager.GetPlayerTeam(charOwner);
                        if (charTeam != null && charTeam.TeamId == userTeam.TeamId)
                        {
                            deadTeammates.Add(character);
                        }
                    }
                }
            }

            if (deadTeammates.Count == 0)
            {
                Plugin.Logger.LogInfo("ScoutEffigy: No dead teammates to revive");
                __result = null;
                return false;
            }

            // Pick a random dead teammate and revive them
            Character toRevive = deadTeammates[UnityEngine.Random.Range(0, deadTeammates.Count)];

            toRevive.photonView.RPC("RPCA_ReviveAtPosition", RpcTarget.All, new object[]
            {
                currentConstructHit.point + Vector3.up * 1f,
                false
            });

            Plugin.Logger.LogInfo($"ScoutEffigy: Revived teammate {toRevive.view.Owner.ActorNumber}");

            __result = null;
            return false; // Skip original method
        }
    }
}
