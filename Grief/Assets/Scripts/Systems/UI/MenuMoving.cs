using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuMoving : MonoBehaviour
{
    //to add more menu buttons just create new buttons in the scene don't worry about colors this code overrides any colors that were on the button prestart
    //this should continue to work even when time scale is set to 0

    // Daniel Try onPointer things

    public UserInterfaceInputControls inputcontrols;
    public Button[] buttons;
    public float verticalInput;
    public int indexNum;
    public ColorBlock defualt, highlight;
    public Vector2 Inputs;
    public bool canSwap;

    // Start is called before the first frame update
    void Awake()
    {
        inputcontrols = new UserInterfaceInputControls();
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(verticalInput);
        swapButtons();
        buttonColor();
    }

    void swapButtons()
    {
        if(verticalInput > 0.5 && canSwap)
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
        indexNum = (int)Mathf.Clamp(indexNum, 0f, buttons.Length-1);
        //Debug.Log(indexNum);
        //Debug.Log(canSwap);
    }

    void returnSwapping()
    {
        canSwap = true;
    }

    void buttonColor()
    {
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if(i == indexNum)
            {
                buttons[indexNum].colors = highlight;
            }
            else
            {
                buttons[i].colors = defualt; ;
            }
        }
          
    }

    void OnNavigation(InputValue value)
    {
        Inputs = value.Get<Vector2>();
        verticalInput = Inputs.y;
    }

    void OnSelection()
    {
        Debug.Log(indexNum);
        buttons[indexNum].colors.selectedColor.Equals(buttons[indexNum].colors.highlightedColor);
        buttons[indexNum].onClick.Invoke();
        
    }
}
