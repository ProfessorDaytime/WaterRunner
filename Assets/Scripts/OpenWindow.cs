using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;

public class OpenWindow : MonoBehaviour
{


    
    [SerializeField] private TextMeshProUGUI contextText = default;
    // [SerializeField] private TextMeshProUGUI talkText = default;
    [SerializeField] private KeyCode enterKey = KeyCode.E;

    // private bool isTalking = false;
    // private int talkPhrase = 0;
    private bool inRange = false;

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player"){
            print("WINDOW");
            if(contextText.text == "press R to climb" || contextText.text == ""){
                contextText.text = "press E to enter the window";
            }
            
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
            if(Input.GetKeyDown(enterKey)){
                contextText.text = "";
                
                //TODO: Load Kitchen Room
                SceneManager.LoadScene("Kitchen_Scene001");

            }
        }
        
        
        
        
        
        
    }
}
