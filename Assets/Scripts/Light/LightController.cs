using System;
using UnityEngine;

public class LightController : MonoBehaviour
{
    //Speed of light oscillation
    float moveSpeed = 0.5f;

    //Tracker for time elapsed
    float elapsedTime;

    //Activate or deactive movement
    bool moveLight;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Toggle movement
        if(Input.GetKeyUp(KeyCode.L))
        {
            if(moveLight)
            {
                moveLight = false;
            }
            else
            {
                moveLight = true;
            }
        }

        //Move if activated
        if(moveLight)
        {
            elapsedTime += Time.deltaTime;

            // Set EA based on a sine function between 0 and 85 degrees, changes according to time elapsed and speed
            this.transform.eulerAngles = new Vector3( 85*0.5f*(1.0f + Mathf.Sin(moveSpeed * elapsedTime)), -30.0f, 0);
        }
    }
}
