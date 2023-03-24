using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExploreToggleText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public string[] buttonTexts = {"Explore: ON", "Explore: OFF"};
    private int currentTextIndex = 0;

    private void Start()
    {
        textMesh = this.GetComponent<TextMeshProUGUI>();
    }

    public void ToggleText()
    {
        currentTextIndex = (currentTextIndex + 1) % buttonTexts.Length;
        textMesh.text = buttonTexts[currentTextIndex];
    }
}
