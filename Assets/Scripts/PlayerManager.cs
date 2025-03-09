using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Life Settings")]
    public int life = 3;
    public static event Action OnGameOver;
    public static bool IsGamePaused = false;

    public void TakeDamage(int damage = 1)
    {
        life -= damage;
        if(life <= 0)
        {
            OnGameOver?.Invoke();
            IsGamePaused = true;
        }
    }
}
