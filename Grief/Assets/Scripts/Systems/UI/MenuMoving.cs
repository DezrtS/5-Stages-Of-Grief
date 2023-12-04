using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuMoving : MonoBehaviour
{
    //to add more menu buttons just create new buttons in the scene don't worry about colors this code overrides any colors that were on the button prestart
    //this should continue to work even when time scale is set to 0


    public UserInterfaceInputControls inputcontrols;
    public Button[] buttons;
    public float horizontalInput;
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
        //Debug.Log(horizontalInput);
        swapButtons();
        buttonColor();
    }

    void swapButtons()
    {
        if(horizontalInput > 0.5 && canSwap)
        {
            indexNum--;
            canSwap = false;
            Invoke("returnSwapping", Time.unscaledDeltaTime + 0.5f);
        }
        if (horizontalInput < -0.5 && canSwap)
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
        horizontalInput = Inputs.y;
    }

    void OnSelection()
    {
        Debug.Log(indexNum);
        buttons[indexNum].colors.selectedColor.Equals(buttons[indexNum].colors.highlightedColor);
        buttons[indexNum].onClick.Invoke();
        
    }
}
