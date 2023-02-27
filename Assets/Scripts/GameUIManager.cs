using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [SerializeField]
    private Image jumpBase;
    [SerializeField]
    private Image jumpBar;
    [SerializeField]
    private Image jumpCDBase;
    [SerializeField]
    private Image jumpCDCover;
    [SerializeField]
    PlayerComponents playerComponents;

    public void UpdateJump(float ratio)
    {
        jumpBar.fillAmount = ratio;
    }

    public void UpdateJumpCD(float ratio)
    {
        jumpCDCover.fillAmount = ratio;
    }
}
