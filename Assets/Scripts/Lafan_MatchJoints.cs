using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lafan_MatchJoints : MonoBehaviour
{

    private struct correspondence
    {
        public Transform reference;
        public Transform mimic;

        public correspondence(Transform mimic, Transform reference)
        {
            this.reference = reference;
            this.mimic = mimic;
        }
    }

    public GameObject ReferenceSkeleton;
    public string preName;

    private List<correspondence> correspondences = new List<correspondence>();

    // Start is called before the first frame update

    void Awake()
    {
        transform.localScale = new Vector3(1,-1,1);
        foreach (Transform reference in ReferenceSkeleton.transform.GetComponentsInChildren<Transform>())
        {
            if (reference.name != ReferenceSkeleton.name) {

                foreach (Transform mimic in this.GetComponentsInChildren<Transform>())
                {
                    if (mimic.name == preName+reference.name)
                    {
                        correspondences.Add(new correspondence(mimic, reference));
                    }
                }

            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSkin();
    }

    public void UpdateSkin()
    {
        this.transform.position = ReferenceSkeleton.transform.position;
        this.transform.rotation = Quaternion.AngleAxis(-90, ReferenceSkeleton.transform.TransformDirection(Vector3.forward)) * ReferenceSkeleton.transform.rotation;
        foreach (correspondence cor in correspondences)
        {
            cor.mimic.position = cor.reference.position;
            cor.mimic.rotation = cor.reference.rotation;
            cor.mimic.rotation = Quaternion.AngleAxis(-90, cor.reference.TransformDirection(Vector3.forward)) * cor.mimic.rotation;
        }
    }
}
