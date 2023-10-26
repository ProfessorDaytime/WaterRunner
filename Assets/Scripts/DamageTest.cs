using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            print("TRIGGER ENTER");
            FirstPersonController.OnTakeDamage(15);
        }
    }
}
