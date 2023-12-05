using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private bool isPaused;

    public bool IsPaused { get { return isPaused; } }

    private void Start()
    {
        Time.timeScale = 1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Time.timeScale = Mathf.Min(1, Time.timeScale + 0.1f);
            //Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Time.timeScale = Mathf.Max(0.1f, Time.timeScale - 0.1f);
            //Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    public void PauseGame()
    {
        if (isPaused)
        {
            return;
        }

        isPaused = true;
        TempUI.Instance.PauseGame();
        //EnemySpawning.Instance.PauseSpawning();
        Time.timeScale = 0;
    }

    public void UnPauseGame()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        TempUI.Instance.UnPauseGame();
        //EnemySpawning.Instance.UnPauseSpawning();
        Time.timeScale = 1;
    }
}