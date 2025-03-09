using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI countdownText;  // Texto de la cuenta atrás entre oleadas
    public TextMeshProUGUI lifeText;       // Muestra las vidas actuales
    public TextMeshProUGUI currencyText;   // Muestra las monedas actuales

    [Header("Panels")]
    public GameObject countdownPanel;      // Panel de cuenta atrás entre oleadas
    public GameObject mainPanel;           // Panel principal del juego
    public GameObject winPanel;            // Panel de victoria
    public GameObject losePanel;           // Panel de derrota

    [Header("Turret Buttons")]
    public Button turret0Button;           // Botón para torreta tipo 0
    public Button turret1Button;           // Botón para torreta tipo 1
    public Button turret2Button;           // Botón para torreta tipo 2

    [Header("References")]
    public BuildingManager buildingManager; // Gestor de construcción

    private EnemyWaveManager enemyWaveManager; // Referencia al gestor de oleadas de enemigos
    private PlayerManager playerManager;       // Referencia al gestor del jugador
    public static event Action OnGameStart;    // Evento para iniciar el juego

    // Suscripción a eventos
    void OnEnable()
    {
        PlayerManager.OnCurrencyChanged += UpdateCurrencyUI;   // Actualiza UI cuando cambia la moneda
        PlayerManager.OnGameOver += GameLose;                  // Maneja el evento de fin de juego
        EnemyWaveManager.OnWaveCompleted += ResetCountdownUI;  // Resetea la UI de cuenta atrás al terminar oleada
        EnemyWaveManager.OnGameWin += GameWin;                 // Maneja el evento de victoria
    }

    void OnDisable()
    {
        PlayerManager.OnCurrencyChanged -= UpdateCurrencyUI;
        PlayerManager.OnGameOver -= GameLose;
        EnemyWaveManager.OnWaveCompleted -= ResetCountdownUI;
        EnemyWaveManager.OnGameWin -= GameWin;
    }

    void Start()
    {
        enemyWaveManager = FindAnyObjectByType<EnemyWaveManager>();
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    void Update()
    {
        // Actualiza elementos de UI que requieren actualización constante
        UpdateCountdown();
        UpdateHealthText();
    }

    // Resetea la UI de cuenta atrás entre oleadas
    private void ResetCountdownUI()
    {
        countdownText.gameObject.SetActive(true);
        countdownPanel.SetActive(true);
    }

    // Selecciona un tipo de torreta para construir
    public void SelectTurret(int turretType) => buildingManager.SelectTurret(turretType);

    // Actualiza la UI de monedas y la disponibilidad de botones de torretas
    private void UpdateCurrencyUI(int amount)
    {
        currencyText.text = amount.ToString();

        // Habilita/deshabilita botones según si el jugador puede pagar cada torreta
        if (playerManager != null)
        {
            turret0Button.interactable = playerManager.CanAfford(0);
            turret1Button.interactable = playerManager.CanAfford(1);
            turret2Button.interactable = playerManager.CanAfford(2);
        }
    }

    // Muestra el panel de derrota
    private void GameLose()
    {
        mainPanel.SetActive(false);
        countdownPanel.SetActive(false);
        losePanel.SetActive(true);
    }

    // Muestra el panel de victoria
    private void GameWin()
    {
        mainPanel.SetActive(false);
        countdownPanel.SetActive(false);
        winPanel.SetActive(true);
    }

    // Actualiza el texto de vidas
    private void UpdateHealthText() => lifeText.text = playerManager.life.ToString();

    // Actualiza el texto de cuenta atrás y oculta el panel cuando llega a 0
    private void UpdateCountdown()
    {
        countdownText.text = enemyWaveManager.waveTimer.ToString("F0");
        if (enemyWaveManager.waveTimer <= 0)
        {
            countdownText.gameObject.SetActive(false);
            countdownPanel.SetActive(false);
        }
    }

    // Reinicia el nivel actual
    public void RestartScene()
    {
        PlayerManager.IsGamePaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Inicia el juego disparando el evento correspondiente
    public void StartGame() => OnGameStart?.Invoke();

    // Sale del juego
    public void QuitGame() => Application.Quit();
}