using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Unity.Barracuda;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(Trajectory))]
[RequireComponent(typeof(DataReader))]
public class SkeletonController: MonoBehaviour {

	// Inspector
	[System.Serializable] public enum State { Normal, LatentControl, FeedTrueLatents, FeedTrueFeatures, FeedTrueFeatures_LF, FeedTruePoses };
	public State currentState = State.Normal;

	public bool ShowVelocities = true;

	// Member objects
	public NN_Models models;
	private Actor Actor;
	private Trajectory Trajectory;
	private DataReader Datareader;

	// Skeleton Data
	private int poseDim = 134;
	private int featureDim = 48; //66;
	private int latentDim = 25;

	private float[] Feature;
	private float[] Latent;
	private float[] Pose;
	private bool[] Contacts = {false,false};
	public readonly string[] bones = {"Hips", "LeftUpLeg", "LeftLeg", "LeftFoot", "LeftToe", "RightUpLeg", "RightLeg", "RightFoot", "RightToe", "Spine", "Spine1", "Spine2", "Neck", "Head", "LeftShoulder", "LeftArm", "LeftForeArm", "LeftHand", "RightShoulder", "RightArm", "RightForeArm", "RightHand"};
	private float rotation;
	private Vector3 translation;

	// 30FPS update
	private CustomFixedUpdate FU_instance;
	private const float fps = 1f / 30f;
	public const int deltaT = 6;
	private int framecounter = 0;

	// Main Loop
	void Awake()
    {

		Assert.IsNotNull(models);

		Actor = GetComponent<Actor>();
		Datareader = GetComponent<DataReader>();
		Trajectory = GetComponent<Trajectory>();

		Latent = new float[latentDim];
		Feature = new float[featureDim];
		Pose = new float[poseDim];
		Contacts = new bool[] {false, false};
		ApplyPose();

		FU_instance = new CustomFixedUpdate(fps, Update30fps);

	}

    void Update()
    {
		FU_instance.Update();
	}

	//30 FPS update
	private void Update30fps(float dt)
	{
		switch (currentState)
		{
			case State.Normal:
				//Trajectory.PredictUsingNN();
				Trajectory.PropogatePast(rotation, translation);
				Trajectory.Predict();
				updateFeature();
				if (framecounter % deltaT == 0) {
					Do_LF_step();
					updatePhase_UI();
				}
				Do_HF_step();
				Decode();
				ApplyPose();
				updateContact_UI();
				updateSliders_UI();
				updatePCA_UI();
				break;

			case State.LatentControl:
				Trajectory.Predict();
				Trajectory.PropogatePast(rotation, translation);
				Decode_from_latent(getSliders());
				ApplyPose();
				Trajectory.PropogatePast(rotation, translation);
				updateContact_UI();
				updateSliders_UI(getSliders());
				break;

			case State.FeedTrueFeatures:
			
				var tmpFeat = Datareader.getFeature(framecounter);
				Trajectory.ImportFutureFromFeature(tmpFeat);
                updateFeature();
                if (framecounter % deltaT == 0)
                {
                    Do_LF_step();
                    updatePhase_UI();
				}
                Do_HF_step();
                Decode();
                ApplyPose();
				updateContact_UI();
				updateSliders_UI();
                updatePCA_UI();
				Trajectory.PropogatePast(rotation, translation);
				break;


			case State.FeedTrueFeatures_LF:
				if (framecounter % deltaT == 0)
				{
					Feature = Datareader.getFeature(framecounter);
					Trajectory.ImportFutureFromFeature(Feature);
					Do_LF_step();
					Decode_from_latent(models.getNextLatent());

					updateContact_UI();
					updateSliders_UI();
					updatePhase_UI();
					updatePCA_UI();

				}
				ApplyPose();
				break;


			case State.FeedTrueLatents:
				Trajectory.ImportFutureFromFeature(Datareader.getFeature(framecounter));
				Latent = Datareader.getLatent(framecounter);
				Decode_from_latent(Latent);
				ApplyPose();
				updateSliders_UI();
				updatePCA_UI();
				break;

			case State.FeedTruePoses:
				Trajectory.ImportFutureFromFeature(Datareader.getFeature(framecounter));
				updateSliders_UI(Datareader.getLatent(framecounter));
				updatePCA_UI(Datareader.getLatent(framecounter));
				Pose = Datareader.getPose(framecounter);
				ApplyPose();
				updateContact_UI();
				Trajectory.PropogatePast(rotation, translation);
				updateFeature();
				break;

			default:
				Debug.Log("Invalid Skeleton Setting Case");
				break;

		}

		++framecounter;

	}
		

