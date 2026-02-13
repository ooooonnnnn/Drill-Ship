using UnityEngine;
using UnityEngine.UI;

public class FillBarUI : MonoBehaviour
{
    [SerializeField] private Gradient fillColor;
    [SerializeField, HideInInspector] private Slider fillBar;
    [SerializeField] private Image fillImage;

    private void OnValidate()
    {
        fillBar = GetComponent<Slider>();
    }

    /// <summary>
    /// Updates the fill of the fuel bar given a number from 0-1
    /// </summary>
    /// <param name="fill"></param>
    public void UpdateFill(float fill)
    {
        fillBar.value = fill;
        fillImage.color = fillColor.Evaluate(fill);
    }
}
