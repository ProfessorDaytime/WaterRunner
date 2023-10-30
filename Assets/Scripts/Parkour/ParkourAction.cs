using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Parkour Menu / Create New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] private string animationName;
    [SerializeField] private float minimumHeight;
    [SerializeField] private float maximumHeight;


    public bool CheckIfAvailable(ObstacleInfo hitInfo, Transform player){
        float checkHeight = hitInfo.heightHit.point.y - player.position.y;

        if(checkHeight < minimumHeight || checkHeight > maximumHeight){
            return false;
        }

        return true;
    }

    public string AnimationName => animationName;
}
