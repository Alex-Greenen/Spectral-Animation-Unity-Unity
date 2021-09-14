using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LatentSliders : MonoBehaviour
{
    public SkeletonController skeleton;
    public float heightFactor;
    public GameObject sliderPrototype;
    
    private GameObject[] sliders;
    private int n_latents;

    // Start is called before the first frame update
    void Start()
    {   
        n_latents = skeleton.getLatentDim();

        sliders = new GameObject[n_latents];

        for (int i = 0; i < n_latents ; ++i){
            sliders[i] = GameObject.Instantiate(sliderPrototype);
            sliders[i].transform.SetParent(this.transform);
            SetLeftBottomPosition(sliders[i].GetComponent<RectTransform>(), new Vector2(0, i * heightFactor / (float) n_latents));
            sliders[i].GetComponent<RectTransform>().localScale = new Vector3(2.5f, 30f / (float) n_latents, 1);
        }
        
    }

    void Update()
    {

    }

    private static void SetLeftBottomPosition(RectTransform trans, Vector2 newPos) {
        trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
    }

    public void setSliders(float[] latents){
        for (int i = 0; i < n_latents; ++i){
            sliders[i].GetComponent<Slider>().value = latents[i];
        }
    }

    public float[] getSliders(){
        float[] sliderValues = new float[n_latents];
        for (int i = 0; i < n_latents; ++i){
            sliderValues[i] = sliders[i].GetComponent<Slider>().value;
        }
        return sliderValues;
    }

}
