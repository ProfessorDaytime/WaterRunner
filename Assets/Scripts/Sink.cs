using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Sink : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI contextText = default;
    // [SerializeField] private TextMeshProUGUI talkText = default;
    [SerializeField] private KeyCode collectKey = KeyCode.E;

    private bool isTalking = false;
    private int talkPhrase = 0;
    private bool inRange = false;

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player"){
            print("SINK");
            contextText.text = "press E to collect water";
            inRange = true;
        }
    }

    void OnTriggerExit(Collider other){
        if(other.gameObject.tag == "Player"){
            contextText.text = "";
            inRange = false;
        }
    }


    

    // Update is called once per frame
    void Update()
    {
        if(inRange){
            if(Input.GetKeyDown(collectKey)){
                contextText.text = "water collected";    
            }
        }
     
    }
}