	// Utilise Model
	private void Do_LF_step(){
		models.LowFrequency(Feature);
	}
	private void Do_HF_step(){
		models.HighFrequency(Feature);		
	}

	private void Decode(){
		Pose = models.Decode();
	}
	private void Decode_from_latent(float[] inpLat) {
		Pose = models.Decode(inpLat);
	}

	private void updatePhase_UI() {
		var tmp = GameObject.FindObjectsOfType<ViewPhase>();
		if (tmp.Length != 0) { tmp[0].GetComponent<ViewPhase>().phase = models.getLFMode(); }
	}

	private void updatePCA_UI(float[] latent = null) {
		var tmp = GameObject.FindObjectsOfType<LatentPCA>();
		if (tmp.Length != 0) {
			var lat = latent == null ? models.getLatent() : latent;
			tmp[0].GetComponent<LatentPCA>().updateWithLatent(lat);
		}
	}

	private void ApplyFootIK(bool right, bool left)
    {
		var tmp = GameObject.FindObjectsOfType<FootIK>();
		if (tmp.Length != 0)
		{
			var footIKs = tmp[0].GetComponents<FootIK>();
			Assert.AreEqual(footIKs.Length, 2);
			footIKs[0].IsActive = left;
			footIKs[1].IsActive = right;
		}

		Contacts[0] = right;
		Contacts[1] = left;

	}

	private void updateContact_UI()
	{
		var tmp = GameObject.FindObjectsOfType<ContactViz>();
		if (tmp.Length != 0)
		{
			tmp[0].GetComponent<ContactViz>().updateContacts(Contacts);
		}
	}

	private void updateSliders_UI(float[] latent = null)
	{
		var tmp = GameObject.FindObjectsOfType<LatentSliders>();
		if (tmp.Length != 0)
		{
			var lat = latent == null ? models.getLatent() : latent;
			tmp[0].GetComponent<LatentSliders>().setSliders(lat);
		}
	}

	private float[] getSliders()
	{
		var tmp = GameObject.FindObjectsOfType<LatentSliders>();
		if (tmp.Length != 0) { return tmp[0].GetComponent<LatentSliders>().getSliders(); }
		return null;
	}

	// Update Feature
	private void updateFeature(){
        Feature = getFeature();
    }

	private float[] getFeature()
    {
		float[] feat = new float[featureDim];

		//Root
		for (int i = 0; i < 6; ++i) { feat[i] = Pose[126 + i]; }

        //Past Future
        float[] trajectoryFeature = Trajectory.GetFeature();
        for (int i = 0; i < trajectoryFeature.Length; i++) { feat[6 + i] = trajectoryFeature[i]; }

        //X_feet, V_feet, X_hands, V_hands
        float[] extremeties = getExtremeties();
		for (int i = 0; i < extremeties.Length; i++) { feat[24 + i] = extremeties[i]; } //42

		return feat;
	}

	private float[] getExtremeties()
    {
		Transform CurrentRoot = Actor.FindBone(bones[0]).Transform;

		float[] extremeties = new float[24];
		string[] feet = { "LeftFoot", "RightFoot" };
		for (int i = 0; i < 2; i++)
		{
			Vector3 pos = CurrentRoot.InverseTransformPoint(Actor.FindBone(feet[i]).Transform.position);  //transRot * (Actor.FindBone(feet[i]).Transform.position - transPos);
			extremeties[0  + 3 * i + 0] = pos.y * 100;
			extremeties[0  + 3 * i + 1] = -pos.x * 100;
			extremeties[0  + 3 * i + 2] = pos.z * 100;

            Vector3 vel = Actor.FindBone(feet[i]).LocalVelocity;
            extremeties[6 + 3 * i + 0] = vel.y * 100;
            extremeties[6 + 3 * i + 1] = -vel.x * 100;
            extremeties[6 + 3 * i + 2] = vel.z * 100;
        }
        string[] hands = { "LeftHand", "RightHand" };
        for (int i = 0; i < 2; i++)
        {
            Vector3 pos = CurrentRoot.InverseTransformPoint(Actor.FindBone(hands[i]).Transform.position);
            extremeties[12 + 3 * i + 0] = pos.y * 100;
            extremeties[12 + 3 * i + 1] = -pos.x * 100;
            extremeties[12 + 3 * i + 2] = pos.z * 100;

            Vector3 vel = Actor.FindBone(hands[i]).LocalVelocity;
            extremeties[18 + 3 * i + 0] = vel.y * 100;
            extremeties[18 + 3 * i + 1] = -vel.x * 100;
            extremeties[18 + 3 * i + 2] = vel.z * 100;
        }
		return extremeties;
	}

