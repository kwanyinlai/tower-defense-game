using TMPro;
using UnityEngine;

public class TextUIScript : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    public GameObject waveSystem;
    private WaveScript waveScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waveScript = waveSystem.GetComponent<WaveScript>();
        updateText();
    }

    // Update is called once per frame
    void Update()
    {
        updateText();
    }

    public void updateText()
    {
        int wave = waveScript.waveNum;
        string text = "Wave: " + wave + "\n";
        text += ResourcePool.getResourceText();
        text += "Base health: " + BaseBattleSystem.getHealth() + "\n";
        infoText.text = text;
    }
}
