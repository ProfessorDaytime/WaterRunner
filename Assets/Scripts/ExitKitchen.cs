using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;


public class ExitKitchen : MonoBehaviour
{


    
    [SerializeField] private TextMeshProUGUI contextText = default;
    [SerializeField] private TextMeshProUGUI talkText = default;
    [SerializeField] private KeyCode exitKey = KeyCode.E;

    private bool isTalking = false;
    private int talkPhrase = 0;
    private bool inRange = false;

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player" && !isTalking){
            contextText.text = "press E exit";
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
            if(Input.GetKeyDown(exitKey)){
                contextText.text = "";
                //TODO Load Scene
                SceneManager.LoadScene("City_Test001");
                
            }
        }
        
        
        
        
        
        if(isTalking){
            contextText.text = "";
        }
    }
}
