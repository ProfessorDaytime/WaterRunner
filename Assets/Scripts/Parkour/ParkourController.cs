using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    
    public EnvironmentChecker environmentChecker;
    private bool playerInAction;
    public Animator animator;

    [Header("Functional Options")]
    // [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;


    [Header("Controls")]
    // [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Parkour Actions")]
    public List<ParkourAction> parkourActions;

    void Update(){

        if(Input.GetKeyDown(jumpKey)){
            print("JUMP");
            print("Player In Action: " + playerInAction);
        }

        if(Input.GetKeyDown(jumpKey) && !playerInAction){
            var hitData = environmentChecker.CheckObstacle();
        
            if(hitData.hitFound){
                print("Object Found: " + hitData.obstacleHit.transform.name);

                foreach (var action in parkourActions){
                    print("Action in ParkourActions");
                    if(action.CheckIfAvailable(hitData, transform)){
                        // perform parkour action
                        print("Available");
                        StartCoroutine(PerformParkourAction(action));
                        break;
                    }
                }
            }
        }
        
    }

    IEnumerator PerformParkourAction(ParkourAction action){
        playerInAction = true;

        animator.CrossFade(action.AnimationName, 0.2f);

        yield return null;

        var animationState = animator.GetNextAnimatorStateInfo(0);
        if(!animationState.IsName(action.AnimationName)){
            print("Animation Name is Incorrect:  " + action.AnimationName);
        }

        yield return new WaitForSeconds(animationState.length);

        playerInAction = false;
    }

}
