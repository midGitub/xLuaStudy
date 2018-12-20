
using UnityEngine;
using UnityEngine.UI;

public class UILoadingView : MonoBehaviour
{
    public Text sliderText;
    public Slider slider;

    public void Refresh(int curNum, int allNum, string textString)
    {
        sliderText.text = textString;
        slider.value = ((float)curNum) / ((float)allNum);
    }
}

