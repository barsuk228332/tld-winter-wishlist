using System;
using System.Collections.Generic;
using Il2CppRewired;

namespace WinterWishlist
{
    /// <summary>
    /// Глушит игровой ввод (Rewired), пока открыто окно мода.
    /// </summary>
    internal static class GameInput
    {
        private static readonly List<Player> Muted = new List<Player>();
        private static bool _errorLogged;

        internal static void SetBlocked(bool blocked)
        {
            try
            {
                var players = ReInput.players;
                if (players == null)
                    return;

                if (blocked)
                {
                    if (Muted.Count > 0)
                        return;

                    int count = players.allPlayerCount;
                    for (int i = 0; i < count; i++)
                    {
                        var player = players.GetPlayer(i);
                        if (player == null || !player.isPlaying)
                            continue;
                        player.isPlaying = false;
                        Muted.Add(player);
                    }
                }
                else
                {
                    foreach (var player in Muted)
                    {
                        if (player != null)
                            player.isPlaying = true;
                    }
                    Muted.Clear();
                }
            }
            catch (Exception e)
            {
                if (!_errorLogged)
                {
                    _errorLogged = true;
                    WinterWishlistMod.Log.Warning("Блокировка ввода: " + e.Message);
                }
            }
        }
    }
}
