using TMPro;
using UnityEngine;

public class WaveTextScript : MonoBehaviour
{
    public TextMeshProUGUI waveText;
    public GameObject waveSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        updateWaveText();
    }

    // Update is called once per frame
    void Update()
    {
        updateWaveText();

    }

    public void updateWaveText()
    {
        WaveScript waveScript = waveSystem.GetComponent<WaveScript>();
        int wave = waveScript.waveNum;
        waveText.text = "Wave: " + wave;
    }
}
