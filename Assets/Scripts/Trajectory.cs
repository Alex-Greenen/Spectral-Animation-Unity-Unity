using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(InputController))]
public class Trajectory: MonoBehaviour {

	public bool ShowTrajectory = false;

	private static Matrix4x4 correction = Matrix4x4.Rotate(Quaternion.AngleAxis(-90, Vector3.up));
	private GameObject HierarchyParent;
	public GameObject directionPointer;
	public GameObject velocityPointer;
	public GameObject pathNode;
	private LineRenderer[] directionPointers;
	// private LineRenderer[] velocityPointers;
	private Transform[] pathNodes;

	public float velocityfactor = 30;

	private int featureDim = 36; //48;

	// Trajectory points
	private Point[] FuturePoints;
	private Point CurrentPoint;
	private Point[] PastPoints;

	// InputController
	private InputController controller;

	// Trajectory Values
	public float TargetGain = 0.25f;
	public float TargetDecay = 0.05f;
	private float TrajectoryCorrection = 1f;
	public float bias_pos = 0.75f;
	public float bias_dir = 0.75f;
	public float bias_vel = 0.75f;
	private Vector3 TargetDirection = Vector3.forward;
	private Vector3 TargetVelocity;

	//LongTerm
	public float maxSpeed = 2;
	public float gain = 1;
	private float turn = 0;
	private float turnVelocity = 0;
	private Vector3 move = Vector3.zero;
	private Vector3 moveVelocity = Vector3.zero;

	// Counters
	public const int PastSamples_N = 3;
	public const int FutureSamples_N = 3;
	public const int PointSamples_N = PastSamples_N + FutureSamples_N;

	private const int PointDensity = 10;
	private const int PastPoints_N = PastSamples_N * PointDensity;
	private const int FuturePoints_N = FutureSamples_N * PointDensity;
	private const int Points_N = PastPoints_N + FuturePoints_N + 1;

	void Start() {
		HierarchyParent = new GameObject("Trajectory");
		HierarchyParent.transform.parent = this.gameObject.transform;
		setVisible(ShowTrajectory);
		controller = GetComponent<InputController>();
		FuturePoints = new Point[FuturePoints_N];
		PastPoints = new Point[PastPoints_N];
		CurrentPoint = new Point(-1);

		directionPointers = new LineRenderer[PointSamples_N+1];
		// velocityPointers = new LineRenderer[PointSamples_N + 1];
		pathNodes = new Transform[PointSamples_N + 1];

		for (int i = 0; i < PastPoints_N; i++)
		{
			PastPoints[i] = new Point(i);
		}

		for (int i=0; i<FuturePoints_N; i++) {
			FuturePoints[i] = new Point(i);
		}

		for (int i=0; i < PointSamples_N+1; ++i)
        {
			GameObject tmp = Instantiate(directionPointer, HierarchyParent.transform);
			directionPointers[i] = tmp.GetComponent<LineRenderer>();
			// tmp = Instantiate(velocityPointer, HierarchyParent.transform);
			// velocityPointers[i] = tmp.GetComponent<LineRenderer>();
			tmp = Instantiate(pathNode, HierarchyParent.transform);
			pathNodes[i] = tmp.GetComponent<Transform>();
		}

	}

	public void setVisible(bool vis)
    {
		HierarchyParent.SetActive(vis);
	}


