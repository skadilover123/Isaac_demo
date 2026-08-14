using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 游戏管理器：单例。负责刷新血条、统计击杀数与总目标、判定胜负、处理重启与退出。
/// 敌人死亡时调用 OnEnemyKilled 增加击杀数，击杀数达到总目标（含未生成的）即胜利；
/// 玩家死亡显示失败面板；游戏结束后调用玩家 StopControl 停止角色行动。
/// </summary>
public class GameManager : MonoBehaviour
{
    // 单例访问入口（供 Enemy 等脚本调用）
    public static GameManager Instance;

    // ===================== 玩家 =====================
    [Header("玩家")]
    [SerializeField] private Player player;   // 玩家组件（读取血量与死亡状态）

    // ===================== UI =====================
    [Header("UI")]
    [SerializeField] private Image healthFill;              // 血条填充图
    [SerializeField] private GameObject gameOverPanel;     // 失败面板
    [SerializeField] private GameObject victoryPanel;      // 胜利面板
    [SerializeField] private TextMeshProUGUI killText;     // 击杀数显示（已击杀 / 总目标）

    private CanvasGroup gameOverCG;   // 失败面板的透明度控制器
    private CanvasGroup victoryCG;    // 胜利面板的透明度控制器
    private int totalCount;           // 本局总怪物数（含未生成的）
    private int killedCount;          // 已击杀怪物数
    private bool isOver;              // 游戏是否已结束（防止重复判定）
    private AudioSource music;        // 背景音乐

    private void Awake()
    {
        music = transform.GetComponent<AudioSource>();

        // 单例：重复实例直接销毁
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

        // 开局统计场景中预置的敌人（生成器生成的总数会额外累加）
        totalCount = FindObjectsOfType<Enemy>().Length;
        killedCount = 0;
        RefreshKillText();
    }

    private void Update()
    {
        // 快捷键：R 重开本关，Esc 返回主菜单
        if (Input.GetKeyDown(KeyCode.R)) Restart();
        if (Input.GetKeyDown(KeyCode.Escape)) Quit();

        // 游戏已结束后不再刷新逻辑
        if (isOver) return;

        // 刷新血条
        float ratio = (float)player.Hp / (float)player.MaxHp;
        healthFill.fillAmount = ratio;

        // 玩家死亡 → 失败
        if (player.IsDead)
        {
            ShowGameOver();
            return;
        }

        // 击杀数达到总目标 → 胜利（兜底判定，正常情况下在 OnEnemyKilled 里已触发）
        if (totalCount > 0 && killedCount >= totalCount)
        {
            ShowVictory();
        }
    }

    /// <summary>敌人死亡时调用：击杀数加一，刷新显示，杀满总目标则胜利</summary>
    public void OnEnemyKilled()
    {
        killedCount++;
        RefreshKillText();

        if (totalCount > 0 && killedCount >= totalCount)
        {
            ShowVictory();
        }
    }

    /// <summary>生成器开局调用：累加本局总怪物数（含未生成的），并刷新显示</summary>
    public void AddTotalEnemies(int amount)
    {
        totalCount += amount;
        RefreshKillText();
    }

    /// <summary>刷新击杀数文本：Killed x / y（项目字体不含中文字形，用英文显示）</summary>
    private void RefreshKillText()
    {
        if (killText == null) return;
        killText.text = "Killed " + killedCount + " / " + totalCount;
    }

    /// <summary>显示失败面板并停止玩家控制</summary>
    private void ShowGameOver()
    {
        if (isOver) return;
        isOver = true;
        gameOverCG.alpha = 1f;
        if (player != null) player.StopControl();
        music.Stop();
    }

    /// <summary>显示胜利面板并停止玩家控制</summary>
    private void ShowVictory()
    {
        if (isOver) return;
        isOver = true;
        victoryCG.alpha = 1f;
        if (player != null) player.StopControl();
        music.Stop();
    }

    /// <summary>重开当前关卡</summary>
    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>返回主菜单（场景 0）</summary>
    private void Quit()
    {
        SceneManager.LoadScene(0);
    }
}
