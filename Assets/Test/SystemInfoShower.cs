using TMPro;
using UnityEngine;


[RequireComponent (typeof (TextMeshProUGUI))]
public class SystemInfoShower : MonoBehaviour
{
    private TextMeshProUGUI _infoText;

    private void OnEnable ()
    {
        _infoText = GetComponent<TextMeshProUGUI> ();
    }

    private void Update ()
    {
        string text1 = "Support Imange Effect : " + SystemInfo.supportsImageEffects;
        string text2 = "\nSupport GPU Instansing : " + SystemInfo.supportsInstancing;
        string text3 = "\nSupport Computer Shader : " + SystemInfo.supportsComputeShaders;
        string text4 = "\nSupport Reversed ZBuffer : " + SystemInfo.usesReversedZBuffer;
        _infoText.text = text1 + text2 + text3 + text4;
    }

}