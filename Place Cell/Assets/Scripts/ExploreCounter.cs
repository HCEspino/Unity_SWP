using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExploreCounter : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;
    private int stepCount = 0;

    private void Awake()
    {
        // Get the TextMeshPro component on this object
        textMeshPro = GetComponent<TextMeshProUGUI>();
        stepCount = 0;
    }

    public void ChangeStepCount()
    {
        // Increase the step count by 1
        stepCount++;

        // Update the text of the TextMeshPro object
        textMeshPro.text = "Exploration Steps\n" + stepCount;
    }
}
