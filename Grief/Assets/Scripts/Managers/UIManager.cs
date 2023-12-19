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

    }

    public void GetClosestAngledButton()
    {
        
    }
}
