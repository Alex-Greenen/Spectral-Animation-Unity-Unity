using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class DataReader : MonoBehaviour
{

    // See : https://answers.unity.com/questions/12598/help-me-about-reading-txt-file.html

    public string FeaturePath;
    public string LatentPath;
    public string PosePath;
    public int minFrameRead;
    public int maxFrameRead;
    private int range;
    private float[][] listoffeatures;
    private float[][] listoflatents;
    private float[][] listofposes;

    void Awake()
    {
        range = maxFrameRead - minFrameRead;
        init_list(FeaturePath, ref listoffeatures);
        init_list(LatentPath, ref listoflatents);
        init_list(PosePath, ref listofposes);
    }

    private void init_list(string path, ref float[][] list)
    {

        TextAsset txt_file = (TextAsset)Resources.Load(path, typeof(TextAsset));
        string[] txt_fine_lines = txt_file.text.Split('\n');

        list = new float[range][];

        int counter = 0;
        while (counter < maxFrameRead)
        {
            if (counter >= minFrameRead)
            {
                string txt = txt_fine_lines[counter];
                string[] words = txt.Split(',');
                float[] feature = new float[words.Length];
                for (int i = 0; i < feature.Length; ++i) { feature[i] = float.Parse(words[i]); }
                list[counter-minFrameRead] = feature;
            }
            ++counter;
        }

    }

    public float[] getFeature(int ind)
    {
        return listoffeatures[ind % range];
    }
    public float[] getLatent(int ind)
    {
        return listoflatents[ind % range];
    }
    public float[] getPose(int ind)
    {
        return listofposes[ind % range];
    }

}

