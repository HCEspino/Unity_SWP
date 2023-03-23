using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    public Text myText;

    void Start()
    {
        // Create a new Text component
        myText = gameObject.AddComponent<Text>();

        // Set the text and font size
        myText.text = "Hello World!";
        myText.fontSize = 30;

        // Set the text color
        myText.color = Color.white;

        // Set the position of the text on the canvas
        myText.rectTransform.anchoredPosition = new Vector2(0, 0);
    }
}
