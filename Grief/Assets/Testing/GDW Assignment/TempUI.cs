using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempUI : Singleton<TempUI>
{

    [SerializeField] private GameObject pauseMenu;

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
    }

    public void UnPauseGame()
    {
        pauseMenu.SetActive(false);
    }
}
