using System;
using UnityEngine;

public static class GameEvents
{



    #region player respawn related

    public static Action OnPlayerDeath;
    /// <summary>
    /// transform = player's <see cref="Transform"/>
    /// </summary>
    public static Action<Transform> OnPlayerRespawn;

    #endregion



}