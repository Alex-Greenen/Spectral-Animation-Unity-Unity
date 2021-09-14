using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayerInput))]
public class InputController: MonoBehaviour {

	//public KeyCode Forward = KeyCode.W;
	//public KeyCode Back = KeyCode.S;
	//public KeyCode Left = KeyCode.A;
	//public KeyCode Right = KeyCode.D;
	//public KeyCode TurnLeft = KeyCode.Q;
	//public KeyCode TurnRight = KeyCode.E;

    public float directionalFalloff = 0.5f;
    public float moveSensitivity = 0.9f;
    public float turnSensitivity = 90f;

    private PlayerControls controls;
    private PlayerInput inputs;
    private Vector2 move_inp;
    private float turn_inp;



    private void Awake()
    {
		controls = new PlayerControls();

        inputs = GetComponent<PlayerInput>();

        //controls.Player.Move.performed += ctx => move_inp = ctx.ReadValue<Vector2>();
        //controls.Player.Move.performed += ctx => move_inp = Vector2.zero;

        //controls.Player.Turn.performed += ctx => turn_inp = ctx.ReadValue<float>();
        //controls.Player.Turn.performed += ctx => turn_inp = 0f;

    }

    private void Update()
    {
        move_inp = inputs.actions["Move"].ReadValue<Vector2>() * moveSensitivity;

        move_inp.x *= directionalFalloff;
        if (move_inp.y < 0f)
            move_inp.y *= directionalFalloff;

        Vector2 tmp = inputs.actions["Turn_Stick"].ReadValue<Vector2>();

        turn_inp = inputs.actions["Turn"].ReadValue<float>() * turnSensitivity + Mathf.Atan2(tmp.x, tmp.y) * Mathf.Rad2Deg * 0.7f;

    }

    public Vector3 QueryMove() {
		return new Vector3(move_inp.x, 0f, move_inp.y);
	}

	public float QueryTurn() {
		return turn_inp;
	}


    //[System.Serializable]
    //public class Style {
    //	public string Name;
    //	public float Bias = 1f;
    //	public float Transition = 0.1f;
    //	public KeyCode[] Keys = new KeyCode[0];
    //	public bool[] Negations = new bool[0];
    //	public Multiplier[] Multipliers = new Multiplier[0];

    //	public bool Query() {
    //		if(Keys.Length == 0) {
    //			return false;
    //		}

    //		bool active = false;

    //		for(int i=0; i<Keys.Length; i++) {
    //			if(!Negations[i]) {
    //				if(Keys[i] == KeyCode.None) {
    //					if(!InputHandler.anyKey) {
    //						active = true;
    //					}
    //				} else {
    //					if(InputHandler.GetKey(Keys[i])) {
    //						active = true;
    //					}
    //				}
    //			}
    //		}

    //		for(int i=0; i<Keys.Length; i++) {
    //			if(Negations[i]) {
    //				if(Keys[i] == KeyCode.None) {
    //					if(!InputHandler.anyKey) {
    //						active = false;
    //					}
    //				} else {
    //					if(InputHandler.GetKey(Keys[i])) {
    //						active = false;
    //					}
    //				}
    //			}
    //		}

    //		return active;
    //	}

    //	public void SetKeyCount(int count) {
    //		count = Mathf.Max(count, 0);
    //		if(Keys.Length != count) {
    //			System.Array.Resize(ref Keys, count);
    //			System.Array.Resize(ref Negations, count);
    //		}
    //	}

    //	public void AddMultiplier() {
    //		ArrayExtensions.Add(ref Multipliers, new Multiplier());
    //	}

    //	public void RemoveMultiplier() {
    //		ArrayExtensions.Shrink(ref Multipliers);
    //	}

    //	[System.Serializable]
    //	public class Multiplier {
    //		public KeyCode Key;
    //		public float Value;
    //	}
    //}

    //#if UNITY_EDITOR
    //public void Inspector() {
    //	Utility.SetGUIColor(Color.grey);
    //	using(new EditorGUILayout.VerticalScope ("Box")) {
    //		Forward = (KeyCode)EditorGUILayout.EnumPopup("Forward", Forward);
    //		Back = (KeyCode)EditorGUILayout.EnumPopup("Backward", Back);
    //		Left = (KeyCode)EditorGUILayout.EnumPopup("Left", Left);
    //		Right = (KeyCode)EditorGUILayout.EnumPopup("Right", Right);
    //		TurnLeft = (KeyCode)EditorGUILayout.EnumPopup("Turn Left", TurnLeft);
    //		TurnRight = (KeyCode)EditorGUILayout.EnumPopup("Turn Right", TurnRight);
    //	}
    //}
    //#endif

}
