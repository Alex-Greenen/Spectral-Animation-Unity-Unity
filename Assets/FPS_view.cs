using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class FPS_view : MonoBehaviour
{
    private float deltaTime;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        GetComponent<TMP_Text>().text = "FPS: " + Mathf.Ceil(fps).ToString();
    }
}