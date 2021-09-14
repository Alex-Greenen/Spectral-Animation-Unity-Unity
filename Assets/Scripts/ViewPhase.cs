using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ViewPhase : MonoBehaviour
{
    public GameObject dot;

    public Vector2[] anchors = new Vector2[] { new Vector3(-50f, -50f), new Vector3(50f, -50f), new Vector3(50f, 0f) };
    private Vector2 currentPosition = Vector2.zero;
    public float[] phase = new float[] { 1f, 0f, 0f };

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update() 
    {
        currentPosition = Vector2.zero;
        for (int i = 0; i < phase.Length; ++i) { currentPosition += anchors[i] * phase[i]; }
        dot.GetComponent<RectTransform>().anchoredPosition = currentPosition;
        dot.GetComponent<Image>().color = new Color(phase[0], phase[1], phase[2], 1);
    }

}
