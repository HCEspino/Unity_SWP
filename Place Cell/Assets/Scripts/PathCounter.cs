using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PathCounter : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;

    private void Start()
    {
        // Get the TextMeshPro component on this object
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    public void SetPathText(float len)
    {
        // Update the text of the TextMeshPro object
        textMeshPro.text = "Path Length\n" + System.Math.Round(len, 3);
    }
}