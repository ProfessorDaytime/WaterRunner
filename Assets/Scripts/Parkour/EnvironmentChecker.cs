using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentChecker : MonoBehaviour
{
    public Vector3 rayOffset = new Vector3(0,0.2f,0);
    public float rayLength = 0.9f;
    public float heightRayLength = 6f;
    public LayerMask obstacleLayer;

    public ObstacleInfo CheckObstacle(){

        var hitData = new ObstacleInfo();


        var rayOrigin = transform.position + rayOffset;
        hitData.hitFound = Physics.Raycast(rayOrigin, transform.forward, out hitData.obstacleHit, rayLength, obstacleLayer);


        Debug.DrawRay(rayOrigin, transform.forward * rayLength, hitData.hitFound ? Color.red : Color.green);


        if(hitData.hitFound){
            var heightOrigin = hitData.obstacleHit.point + Vector3.up * heightRayLength;

            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightHit, heightRayLength, obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, hitData.heightHitFound ? Color.blue : Color.green);



        }

        return hitData;
    }

    
}

public struct ObstacleInfo
{
    public bool hitFound;
    public bool heightHitFound;
    public RaycastHit obstacleHit;
    public RaycastHit heightHit;
}
