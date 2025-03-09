using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Variables")]
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI lifeText;

    [Header("References")]
    public BuildingManager buildingManager;
    public GameObject countdownPanel;
    public GameObject mainPanel;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Turret Settings")]
    public Button buildButton;
    public Button cancelButton;
    public GameObject buildingPanel;

    [Header("Turret Types")]
    public Button turret1Button;
    public Button turret2Button;
    public Button turret3Button;

    private EnemyWaveManager enemyWaveManager;
    private PlayerManager playerManager;

    public static event Action OnGameStart;

    void OnEnable()
    {
        PlayerManager.OnGameOver += GameLose;
    }

    void OnDisable()
    {
        PlayerManager.OnGameOver -= GameLose;
    }

    void Start()
    {
        enemyWaveManager = FindAnyObjectByType<EnemyWaveManager>();
        playerManager = FindAnyObjectByType<PlayerManager>();

        turret1Button.onClick.AddListener(() => SelectTurret(1));
        turret2Button.onClick.AddListener(() => SelectTurret(2));
        turret3Button.onClick.AddListener(() => SelectTurret(3));
    }

    void Update()
    {
        Countdown();
        UpdateHealthText();
    }

    public void SelectTurret(int turretType)
    {
        buildingManager.SelectTurret(turretType);
    }

    public void CancelBuilding()
    {
        buildingManager.CancelBuildMode();
    }

    private void GameLose()
    {
        mainPanel.SetActive(false);
        countdownPanel.SetActive(false);
        losePanel.SetActive(true);
    }

    private void GameWin()
    {
        mainPanel.SetActive(false);
        countdownPanel.SetActive(false);
        winPanel.SetActive(true);
    }

    private void UpdateHealthText()
    {
        lifeText.text = playerManager.life.ToString("F0");
    }

    private void Countdown()
    {
        countdownText.text = enemyWaveManager.waveTimer.ToString("F0");
        if(enemyWaveManager.waveTimer <= 0)
        {

            countdownText.gameObject.SetActive(false);
            countdownPanel.SetActive(false);
        }
    }

    public void RestartScene()
    {
        PlayerManager.IsGamePaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    } 

    public void StartGame(){
        OnGameStart?.Invoke();
    }

    public void QuitGame(){
        Application.Quit();
    }
}
