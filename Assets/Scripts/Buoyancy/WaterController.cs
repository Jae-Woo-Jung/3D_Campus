using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    public static WaterController current;
    
    [Tooltip("water")]
    public GameObject WaterObj;

    public bool isMoving;

    [Tooltip("Wave height and speed")]
    public float scale = 0.1f;
    public float speed = 1.0f;

    [Tooltip("The width between the waves")]
    public float waveDistance = 1f;
    //Noise parameters
    public float noiseStrength = 1f;
    public float noiseWalk = 1f;

    void Start()
    {
        current = this;
    }

    /// <summary>
    /// Get the y coordinate from whatever wavetype we are using
    /// </summary>
    /// <param name="position"></param>
    /// <param name="timeSinceStart"></param>
    /// <returns></returns>
    public float GetWaveYPos(Vector3 position, float timeSinceStart)
    {
        //if (isMoving)
        //{
        //return WaveTypes.SinXWave(position, speed, scale, waveDistance, noiseStrength, noiseWalk, timeSinceStart);
        //}
        //else
        //{
        //return 0f;
        //}

        return WaterObj.transform.position.y;
    }

    //Find the distance from a vertice to water
    //Make sure the position is in global coordinates
    //Positive if above water
    //Negative if below water
    public float DistanceToWater(Vector3 position, float timeSinceStart)
    {
        float waterHeight = GetWaveYPos(position, timeSinceStart);

        float distanceToWater = position.y - waterHeight;

        return distanceToWater;
    }
}
