// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayerIK : MonoBehaviour
// {

//     ClimbController3 movementScript;
//     Animator animator;


//     // Start is called before the first frame update
//     void Start()
//     {
//         animator = transform.GetComponent<Animator>();
//         movementScript = transform.parent.GetComponent<ClimbController3>();
//     }

//     void OnAnimatorIK(int layerIndex){
//         print("ANIMATOR IK");
        
//         if(movementScript.state == ClimbController3.PlayerState.CLIMBING){
//             HumanBodyBones[] ikBones = {
//                 HumanBodyBones.LeftFoot,
//                 HumanBodyBones.RightFoot,
//                 HumanBodyBones.LeftHand,
//                 HumanBodyBones.RightHand
//             };

//             for(int i = 0; i < ikBones.Length; i++){
//                 AvatarIKGoal goal = (AvatarIKGoal)i;
//                 Transform t = animator.GetBoneTransform(ikBones[i]);
//                 Vector3 direction = transform.parent.forward;

//                 RaycastHit boneHit;
//                 if(Physics.Raycast(t.position - direction, direction * 3f, out boneHit)){
//                     animator.SetIKPosition(goal, boneHit.point - direction * 0.1f);
//                     animator.SetIKPositionWeight(goal, 1f);
//                 } else{
//                     animator.SetIKPositionWeight(goal, 0f);
//                 }
//             }
//         } else{
//             for(int i = 0; i < 4; i++){
//                 animator.SetIKPositionWeight((AvatarIKGoal)i, 0f);
//             }
//         }
//     }
// }
