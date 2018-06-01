using UnityEngine;
using UnityEngine.UI;

public class FPSShower : MonoBehaviour
{
    public Text ShowFPSText;

    private void Update ()
    {
        ShowFPSText.text = string.Format ("FPS : {0}", FPSCounter.Instance.AverageFPS);
    }


}