	// Apply Pose
	private void ApplyPose() {

		float[] Joints = Pose.Take(126).ToArray();
        float Height = Pose[126];
        float[] posVelXZ = Pose.Skip(127).Take(2).ToArray();
        float[] RotXZ = Pose.Skip(129).Take(2).ToArray();
		float RotVelY = Pose[131];

		rotation = Util.rad_to_deg(RotVelY);
		translation = new Vector3(posVelXZ[1], 0 , posVelXZ[0]) / 100f;

		//Update root rotation
		Actor.FindBone(bones[0]).Transform.localRotation = Quaternion.AngleAxis(Util.rad_to_deg(RotXZ[1]), Vector3.forward) * Quaternion.AngleAxis(-Util.rad_to_deg(RotXZ[0]), Vector3.right);
		this.transform.rotation = Quaternion.Euler(new Vector3(0, this.transform.rotation.eulerAngles.y + Util.rad_to_deg(RotVelY), 0));

		// Update RootPosition
		if ( ! (Contacts[0] && Contacts[1]))
		{
			this.transform.position = this.transform.position + posVelXZ[1] * this.transform.forward / 100f - posVelXZ[0] * this.transform.right / 100f;
			Actor.FindBone(bones[0]).Transform.localPosition = new Vector3(0, Height / 100f, 0);
		}

        // remeber old local joint localpositions
        Vector3[] oldPoses = new Vector3[bones.Length];
		for (int i = 0; i < bones.Length; ++i) { oldPoses[i] = Actor.FindBone(bones[0]).Transform.InverseTransformPoint(Actor.FindBone(bones[i]).Transform.position); }

		// Update Joint Rotations
		for (int i = 0; i<bones.Length-1; ++i){
			int start = i*6;

			Vector3 v0 = new Vector3(Joints[start+0], Joints[start+1], Joints[start+2]);
            Vector3 v1 = new Vector3(Joints[start+3], Joints[start+4], Joints[start+5]);
			(v0,v1) = Util.CorrectSixDimRot(v0,v1);
			Quaternion quaternion = Util.SixDim_to_quat(v0, v1);

			Actor.FindBone(bones[i+1]).Transform.localRotation = quaternion;
		}

        // Update Joint Local Velocities
        for (int i = 0; i < bones.Length; ++i) {
			var v = Actor.FindBone(bones[0]).Transform.InverseTransformPoint(Actor.FindBone(bones[i]).Transform.position) - oldPoses[i];
			Actor.FindBone(bones[i]).LocalVelocity = v;
		}

		ApplyFootIK(Util.Sigmoid(Pose[133]) > 0.5f, Util.Sigmoid(Pose[132]) > 0.5f);

	}

	// Public access to latents
	public int getLatentDim() {
		return latentDim;
	}
	public float[] getLatents() {
		return Latent;
	}


	// EDITOR
#if UNITY_EDITOR
	[CustomEditor(typeof(SkeletonController))]
    public class SkeletonController_Editor : Editor
    {

        public SkeletonController Target;

        void Awake()
        {
            Target = (SkeletonController)target;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(Target, Target.name);

			Inspector();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(Target);
            }
        }

        private void Inspector()
        {
            
            using (new EditorGUILayout.VerticalScope("Box"))
            {
				Utility.SetGUIColor(UltiDraw.DarkGrey);
				Target.currentState = (State)EditorGUILayout.EnumPopup("Skeleton Setting", Target.currentState);
			}
			Target.models = (NN_Models)EditorGUILayout.ObjectField("Neural Network Object", Target.models, typeof(NN_Models), true);
			//Target.ShowVelocities = EditorGUILayout.Toggle("Show Velocities", Target.ShowVelocities);
        }
    }
	#endif

}

// Dimensions that are useful to note
// self.poseComponents = ['R6', 'Height', 'Root_Position_Velocity', 'Root_HipAngleRad', 'Root_HipTurnVelRad']
// self.featureComponents = ['Height_last', 'Root_Position_Velocity_last', 'Root_HipAngleRad_last', 'Root_HipTurnVelRad_last', 'Future_past_pos_last', 'Future_past_ori_last', 'Future_past_vel_last', 'Future_past_speed_last', 'X_feet_last', 'V_feet_last', 'X_hands_last', 'V_hands_last']
// private int featureDim = 67;
// private int poseDim = 133;
// private static float[] featureDimDiv = new float[]{1, 3, 2, 1, 12, 6, 12, 6, 6, 6, 6, 6};
// private static float[] poseDimDiv = new float[]{126, 1, 2, 2, 1};