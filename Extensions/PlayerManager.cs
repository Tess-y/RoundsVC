using System;
using System.Linq;
using UnityEngine;
namespace RoundsVC
{
    static class PlayerManagerExtensions
    {
        public static Player GetPlayerWithID(this PlayerManager instance, int playerID)
        {
            return instance.players.FirstOrDefault(p => p.playerID == playerID);
        }
        public static Player GetLocalPlayer(this PlayerManager instance)
        {
            return instance.players.FirstOrDefault(p => p.data.view.IsMine);
        }
        public static Player GetPlayerWithActorID(this PlayerManager instance, int actorID)
        {
            return instance.players.FirstOrDefault(p => p.data.view.Owner.ActorNumber == actorID);
        }
        public static Player GetClosestOtherPlayer(this PlayerManager instance, Player thisPlayer, bool requireAlive = true, bool requireLoS = false)
        {
            Player closest = null;
            float dist = float.PositiveInfinity;
            foreach (Player player in instance.players)
            {
                if (player.playerID == thisPlayer.playerID) { continue; }
                if (requireAlive && player.data.dead) { continue; }
                if (requireLoS && !instance.CanSeePlayer(thisPlayer.data.playerVel.position, player).canSee) { continue; }
                if (Vector2.Distance(player.data.playerVel.position, thisPlayer.data.playerVel.position) < dist) { closest = player; }
            }
            return closest;
        }
        public static void ForEachPlayer(this PlayerManager instance, Action<Player> action)
        {
            foreach (Player player in instance.players)
            {
                action(player);
            }
        }
        public static void ForEachAlivePlayer(this PlayerManager instance, Action<Player> action)
        {
            foreach (Player player in instance.players.Where(p => !p.data.dead))
            {
                action(player);
            }
        }
        public static void ForEachDeadPlayer(this PlayerManager instance, Action<Player> action)
        {
            foreach (Player player in instance.players.Where(p => p.data.dead))
            {
                action(player);
            }
        }
    }
}
