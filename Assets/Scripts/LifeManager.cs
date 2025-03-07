using UnityEngine;

public class LifeManager : MonoBehaviour
{
    [Header("Life Settings")]
    public int life = 3;

    public void TakeDamage(int damage = 1)
    {
        life -= damage;
    }

    public delegate void OnAction();
    public event OnAction OnLifesOver;

}