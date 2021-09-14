using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour {

	public enum MODE {Follow, LookAt, FreeView, FixedView}

	public MODE Mode = MODE.Follow;
	public Transform Target;
	public Vector3 SelfOffset = Vector3.zero;
	public Vector3 TargetOffset = Vector3.zero;
	[Range(0f, 1f)] public float Damping = 0.975f;
	[Range(-180f, 180f)] public float Yaw = 0f;
	[Range(-45f, 45f)] public float Pitch = 0f;
	[Range(0f, 10f)] public float FOV = 1.5f;
	public float MinHeight = 0.5f;

	private float Velocity = 5f;
	private float AngularVelocity = 5f;
	private float ZoomVelocity = 10;
	private float Sensitivity = 1f;
	private Vector2 MousePosition;
	private Vector2 LastMousePosition;
	private Vector3 DeltaRotation;
	private Quaternion ZeroRotation;

	private Vector3 TargetPosition;
	private Quaternion TargetRotation;

	private GUIStyle ButtonStyle;
	private GUIStyle SliderStyle;
	private GUIStyle ThumbStyle;
	private GUIStyle FontStyle;

	private Vector3 LastTargetPosition;
	private List<Vector3> TargetVelocities;
	private int TargetSmoothing = 50;
	private float MaxVelocity = 0.1f;

	void Awake() {
		TargetPosition = transform.position;
		TargetRotation = transform.rotation;
	}

	void Start() {
		SetMode(Mode);
		TargetVelocities = new List<Vector3>();
	}

	void Update() {

		if(Target == null) {
			return;
		}

		while(TargetVelocities.Count >= TargetSmoothing) {
			TargetVelocities.RemoveAt(0);
		}
		TargetVelocities.Add(Vector3.ClampMagnitude(Target.position - LastTargetPosition, MaxVelocity));
		LastTargetPosition = Target.position;

		if(Mode == MODE.Follow) {
			UpdateFollowCamera();
		}
		if(Mode == MODE.LookAt) {
			UpdateLookAtCamera();
		}
		if(Mode == MODE.FreeView) {
			MousePosition = GetNormalizedMousePosition();
			if(EventSystem.current != null) {
				if(!Mouse.current.leftButton.isPressed) {
					EventSystem.current.SetSelectedGameObject(null);
				}
				if(EventSystem.current.currentSelectedGameObject == null) {
					UpdateFreeCamera();
				}
			} else {
				UpdateFreeCamera();
			}

			LastMousePosition = MousePosition;
		}
		if(Mode == MODE.FixedView) {
			UpdateFixedView();
		}
	}

	void LateUpdate() {
		if(!Application.isPlaying) {
			return;
		}
		
		//Apply Transformation
		float damping = Mode == MODE.FreeView ? 1f : 1f - GetDamping();
		transform.position = Vector3.Lerp(transform.position, TargetPosition, damping);
		transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, damping);

		//Correct Height
		float height = transform.position.y - Target.position.y;
		if(height < MinHeight) {
			transform.position += new Vector3(0f, MinHeight-height, 0f);
		}
	}

	private GUIStyle GetButtonStyle() {
		if(ButtonStyle == null) {
			ButtonStyle = new GUIStyle(GUI.skin.button);
			ButtonStyle.font = (Font)Resources.Load("Fonts/Coolvetica");
			ButtonStyle.normal.textColor = Color.white;
			ButtonStyle.alignment = TextAnchor.MiddleCenter;
		}
		return ButtonStyle;
	}

	private GUIStyle GetSliderStyle() {
		if(SliderStyle == null) {
			SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
		}
		return SliderStyle;
	}

	private GUIStyle GetThumbStyle() {
		if(ThumbStyle == null) {
			ThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
		}
		return ThumbStyle;
	}

	private GUIStyle GetFontStyle() {
		if(FontStyle == null) {
			FontStyle = new GUIStyle();
			FontStyle.font = (Font)Resources.Load("Fonts/Coolvetica");
			FontStyle.normal.textColor = Color.white;
			FontStyle.alignment = TextAnchor.MiddleLeft;
		}
		return FontStyle;
	}

	private void UpdateFollowCamera() {
		Vector3 currentPosition = transform.position;
		Quaternion currentRotation = transform.rotation;

		//Determine Target
		Vector3 _selfOffset = FOV * SelfOffset;
		Vector3 _targetOffset = TargetOffset;
		transform.position = Target.position + Target.rotation * _selfOffset;
		transform.RotateAround(Target.position + Target.rotation * _targetOffset, Vector3.up, Yaw);
		transform.RotateAround(Target.position + Target.rotation * _targetOffset, transform.right, Pitch);
		transform.LookAt(Target.position + Target.rotation * _targetOffset);

		TargetPosition = transform.position;
		TargetRotation = transform.rotation;
		
		transform.position = currentPosition;
		transform.rotation = currentRotation;
	}

	private void UpdateLookAtCamera() {
		Vector3 currentPosition = transform.position;
		Quaternion currentRotation = transform.rotation;

		//Translation
		Vector3 direction = Vector3.zero;
		if (Keyboard.current[Key.LeftArrow].isPressed)
		{
			direction.x -= 1f;
		}
		if (Keyboard.current[Key.RightArrow].isPressed)
		{
			direction.x += 1f;
		}
		if (Keyboard.current[Key.UpArrow].isPressed)
		{
			direction.z += 1f;
		}
		if (Keyboard.current[Key.DownArrow].isPressed)
		{
			direction.z -= 1f;
		}

		transform.position += Velocity*Sensitivity*Time.deltaTime*(transform.rotation*direction);

		//Zoom
		if(Mouse.current.scroll.y.ReadValue() != 0) {
			transform.position += ZoomVelocity*Sensitivity*Time.deltaTime* Mouse.current.scroll.y.ReadValue() * transform.forward;
		}

		//Rotation
		transform.LookAt(Target);

		TargetPosition = transform.position;
		TargetRotation = transform.rotation;

		transform.position = currentPosition;
		transform.rotation = currentRotation;
	}

	private void UpdateFreeCamera() {
		Vector3 currentPosition = transform.position;
		Quaternion currentRotation = transform.rotation;

		//Translation
		Vector3 direction = Vector3.zero;

		if (Keyboard.current[Key.LeftArrow].isPressed)
		{
			direction.x -= 1f;
		}
		if (Keyboard.current[Key.RightArrow].isPressed)
		{
			direction.x += 1f;
		}
		if (Keyboard.current[Key.UpArrow].isPressed)
		{
			direction.z += 1f;
		}
		if (Keyboard.current[Key.DownArrow].isPressed)
		{
			direction.z -= 1f;
		}

		transform.position += Velocity*Sensitivity*Time.deltaTime*(transform.rotation*direction);

		//Zoom
		if(Mouse.current.scroll.y.ReadValue() != 0) {
			transform.position += ZoomVelocity*Sensitivity * Time.deltaTime * Mouse.current.scroll.y.ReadValue() * transform.forward;
		}

		//Rotation
		MousePosition = GetNormalizedMousePosition();
		if(Mouse.current.leftButton.isPressed) {
			DeltaRotation += 1000f*AngularVelocity*Sensitivity*Time.deltaTime*new Vector3(GetNormalizedDeltaMousePosition().x, GetNormalizedDeltaMousePosition().y, 0f);
			transform.rotation = ZeroRotation * Quaternion.Euler(-DeltaRotation.y, DeltaRotation.x, 0f);
		}

		TargetPosition = transform.position;
		TargetRotation = transform.rotation;

		transform.position = currentPosition;
		transform.rotation = currentRotation;
	}

	private void UpdateFixedView() {
		Vector3 currentPosition = transform.position;
		Quaternion currentRotation = transform.rotation;

		float ahead = 1.25f;
		Vector3 bias = new Vector3(40f, 40f, 40f);
		Vector3 velocity = Vector3.zero;
		for(int i=0; i<TargetVelocities.Count; i++) {
			velocity += TargetVelocities[i];
		}
		velocity /= TargetVelocities.Count;
		transform.position = Target.position + FOV*ahead*SelfOffset + Vector3.Scale(bias, velocity);
		transform.LookAt(Target.position + TargetOffset + ahead*Vector3.Scale(bias, velocity));

		TargetPosition = transform.position;
		TargetRotation = transform.rotation;
		
		transform.position = currentPosition;
		transform.rotation = currentRotation;
	}


	public void SetMode(MODE mode) {
		Mode = mode;
		switch(Mode) {
			case MODE.Follow:
			break;

			case MODE.LookAt:
			break;
			
			case MODE.FreeView:
			Vector3 euler = transform.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
			ZeroRotation = transform.rotation;
			MousePosition = GetNormalizedMousePosition();
			LastMousePosition = GetNormalizedMousePosition();
			break;
		}
	}

	public void setFollow() { SetMode(MODE.Follow); }
	public void setLookAt() { SetMode(MODE.LookAt); }
	public void setFree() { SetMode(MODE.FreeView); }
	public void setFixed() { SetMode(MODE.FixedView); }

	public void SetTargetPosition(Vector3 position) {
		TargetPosition = position;
	}

	public void SetTargetRotation(Quaternion rotation) {
		TargetRotation = rotation;
	}

	private float GetDamping() {
		return Application.isPlaying ? Damping : 0f;
	}

	private Vector2 GetNormalizedMousePosition() {
		Vector2 ViewPortPosition = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue());
		return new Vector2(ViewPortPosition.x, ViewPortPosition.y);
	}

	private Vector2 GetNormalizedDeltaMousePosition() {
		return MousePosition - LastMousePosition;
	}

}