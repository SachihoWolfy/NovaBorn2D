using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class HeaderInfo : MonoBehaviourPun
{
    public TextMeshProUGUI nameText;
    public Slider healthBar;
    public Slider shieldBar;
    private float maxValue;
    public void Initialize(string text, int maxVal)
    {
        nameText.text = text;
        maxValue = maxVal;
        healthBar.maxValue = maxVal;
        healthBar.value = 100;
        shieldBar.maxValue = maxVal;
        shieldBar.value = 0;
    }
    [PunRPC]
    public void UpdateHealthBar(int curHP)
    {
        healthBar.value = curHP;
    }
    [PunRPC]
    public void UpdateShieldBar(int curSP)
    {
        shieldBar.value = curSP;
    }
}
