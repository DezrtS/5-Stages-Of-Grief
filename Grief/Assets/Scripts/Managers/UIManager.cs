using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    private UserInterfaceInputControls userInterfaceInputControls;
    private InputAction navigationAction;

    public UserInterfaceInputControls inputcontrols;
    public Button[] buttons;
    public float verticalInput;
    public int indexNum;
    [SerializeField] Sprite button1, button2;
    //public ColorBlock defualt, highlight; 
    public bool canSwap;

    [SerializeField] private bool canPause = true;
    private bool isPaused;

    public bool IsPaused { get { return isPaused; } }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        userInterfaceInputControls ??= new UserInterfaceInputControls();

        userInterfaceInputControls.UI.Pause.performed += OnPause;
        userInterfaceInputControls.UI.Pause.Enable();

        navigationAction = userInterfaceInputControls.UI.Navigation;
        navigationAction.performed += OnNavigation;

        userInterfaceInputControls.UI.QuickTabNavigation.performed += OnQuickTabNavigation;
        userInterfaceInputControls.UI.Selection.performed += OnSelection;
        userInterfaceInputControls.UI.Deselection.performed += OnDeselection;
    }

    public void OnPause(InputAction.CallbackContext obj)
    {
        if (!canPause)
        {
            return;
        }

        if (isPaused)
        {
            isPaused = false;
            GameManager.Instance.UnPauseGame();
            DisableUserInterfaceInput();
        }
        else
        {
            isPaused = true;
            GameManager.Instance.PauseGame();
            EnableUserInterfaceInput();
        }
    }

    public void OnNavigation(InputAction.CallbackContext obj)
    {
        Vector2 navigationInput = navigationAction.ReadValue<Vector2>();
        verticalInput = navigationInput.y;
        swapButtons();
    }

    public void OnQuickTabNavigation(InputAction.CallbackContext obj)
    {

    }

    public void OnSelection(InputAction.CallbackContext obj)
    {

    }

    public void OnDeselection(InputAction.CallbackContext obj)
    {

    }

    private void OnDisable()
    {
        userInterfaceInputControls.UI.Pause.Disable();

        DisableUserInterfaceInput();
    }

    private void EnableUserInterfaceInput()
    {
        navigationAction.Enable();

        userInterfaceInputControls.UI.QuickTabNavigation.Enable();
        userInterfaceInputControls.UI.Selection.Enable();
        userInterfaceInputControls.UI.Deselection.Enable();
    }

    private void DisableUserInterfaceInput()
    {
        navigationAction.Disable();

        userInterfaceInputControls.UI.QuickTabNavigation.Disable();
        userInterfaceInputControls.UI.Selection.Disable();
        userInterfaceInputControls.UI.Deselection.Disable();
    }

    public void TransferToButton()
    {
        Debug.Log(indexNum);
        buttons[indexNum].colors.selectedColor.Equals(buttons[indexNum].colors.highlightedColor);
        buttons[indexNum].onClick.Invoke();
    }

    public void GetClosestAngledButton()
    {

    }

    private void Update()
    {
        //swapButtons(); 
        buttonColor();
        CheckCancelInvoke();
    }
    void swapButtons()
    {
        if (verticalInput > 0.5 && canSwap)
        {
            indexNum--;
            canSwap = false;
            Invoke("returnSwapping", Time.unscaledDeltaTime + 0.5f);
        }
        if (verticalInput < -0.5 && canSwap)
        {
            indexNum++;
            canSwap = false;
            Invoke("returnSwapping", Time.unscaledDeltaTime + 0.5f);
        }
        indexNum = (int)Mathf.Clamp(indexNum, 0f, buttons.Length - 1);
        //Debug.Log(indexNum); 
        //Debug.Log(canSwap); 
    }
    void CheckCancelInvoke()
    {
        if (verticalInput > -0.5 && verticalInput < 0.5 && !canSwap)
        {
            CancelInvoke();
            returnSwapping();
            //Debug.Log("canceled"); 
        }
    }

    void returnSwapping()
    {
        canSwap = true;
    }

    void buttonColor()
    {

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == indexNum)
            {
                buttons[indexNum].Select();
                //.colors = highlight; 
            }
            else
            {
                //buttons[i].Select(); 
                //.colors = defualt; ; 
            }
        }

    }

}