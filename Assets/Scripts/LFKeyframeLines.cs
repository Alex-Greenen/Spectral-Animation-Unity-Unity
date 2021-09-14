using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public class LFKeyframeLines : MonoBehaviour
{
    private LinkedList<GameObject> objects = new LinkedList<GameObject>();
	private GameObject invisibleCopy;
	private Actor skeletonActor;
	private string[] bones;

	public int max_keyframes = 10;


	public void setup(GameObject trueSkeleton)
	{
		invisibleCopy = Instantiate(trueSkeleton);
		bones = (string[]) invisibleCopy.GetComponent<SkeletonController>().bones.Clone();
		Destroy(invisibleCopy.GetComponent<SkeletonController>());
		Destroy(invisibleCopy.GetComponent<DataReader>());
		Destroy(invisibleCopy.GetComponent<Trajectory>());
		Destroy(invisibleCopy.GetComponent<InputController>());
		Destroy(invisibleCopy.GetComponent<PlayerInput>());
		Destroy(invisibleCopy.GetComponent<SetupOffsets>());

		skeletonActor = invisibleCopy.GetComponent<Actor>();

		Debug.Log("here");

	}


	public void add_keyframe_pose(float[] Pose, Transform refRoot)
    {
		if (objects.Count() >= max_keyframes) {
			Destroy(objects.First());
			objects.RemoveFirst(); }


		float[] Joints = Pose.Take(126).ToArray();
		float Height = Pose[126];
		float[] posVelXZ = Pose.Skip(127).Take(2).ToArray();
		float[] RotXZ = Pose.Skip(129).Take(2).ToArray();
		float RotVelY = Pose[131];

		//Update root rotation
		skeletonActor.FindBone(bones[0]).Transform.localRotation = Quaternion.AngleAxis(Util.rad_to_deg(RotXZ[1]), Vector3.forward) * Quaternion.AngleAxis(-Util.rad_to_deg(RotXZ[0]), Vector3.right);
		this.transform.rotation = Quaternion.Euler(new Vector3(0, 6 * refRoot.rotation.eulerAngles.y + Util.rad_to_deg(RotVelY), 0));

		// Update RootPosition
		this.transform.position = this.transform.position + 6 * posVelXZ[1] * refRoot.forward / 100f -  6 * posVelXZ[0] * refRoot.right / 100f;
		skeletonActor.FindBone(bones[0]).Transform.localPosition = new Vector3(0, Height / 100f, 0);

		// Update Joint Rotations
		for (int i = 0; i < bones.Length - 1; ++i)
		{
			int start = i * 6;

			Vector3 v0 = new Vector3(Joints[start + 0], Joints[start + 1], Joints[start + 2]);
			Vector3 v1 = new Vector3(Joints[start + 3], Joints[start + 4], Joints[start + 5]);
			(v0, v1) = Util.CorrectSixDimRot(v0, v1);
			Quaternion quaternion = Util.SixDim_to_quat(v0, v1);

			skeletonActor.FindBone(bones[i + 1]).Transform.localRotation = quaternion;
		}

		add_Lines();

	}

	private void add_Lines()
    {
		UltiDraw.Begin();
		Action<Actor.Bone> recursion = null;
		recursion = new Action<Actor.Bone>((bone) => {
			if (bone.GetParent() != null) { Debug.DrawLine(bone.GetParent().Transform.forward, bone.Transform.position, Color.green); }
			for (int i = 0; i < bone.Childs.Length; i++) { recursion(bone.GetChild(i)); }
		});

		if (skeletonActor.Bones.Length > 0) { recursion(skeletonActor.Bones[0]); }

		UltiDraw.End();
	}

}


