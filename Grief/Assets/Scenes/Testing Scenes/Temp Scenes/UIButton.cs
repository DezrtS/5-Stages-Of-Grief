using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    [SerializeField] private int sceneToLoad = 0;

    public void Quit()
    {
        SceneManager.Instance.Quit();
    }

    public void Resume()
    {
        GameManager.Instance.UnPauseGame();
    }

    public void LoadScene()
    {
        SceneManager.Instance.LoadScene(sceneToLoad);
    }
}
