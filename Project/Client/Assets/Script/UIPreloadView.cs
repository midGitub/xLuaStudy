
using UnityEngine;
using UnityEngine.UI;

public class UIPreloadView : MonoBehaviour
{
    public Slider slider;
    public Text sliderText;

    public void Refresh(float sliderValue, string sliderTextString)
    {
        slider.value = sliderValue;
        sliderText.text = sliderTextString;
    }
}