	public void PropogatePast(float verticalRotation, Vector3 flatTranslation)
	{
		Matrix4x4 rot_transformation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-verticalRotation, Vector3.up), Vector3.one);

		for (int i = PastPoints_N - 1; i > 0; --i)
		{
			PastPoints[i].SetPosition(rot_transformation * (PastPoints[i - 1].GetPosition() - flatTranslation));
			PastPoints[i].SetDirection(rot_transformation * PastPoints[i - 1].GetDirection());
			// PastPoints[i].SetVelocity(rot_transformation * PastPoints[i - 1].GetVelocity());
			// PastPoints[i].SetSpeed(PastPoints[i - 1].GetSpeed());
		}

		PastPoints[0].SetPosition(rot_transformation * (CurrentPoint.GetPosition() - flatTranslation));
		PastPoints[0].SetDirection(rot_transformation * CurrentPoint.GetDirection());
		// PastPoints[0].SetVelocity(rot_transformation * CurrentPoint.GetVelocity().normalized);
		// PastPoints[0].SetSpeed(CurrentPoint.GetSpeed());

		// CurrentPoint.SetSpeed(flatTranslation.magnitude);
		// CurrentPoint.SetVelocity(flatTranslation.normalized);
	}

	public void ImportFutureFromFeature(float[] feature, int feature_start = 6)
	{
		Matrix4x4 fix = Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.up));

		for (int i = 0; i < FutureSamples_N; i++)
		{
			int idx = PointDensity * (FutureSamples_N - i) - 1;
			FuturePoints[idx].SetPosition(fix * new Vector3(-feature[feature_start + 2 * i + 0] / 100, 0, feature[feature_start + 2 * i + 1] / 100));
			FuturePoints[idx].SetRotation(Quaternion.AngleAxis(Util.rad_to_deg(feature[feature_start + 6 + i]), Vector3.up));
			// FuturePoints[idx].SetVelocity(fix * new Vector3(-feature[feature_start + 9 + 2 * i + 0], 0, feature[feature_start + 9 + 2 * i + 1]));
			// FuturePoints[idx].SetSpeed(feature[feature_start + 15 + i] * velocityfactor / 100);
		}

		//for (int i = 0; i < PastSamples_N; i++)
		//{
		//	int idx = PointDensity * (i+1) - 1;
		//	PastPoints[idx].SetPosition(fix * new Vector3(-feature[feature_start + 18 + 2 * i + 0] / 100, 0, feature[feature_start + 18 + 2 * i + 1] / 100));
		//	PastPoints[idx].SetRotation(Quaternion.AngleAxis(rad_to_deg(feature[feature_start + 18 + 6 + i]), Vector3.up));
		//	PastPoints[idx].SetVelocity(fix * new Vector3(-feature[feature_start + 18 + 9 + 2 * i + 0], 0, feature[feature_start + 18 + 9 + 2 * i + 1]));
		//	PastPoints[idx].SetSpeed(feature[feature_start + 18 + 15 + i] * velocityfactor / 100);
		//}

	}


	public float[] GetFeature()
    {

		// Future
		Matrix4x4 fix = Matrix4x4.Rotate(Quaternion.AngleAxis(-90, Vector3.up));
		float[] feature = new float[featureDim];
		for (int i = 0; i < Trajectory.FutureSamples_N; i++)
		{
			Point p = FuturePoints[PointDensity * (FutureSamples_N - i) - 1];
			Vector3 pos = fix * p.GetPosition();
			float dir = Util.deg_to_rad(p.GetRotation().eulerAngles.y);
			// Vector3 vel = fix * p.GetVelocity().normalized;
			// float speed = p.GetSpeed();

			feature[2 * i + 0] = -pos.x * 100f;
			feature[2 * i + 1] = pos.z * 100f;
			feature[6 + i] = dir;
			// feature[9 + 2 * i + 0] = -vel.x;
			// feature[9 + 2 * i + 1] = vel.z;
			// feature[15 + i] = speed * 100f / velocityfactor;
		}

		// Past
		int ofset = 9;
		for (int i = 0; i < Trajectory.PastSamples_N; i++)
		{
			Point p = PastPoints[PointDensity * (i + 1) - 1];
			Vector3 pos = fix * p.GetPosition();
			float dir = Util.deg_to_rad(p.GetRotation().eulerAngles.y);
			// Vector3 vel = fix * p.GetVelocity().normalized;
			// float speed = p.GetSpeed();

			feature[ofset + 2 * i + 0] = -pos.x * 100f;
			feature[ofset + 2 * i + 1] = pos.z * 100f;
			feature[ofset + 6 + i] = dir;
			// feature[ofset + 9 + 2 * i + 0] = -vel.x;
			// feature[ofset + 9 + 2 * i + 1] = vel.z;
			// feature[ofset + 15 + i] = speed * 100f / velocityfactor;
		}
		return feature;
	}

	private static void print(float[] inp)
	{
		string result = "";
		foreach (var item in inp)
		{
			result += item.ToString() + ",";
		}
		Debug.Log(result);
	}

	//public void PredictUsingNN()
	//   {
	//	turn = Mathf.SmoothDamp(turn, controller.QueryTurn(), ref turnVelocity, gain, maxSpeed);
	//	move = Vector3.SmoothDamp(move, controller.QueryMove(), ref moveVelocity, gain, maxSpeed);
	//	float[] input = new float[] { turn, move.x, move.z };
	//	float[] futureFeature = GameObject.FindObjectOfType<NN_Models>().GetComponent<NN_Models>().getTrajectoryFromInput(input);
	//	ImportFutureFromFeature(futureFeature, 0);

	//}

	public void Predict()
	{
		//Determine Control
		float turn = controller.QueryTurn();
		Vector3 move = controller.QueryMove();
		bool control = turn != 0f || move != Vector3.zero;

		//Update Target Direction / Velocity / Correction
		TargetDirection = Vector3.Lerp(TargetDirection, Quaternion.AngleAxis(turn, Vector3.up) * Vector3.forward, control ? TargetGain : TargetDecay);
		TargetVelocity = Vector3.Lerp(TargetVelocity, (Quaternion.LookRotation(TargetDirection, Vector3.up) * move), control ? TargetGain : TargetDecay);
		TrajectoryCorrection = Utility.Interpolate(TrajectoryCorrection, Mathf.Max(move.magnitude, Mathf.Abs(turn)), control ? TargetGain : TargetDecay);

		//Predict Future Trajectory
		Vector3[] trajectory_positions_blend = new Vector3[FuturePoints.Length];
		for (int i = 0; i < FuturePoints_N; i++)
		{
			float weight = (float) (i+1) / (float)FuturePoints_N;  //w between 1/FuturePoints and 1
			float scale_pos = 1.0f - Mathf.Pow(1.0f - weight, bias_pos);
			float scale_dir = 1.0f - Mathf.Pow(1.0f - weight, bias_dir);
			// float scale_vel = 1.0f - Mathf.Pow(1.0f - weight, bias_vel);

			float scale = 3 * TargetGain / FuturePoints_N;

			trajectory_positions_blend[i] = (i != 0 ? trajectory_positions_blend[i - 1] : Vector3.zero) +
				Vector3.Lerp(
				FuturePoints[i].GetPosition() - (i != 0 ? FuturePoints[i - 1].GetPosition() : Vector3.zero),
				scale * TargetVelocity,
				scale_pos
				);

			FuturePoints[i].SetDirection(Vector3.Lerp(Vector3.forward, FuturePoints[i].GetDirection(), scale_dir));
			FuturePoints[i].SetDirection(Vector3.Lerp(FuturePoints[i].GetDirection(), TargetDirection, scale_dir));

			// FuturePoints[i].SetVelocity(Vector3.Lerp(CurrentPoint.GetVelocity(), FuturePoints[i].GetVelocity(), scale_vel));
			// FuturePoints[i].SetVelocity(Vector3.Lerp(FuturePoints[i].GetVelocity(), TargetVelocity, scale_vel));

			FuturePoints[i].SetPosition(trajectory_positions_blend[i]);

			// FuturePoints[i].SetSpeed(Utility.Interpolate(CurrentPoint.GetSpeed(), FuturePoints[i].GetSpeed(), scale_vel));
			// FuturePoints[i].SetSpeed(Utility.Interpolate(FuturePoints[i].GetSpeed(), TargetVelocity.magnitude, scale_vel));
		}
	}


	void OnRenderObject() {

		// Draw Connections
		if (ShowTrajectory) {

            for (int i = 0; i < FutureSamples_N; ++i)
			{
				Point p = FuturePoints[PointDensity * (FutureSamples_N - i) - 1];
				pathNodes[i].localPosition = correction * p.GetPosition();
				directionPointers[i].SetPosition(0, correction * p.GetPosition());
				directionPointers[i].SetPosition(1, correction * (p.GetPosition() + 0.25f * p.GetDirection()));
				// velocityPointers[i].SetPosition(0, correction * p.GetPosition());
				// velocityPointers[i].SetPosition(1, correction * (p.GetPosition() + 0.25f * p.GetSpeed() * p.GetVelocity()));
			}

			for (int i = 0; i < PastSamples_N; ++i)
			{
				Point p = PastPoints[PointDensity * (i + 1) - 1];
				pathNodes[i + FutureSamples_N].localPosition = correction * p.GetPosition();
				directionPointers[i + FutureSamples_N].SetPosition(0, correction * p.GetPosition());
				directionPointers[i + FutureSamples_N].SetPosition(1, correction * (p.GetPosition() + 0.25f * p.GetDirection()));
				// velocityPointers[i + FutureSamples_N].SetPosition(0, correction * p.GetPosition());
				// velocityPointers[i + FutureSamples_N].SetPosition(1, correction * (p.GetPosition() + 0.25f * p.GetSpeed() * p.GetVelocity()));
			}

			directionPointers[PointSamples_N].SetPosition(1, 0.25f * (correction * CurrentPoint.GetDirection()));
			// velocityPointers[PointSamples_N].SetPosition(1, correction * (0.25f * CurrentPoint.GetSpeed() * CurrentPoint.GetVelocity()));

		}
		else
        {

        }

    }

	// EDITOR
	#if UNITY_EDITOR
	[CustomEditor(typeof(Trajectory))]
	public class Trajectory_Editor : Editor
	{

		public Trajectory Target;

		void Awake()
		{
			Target = (Trajectory)target;
		}

		public override void OnInspectorGUI()
		{
			Undo.RecordObject(Target, Target.name);

			Inspector();

			if (GUI.changed)
			{
				if (Application.isPlaying) { Target.setVisible(Target.ShowTrajectory); }
				EditorUtility.SetDirty(Target);
			}
		}

		private void Inspector()
		{

			Target.ShowTrajectory = EditorGUILayout.Toggle("Show Trajectory", Target.ShowTrajectory);

			using (new EditorGUILayout.VerticalScope("Box"))
			{
				Utility.SetGUIColor(UltiDraw.DarkGrey);
				Target.directionPointer = (GameObject)EditorGUILayout.ObjectField("Direction Pointer", Target.directionPointer, typeof(GameObject), true);
				Target.velocityPointer = (GameObject)EditorGUILayout.ObjectField("Velocity Pointer", Target.velocityPointer, typeof(GameObject), true);
				Target.pathNode = (GameObject)EditorGUILayout.ObjectField("Path Node", Target.pathNode, typeof(GameObject), true);
			}

			using (new EditorGUILayout.VerticalScope("Box"))
			{
				Utility.SetGUIColor(UltiDraw.DarkGrey);
				Target.velocityfactor = EditorGUILayout.FloatField("Speed Factor", Target.velocityfactor);
				Target.TargetGain = EditorGUILayout.FloatField("Target Gain", Target.TargetGain);
				Target.TargetDecay = EditorGUILayout.FloatField("Target Decay", Target.TargetDecay);
				Target.bias_pos = EditorGUILayout.FloatField("Position Bias Factor", Target.bias_pos);
				Target.bias_dir = EditorGUILayout.FloatField("Direction Bias Factor", Target.bias_dir);
				Target.bias_vel = EditorGUILayout.FloatField("Velcoity Bias Factor", Target.bias_vel);
			}
			
		}
	}
	#endif

	public class Point {
		private int Index;
		private Matrix4x4 Transformation;
		private Vector3 Velocity;
		private float Speed;

		public Point(int index) {
			Index = index;
			Velocity = Vector3.forward;
			Transformation = Matrix4x4.identity;
			SetDirection(Vector3.forward);
			Speed = 0;
		}

		public void SetIndex(int index) { Index = index; }

		public int GetIndex() { return Index; }

		public void SetTransformation(Matrix4x4 matrix) { Transformation = matrix; }

		public Matrix4x4 GetTransformation() { return Transformation; }

		public void SetPosition(Vector3 position) { Matrix4x4Extensions.SetPosition(ref Transformation, position); }

		public Vector3 GetPosition() { return Transformation.GetPosition(); }

		public void SetRotation(Quaternion rotation) { Matrix4x4Extensions.SetRotation(ref Transformation, rotation); }

		public Quaternion GetRotation() { return Transformation.GetRotation(); }

		public void SetDirection(Vector3 direction) { SetRotation(Quaternion.LookRotation(direction == Vector3.zero ? Vector3.forward : direction, Vector3.up)); }

		public Vector3 GetDirection() { return Transformation.GetForward(); }

		public void SetSpeed(float speed) { Speed = speed; }

		public float GetSpeed() { return Speed; }

		public void SetVelocity(Vector3 velocity) { Velocity = velocity; }

		public Vector3 GetVelocity() { return Velocity; }

	}

}
