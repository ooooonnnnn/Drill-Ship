using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class FractionTextController : MonoBehaviour
{
    [SerializeField] private TMP_Text topText, bottomText;

    public void UpdateTexts(float topNum, float bottomNum)
    {
        topText.text = topNum.ToString("F0");
        bottomText.text = bottomNum.ToString("F0");
    }
}
