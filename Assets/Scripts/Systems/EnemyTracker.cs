using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EnemyTracker
{
    private static int _activeEnemies;
    public static int ActiveEnemies => _activeEnemies;

    public static event Action AllEnemiesCleared;

    static EnemyTracker()
    {
        SceneManager.sceneLoaded += (_, __) => Reset();
    }

    public static void Reset()
    {
        _activeEnemies = 0;
    }

    public static void Register()
    {
        _activeEnemies++;
    }

    public static void Unregister()
    {
        if (_activeEnemies > 0)
            _activeEnemies--;

        if (_activeEnemies == 0)
            AllEnemiesCleared?.Invoke();
    }
}