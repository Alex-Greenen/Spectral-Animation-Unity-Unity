// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class Controls : MonoBehaviour
// {

//     private PlayerControls controls;
//     private Vector2 posVel; 
//     private Vector2 rotVel; 

//     void Awake()
//     {
//         controls = new PlayerControls();

//         controls.PlayerMovement.PosMovement.performed += cntxt => posVel = cntxt.ReadValue<Vector2>();
//         controls.PlayerMovement.PosMovement.performed += cntxt => posVel = cntxt.ReadValue<Vector2>();

//         controls.PlayerMovement.RotMovement.performed += cntxt => rotVel = cntxt.ReadValue<Vector2>();
//         controls.PlayerMovement.RotMovement.performed += cntxt => rotVel = cntxt.ReadValue<Vector2>();
//     }

//     void OnEnable()
//     {
//         controls.PlayerMovement.Enable();
//     }

//     void OnDisable()
//     {
//         controls.PlayerMovement.Disable();
//     }

//     Vector2 GetPoseMovement() {
//         return 
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         Debug.Log(posVel);

//         // GetComponent<RigidBody>().Velocity = m;
//         // GetComenent<Transform>().Rotate(Vector3.up * rotVel * .2f);
//     }
// }
