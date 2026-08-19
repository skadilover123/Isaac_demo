using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    [Header("玩家")]
    [SerializeField] private Player player;

    [Header("UI")]
    [SerializeField] private Image healthFill;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI killText;

    private CanvasGroup gameOverCG;
    private CanvasGroup victoryCG;
    private int totalCount;
    private int killedCount;
    private bool isOver;
    private AudioSource music;

    private void Awake()
    {
        music = transform.GetComponent<AudioSource>();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        gameOverCG = gameOverPanel.GetComponent<CanvasGroup>();
        victoryCG = victoryPanel.GetComponent<CanvasGroup>();
        gameOverCG.alpha = 0f;
        victoryCG.alpha = 0f;

        totalCount = FindObjectsOfType<Enemy>().Length;
        killedCount = 0;
        RefreshKillText();
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.R)) Restart();
        if (Input.GetKeyDown(KeyCode.Escape)) Quit();

        if (isOver) return;

        float ratio = (float)player.Hp / (float)player.MaxHp;
        healthFill.fillAmount = ratio;

        if (player.IsDead)
        {
            ShowGameOver();
            return;
        }

        if (totalCount > 0 && killedCount >= totalCount)
        {
            ShowVictory();
        }
    }

    public void OnEnemyKilled()
    {
        killedCount++;
        RefreshKillText();

        if (totalCount > 0 && killedCount >= totalCount)
        {
            ShowVictory();
        }
    }

    public void AddTotalEnemies(int amount)
    {
        totalCount += amount;
        RefreshKillText();
    }

    private void RefreshKillText()
    {
        if (killText == null) return;
        killText.text = "Killed " + killedCount + " / " + totalCount;
    }

    private void ShowGameOver()
    {
        if (isOver) return;
        isOver = true;
        gameOverCG.alpha = 1f;
        if (player != null) player.StopControl();
        music.Stop();
    }

    private void ShowVictory()
    {
        if (isOver) return;
        isOver = true;
        victoryCG.alpha = 1f;
        if (player != null) player.StopControl();
        music.Stop();
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Quit() => SceneManager.LoadScene(0);
}
