using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ButtonEffects : MonoBehaviour
{
    UIManager UImanager;
    public string sceneName;
    [SerializeField] GameObject startButton, optionsButton, controlsButton;
    // Start is called before the first frame update
    void Start()
    {
        UImanager = FindAnyObjectByType<UIManager>().GetComponent<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonPressedTest()
    {

        Debug.Log("test");

    }

    public void ResumeButton()
    {
        UImanager.ResumeButton();
    }
    public void StartButton()//this should work but it doesnt and i dont know why
    {
        //SceneManager.LoadScene(sceneName);
    }

    public void SettingsButton()
    {

    }

    public void ControlsButton()
    {

    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
