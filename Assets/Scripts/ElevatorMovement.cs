using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorMovement : MonoBehaviour
{
    public GameObject eFloor;
    public Animator elevator;

    bool atTop = false;
    bool atBottom = true;

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player" && atBottom){
            elevator.SetBool("goDown", false);
            elevator.SetBool("goUp", true);
            // print("go up");
        }

        if(other.gameObject.tag == "Player" && atTop){
            elevator.SetBool("goUp", false);
            elevator.SetBool("goDown", true);
            // print("go down");
        }
    }


    void Update()
    {
        if(eFloor.transform.position.y < 12f){
            atBottom = true;
        } else{
            atBottom = false;
        }

        if(eFloor.transform.position.y > 25f){
            atTop = true;
        } else{
            atTop = false;
        }
        
            // print("at bottom: " + atBottom);
            // print("at top: " + atTop);
            // print(eFloor.transform.position.y);

    }

}
