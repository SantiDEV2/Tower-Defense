using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Life Settings")]
    public int life = 3;                       // Cantidad inicial de vidas
    public static event Action OnGameOver;     // Evento que se dispara cuando el juego termina
    public static bool IsGamePaused = false;   // Variable para controlar si el juego está pausado

    [Header("Currency Settings")]
    public int startingCurrency = 100;         // Cantidad inicial de monedas
    public int[] turretCosts = { 50, 100, 200 }; // Costos de cada tipo de torreta
    private int currentCurrency;               // Monedas actuales del jugador

    // Evento que notifica cambios en la moneda
    public static event Action<int> OnCurrencyChanged;

    void Start()
    {
        // Inicializa la moneda y notifica a los listeners
        currentCurrency = startingCurrency;
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    // Método para agregar monedas al jugador
    public void AddCurrency(int amount)
    {      
        currentCurrency += amount;
        OnCurrencyChanged?.Invoke(currentCurrency); // Notifica del cambio
    }

    // Método para gastar monedas
    public bool SpendCurrency(int amount)
    {   
        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            OnCurrencyChanged?.Invoke(currentCurrency);
            return true;
        }
        
        return false; // No hay suficientes monedas
    }
    
    // Comprueba si el jugador puede pagar una torreta específica
    public bool CanAfford(int turretIndex) => 
        turretIndex >= 0 && turretIndex < turretCosts.Length && currentCurrency >= turretCosts[turretIndex];
    
    // Devuelve la cantidad actual de monedas
    public int GetCurrency() => currentCurrency;
    
    // Obtiene el costo de una torreta específica
    public int GetTurretCost(int turretIndex) => 
        (turretIndex >= 0 && turretIndex < turretCosts.Length) ? turretCosts[turretIndex] : 0;

    // Método para reducir la vida del jugador y verificar Game Over
    public void TakeDamage(int damage = 1)
    {
        life -= damage;
        if(life <= 0)
        {
            OnGameOver?.Invoke();  // Dispara el evento de fin de juego
            IsGamePaused = true;   // Pausa el juego cuando se pierde
        }
    }
}