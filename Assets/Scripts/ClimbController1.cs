using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController1 : MonoBehaviour
{
    [SerializeField] float speed = 5f;

    Rigidbody rb;

    void Start(){
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate(){
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector2 input = SquareToCircle(new Vector2(h, v));

        //Check walls in a cross pattern
        Vector3 offset = transform.TransformDirection(Vector2.one * 0.5f);
        Vector3 checkDirection = Vector3.zero;
        int k = 0;
        for(int i = 0; i < 4; i++){
            RaycastHit checkHit;
            if(Physics.Raycast(transform.position + offset, transform.forward, out checkHit)){
                checkDirection += checkHit.normal;
                k++;
            }

            //Rotate Offset by 90 degrees
            offset = Quaternion.AngleAxis(90f, transform.forward) * offset;
        }

        checkDirection /= k;

        //Check wall directly in front
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit)){
            transform.forward = -hit.normal;
            rb.position = Vector3.Lerp(rb.position, hit.point + hit.normal * 0.51f, 10f * Time.fixedDeltaTime);
        }

        rb.velocity = transform.TransformDirection(input) * speed;
    }

    Vector2 SquareToCircle(Vector2 input){
        return (input.sqrMagnitude >= 1f) ? input.normalized : input;
    }
}
