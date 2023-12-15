using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Lady001 : MonoBehaviour
{


    
    [SerializeField] private TextMeshProUGUI contextText = default;
    [SerializeField] private TextMeshProUGUI talkText = default;
    [SerializeField] private KeyCode talkKey = KeyCode.E;

    private bool isTalking = false;
    private int talkPhrase = 0;
    private bool inRange = false;

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player" && !isTalking){
            contextText.text = "press E to talk";
            inRange = true;
        }

        
    }

    void OnTriggerExit(Collider other){
        if(other.gameObject.tag == "Player"){
            contextText.text = "";
            inRange = false;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(inRange){
            if(Input.GetKeyDown(talkKey)){
                contextText.text = "";
                switch(talkPhrase){
                    case 0:
                        talkText.text = "Hey, climb this white building";
                        talkPhrase++;
                        break;
                    case 1:
                        talkText.text = "Someone left their window open.";
                        talkPhrase++;
                        break;
                    case 2:
                        talkText.text = "Go collect their water.";
                        talkPhrase++;
                        break;
                    case 3:
                        talkText.text = "";
                        talkPhrase = 0;
                        isTalking = false;
                        break;
                }
            }
        }
        
        
        
        
        
        if(isTalking){
            contextText.text = "";
        }
    }
}
