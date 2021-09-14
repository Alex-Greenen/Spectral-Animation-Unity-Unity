#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class FootIK : MonoBehaviour
{
    // Inspired By FastIL
    public Transform Foot;
    protected Transform Target;

    private bool _IsActive = false;

    [HideInInspector] public bool IsActive
    {
        get { return _IsActive; }
        set
        {
            if (value != _IsActive && value == true)
            {
                Init();
            }
            _IsActive = value;
        }
    }
    public bool Switch = false;

    public int ChainLength = 2;
        

    [Header("Solver Parameters")]
    public int Iterations = 10;
    /// Distance when the solver stops
    public float Delta = 0.001f;
    
    /// Strength of going back to the start position.
    [Range(0, 1)]
    public float SnapBackStrength = 1f;


    protected float[] BonesLength; //Target to Origin
    protected float CompleteLength;
    protected Transform[] Bones;
    protected Vector3[] Positions;
    protected Vector3[] StartDirectionSucc;
    protected Quaternion[] StartRotationBone;
    protected Quaternion StartRotationTarget;
    protected Transform Root;

    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }

    void Init()
    {
        //initial array

        Bones = new Transform[ChainLength + 1];
        Positions = new Vector3[ChainLength + 1];
        BonesLength = new float[ChainLength];
        StartDirectionSucc = new Vector3[ChainLength + 1];
        StartRotationBone = new Quaternion[ChainLength + 1];

        //find root
        Root = Foot;
        for (var i = 0; i <= ChainLength; i++)
        {
            if (Root == null)
                throw new UnityException("The chain value is longer than the ancestor chain!");
            Root = Root.parent;
        }

        //init target
        if (Target == null) { Target = new GameObject(gameObject.name + " Target").transform; }
        Target.position = new Vector3(Foot.position.x, 0.06926638f, Foot.position.z);
        Target.rotation = Foot.rotation;

        //init data
        var current = Foot.transform;
        CompleteLength = 0;
        for (var i = Bones.Length - 1; i >= 0; i--)
        {
            Bones[i] = current;
            StartRotationBone[i] = GetRotationRootSpace(current);

            if (i == Bones.Length - 1)
            {
                //leaf
                StartDirectionSucc[i] = GetPositionRootSpace(Target) - GetPositionRootSpace(current);
            }
            else
            {
                //mid bone
                StartDirectionSucc[i] = GetPositionRootSpace(Bones[i + 1]) - GetPositionRootSpace(current);
                BonesLength[i] = StartDirectionSucc[i].magnitude;
                CompleteLength += BonesLength[i];
            }

            current = current.parent;
        }

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Switch == true)
        {
            IsActive = !IsActive;
            Switch = false;
        }

        if(IsActive) { ResolveIK(); }
    }

    private void ResolveIK()
    {
        if (Target == null)
            return;

        if (BonesLength.Length != ChainLength)
            Init();

        //get position
        for (int i = 0; i < Bones.Length; i++)
            Positions[i] = GetPositionRootSpace(Bones[i]);

        var targetPosition = GetPositionRootSpace(Target);

        for (int i = 0; i < Positions.Length - 1; i++)
            Positions[i + 1] = Vector3.Lerp(Positions[i + 1], Positions[i] + StartDirectionSucc[i], SnapBackStrength);

        for (int iteration = 0; iteration < Iterations; iteration++)
        {
            //https://www.youtube.com/watch?v=UNoX65PRehA
            //back
            for (int i = Positions.Length - 1; i > 0; i--)
            {
                if (i == Positions.Length - 1)
                    Positions[i] = targetPosition; //set it to target
                else
                    Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BonesLength[i]; //set in line on distance
            }

            //forward
            for (int i = 1; i < Positions.Length; i++) 
                Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1];

            //close enough?
            if ((Positions[Positions.Length - 1] - targetPosition).sqrMagnitude < Delta * Delta)
                break;
        }

        //set position & rotation
        for (int i = 0; i < Bones.Length; i++)
        {
            if (i != Bones.Length - 1) {
                SetRotationRootSpace(Bones[i], Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]) * Quaternion.Inverse(StartRotationBone[i]));
            }

            SetPositionRootSpace(Bones[i], Positions[i]);
        }
    }

    private Vector3 GetPositionRootSpace(Transform current)
    {
        if (Root == null)
            return current.position;
        else
            return Quaternion.Inverse(Root.rotation) * (current.position - Root.position);
    }

    private void SetPositionRootSpace(Transform current, Vector3 position)
    {
        if (Root == null)
            current.position = position;
        else
            current.position = Root.rotation * position + Root.position;
    }

    private Quaternion GetRotationRootSpace(Transform current)
    {
        //inverse(after) * before => rot: before -> after
        if (Root == null)
            return current.rotation;
        else
            return Quaternion.Inverse(current.rotation) * Root.rotation;
    }

    private void SetRotationRootSpace(Transform current, Quaternion rotation)
    {
        if (Root == null)
            current.rotation = rotation;
        else
            current.rotation = Root.rotation * rotation;
    }

    //#if UNITY_EDITOR
    //void OnDrawGizmos()
    //{
    //    var current = this.transform;
    //    for (int i = 0; i < ChainLength && current != null && current.parent != null; i++)
    //    {
    //        var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
    //        Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
    //        Handles.color = Color.green;
    //        Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
    //        current = current.parent;
    //    }
    //}
    //#endif

}
