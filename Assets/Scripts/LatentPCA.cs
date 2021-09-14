using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LatentPCA : MonoBehaviour
{
    private static float[] latentdir_1 = new float[] {
          0.0648965621f, -0.2493464044f, -0.1673032002f,  0.3696470924f,
          0.0769762517f, -0.1956470764f,  0.2129139314f, -0.1511363397f,
          0.2816915019f, -0.0374545860f,  0.2824319240f, -0.0983632325f,
          0.0927786321f, -0.2631811419f,  0.1053525471f, -0.2132250469f,
          0.1584719248f, -0.0163172775f,  0.0543094987f, -0.1256285791f,
          0.0962372538f, -0.2694973423f,  0.2634870963f, -0.3377139933f,
          0.2061484874f};


    private static float[] latentdir_2 = new float[] {
         -0.3925266706f, -0.2124234880f,  0.1614695835f, -0.1995287572f,
         -0.1726738082f, -0.2773849504f,  0.1585664631f, -0.1542496409f,
          0.4269302163f, -0.0932300947f,  0.0886683141f,  0.0174273074f,
         -0.2061672703f,  0.2129003646f, -0.1875725365f,  0.0245945325f,
         -0.0947153494f, -0.0777039377f, -0.3300215877f,  0.3346000959f,
          0.0668583167f,  0.0495556944f,  0.0593162778f, -0.0372838132f,
         -0.0935864285f};

    private float x = 0;
    private float y = 0;

    public GameObject markerPrototype;
    public float viewingFactor = 1.5f;
    public Vector2 offset = new Vector2();
    public int numberOfMarkers = 20;
    private GameObject[] markers;

    // Start is called before the first frame update
    void Start()
    {
        markers = new GameObject[numberOfMarkers];
        for (int i = 0; i < numberOfMarkers; ++i)
        {
            markers[i] = Instantiate(markerPrototype, this.transform);
            float f = (float) i / (float) numberOfMarkers;
            markers[i].GetComponent<Image>().color = new Color(f, f, f, f);
        }
        
    }

    public void updateWithLatent(float[] latent)
    {
        x = 0;
        y = 0;
        for (int i=0; i<latent.Length; ++i)
        {
            x += latentdir_1[i] * latent[i];
            y += latentdir_2[i] * latent[i];
        }

        x -= offset.x;
        y -= offset.y;
        for (int i = 0; i < numberOfMarkers-1; ++i)
        {
            markers[i].GetComponent<RectTransform>().anchoredPosition = markers[i + 1].GetComponent<RectTransform>().anchoredPosition;
        }

        markers[numberOfMarkers - 1].GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Clamp(100 * viewingFactor * x,-100, 100), Mathf.Clamp(100 * viewingFactor * y, -100, 100));
    }
}
