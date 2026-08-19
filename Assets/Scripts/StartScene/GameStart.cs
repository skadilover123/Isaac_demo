using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject exitPanel;
    [SerializeField] private AudioSource Audio;
    [SerializeField] private AudioSource startAudio;
    private CanvasGroup startCG;
    private CanvasGroup exitCG;
    private bool isExitPanelActive = false;
    void Awake()
    {
        startCG = startPanel.GetComponent<CanvasGroup>();
        exitCG = exitPanel.GetComponent<CanvasGroup>();
        startCG.alpha = 1;
        exitCG.alpha = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Audio.Stop();
            startAudio.Play();
            SceneManager.LoadScene(1);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isExitPanelActive = !isExitPanelActive;
            exitCG.alpha = isExitPanelActive ? 1 : 0;
        }
        if (isExitPanelActive)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Application.Quit();
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                isExitPanelActive = false;
                exitCG.alpha = 0;
            }
        }
    }
}
