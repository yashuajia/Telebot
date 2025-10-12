
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 管理游戏暂停、退出和UI面板切换
/// </summary>
public class ApplicationController : MonoBehaviour
{
    [Header("Pause Settings")]
    [Tooltip("按Esc键是否暂停游戏")]
    [SerializeField] private bool pauseGameOnEscape = true;

    [Header("UI Panel References")]
    [Tooltip("拖入你的 Pause Menu Panel")]
    [SerializeField] private GameObject pauseMenuPanel;
    
    [Tooltip("拖入你的 Main Menu Panel")]
    [SerializeField] private GameObject mainMenuPanel;
    
    [Tooltip("拖入你的 Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Scene Settings")]
    [Tooltip("主菜单场景的名字")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // 静态属性，可以在其他脚本中访问
    public static bool GameIsPaused { get; private set; } = false;

    private void Start()
    {
        // 确保游戏开始时是正常状态
        Time.timeScale = 1f;
        GameIsPaused = false;

        // 初始化：关闭所有菜单
        HideAllPanels();
        ShowMainMenuPanel();
        
    }

    private void Update()
    {
        // 按Esc键切换暂停菜单
        if (Input.GetKeyDown(KeyCode.Escape) && pauseGameOnEscape)
        {
            if (GameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                ShowPauseMenu();
            }
        }
    }

    // ============= 暂停菜单相关 =============
    
    /// <summary>
    /// 显示暂停菜单（可以连接到Button）
    /// </summary>
    public void ShowPauseMenu()
    {
        HideAllPanels();
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        
        PauseGame(true);
    }

    /// <summary>
    /// 恢复游戏（连接到Resume按钮）
    /// </summary>
    public void ResumeGame()
    {
        HideAllPanels();
        PauseGame(false);
    }

    // ============= 面板切换 =============

    /// <summary>
    /// 显示主菜单面板（连接到Main Menu按钮）
    /// </summary>
    public void ShowMainMenuPanel()
    {
        HideAllPanels();
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        
        // 主菜单面板显示时保持暂停状态
        PauseGame(true);
    }

    /// <summary>
    /// 显示设置面板（连接到Settings按钮）
    /// </summary>
    public void ShowSettingsPanel()
    {
        HideAllPanels();
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        
        // 设置面板显示时保持暂停状态
        PauseGame(true);
    }

    /// <summary>
    /// 从设置或主菜单返回暂停菜单（连接到Back按钮）
    /// </summary>
    public void BackToPauseMenu()
    {
        ShowPauseMenu();
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    private void HideAllPanels()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ============= 场景加载 =============


    /// <summary>
    /// 重新加载当前场景（连接到Restart按钮）
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ============= 游戏控制 =============

    /// <summary>
    /// 暂停或恢复游戏
    /// </summary>
    public void PauseGame(bool paused)
    {
        GameIsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    /// <summary>
    /// 退出游戏（连接到Quit按钮）
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Game quit! Exiting play mode...");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Debug.Log("Game quit! Closing application...");
        Application.Quit();
#endif
    }
}