using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    
    public EnvironmentChecker environmentChecker;
    private bool playerInaction;
    public Animator animator;

    [Header("Functional Options")]
    // [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;


    [Header("Controls")]
    // [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Parkour Actions")]
    public List<ParkourAction> parkourAction;

    void Update(){

        var hitData = environmentChecker.CheckObstacle();
        
        if(hitData.hitFound){
            print("Object Found: " + hitData.obstacleHit.transform.name);
        }
    }
}
