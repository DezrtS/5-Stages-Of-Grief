using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
    private float healthBarValue = 0;

    [SerializeField] private TextMeshProUGUI healthBarText;

    private void Start()
    {
        PlayerController.Instance.OnPlayerHealthEvent += OnPlayerHealthEvent;
        healthBarValue = PlayerController.Instance.Health;
        UpdateHealthBar();
    }

    public void OnPlayerHealthEvent(float health)
    {
        healthBarValue = health;
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        if (healthBarText == null)
        {
            return;
        }

        healthBarText.text = "Health: " + healthBarValue.ToString();
    }

    private void OnDisable()
    {
        PlayerController.Instance.OnPlayerHealthEvent -= OnPlayerHealthEvent;
    }
}
