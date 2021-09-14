using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(Actor))]
public class SetupOffsets : MonoBehaviour
{
    Actor actor;
    private static string[] bones = new string[] { "Hips", "LeftUpLeg", "LeftLeg", "LeftFoot", "LeftToe", "RightUpLeg", "RightLeg", "RightFoot", "RightToe", "Spine", "Spine1", "Spine2", "Neck", "Head", "LeftShoulder", "LeftArm", "LeftForeArm", "LeftHand", "RightShoulder", "RightArm", "RightForeArm", "RightHand" };
    public string pathToOffsets;
    private float[][] offsets = new float[22][];

    void Awake()
    {
        actor = this.GetComponent<Actor>();

        TextAsset txt_file = (TextAsset) Resources.Load(pathToOffsets, typeof(TextAsset));
        string[] txt_fine_lines = txt_file.text.Split('\n');

        for (int i = 0; i<22; ++i)
        {
            string txt = txt_fine_lines[i];
            string[] words = txt.Split(',');
            float[] line = new float[words.Length];
            for (int j = 0; j < line.Length; ++j) { line[j] = float.Parse(words[j]) / 100f; }
            offsets[i] = line;
        }

        for(int i=0; i<22; ++i)
        {
            actor.FindBone(bones[i]).Transform.localRotation = Quaternion.identity;
            var f = offsets[i];
            actor.FindBone(bones[i]).Transform.localPosition = new Vector3(-f[1], f[0], f[2]);
        }
    }

}
