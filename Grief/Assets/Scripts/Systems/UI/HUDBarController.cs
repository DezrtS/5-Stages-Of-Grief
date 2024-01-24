using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class HUDBarController : MonoBehaviour
{
    [SerializeField] GameObject hitPointBar, abilityBar;
    public float hitPointNum, abilityNum;

    // Start is called before the first frame update
    void Start()
    {

        PlayerController.Instance.OnAttackEvent += OnAttackEvent;
        //hitPointNum = PlayerController.Instance.Health;
        UpdateHPBar();
        abilityNum = 1;//if we decide to have multipule ability charges this will need to be changed to find the max ability uses
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHPBar();
        UpdateAbilityBar();
    }

    public void OnAttackEvent(string attackId)
    {
        var IdAttack = attackId;
        if (IdAttack == "ice_shard")
        {
            abilityNum--;
            //abilityBar.SetActive(false);
            Invoke("AbilityCanUse",0.75f);//this timer has to be set manualy this should be changed so that it can find the cooldown on its own
            //Debug.Log("is ice shard");
        }
    }
    private void AbilityCanUse()
    {
        abilityNum++;
        //abilityBar.SetActive(true);
    }
    private void UpdateHPBar()
    {
        hitPointNum = PlayerController.Instance.Health;
        hitPointBar.gameObject.transform.localScale = new Vector3(hitPointNum,1,1);
        //Debug.Log("hp=" + hitPointNum);
    }
    private void UpdateAbilityBar()
    {
        abilityBar.gameObject.transform.localScale = new Vector3(abilityNum, 1, 1);
    }
}
