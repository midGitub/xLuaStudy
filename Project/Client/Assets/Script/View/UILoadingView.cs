
using UnityEngine;
using UnityEngine.UI;

public class UILoadingView : MonoBehaviour
{
    #region Instance
    private static UILoadingView instance;

    public static UILoadingView Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject canvas = GameObject.Find("Canvas");

                instance = canvas.GetComponentInChildren<UILoadingView>();
            }
            return instance;
        }
    }
    #endregion


    public Text sliderText;
    public Slider slider;

    public void Refresh(int curNum, int allNum, string textString)
    {
        sliderText.text = textString;
        slider.value = ((float)curNum) / ((float)allNum);
    }
}

