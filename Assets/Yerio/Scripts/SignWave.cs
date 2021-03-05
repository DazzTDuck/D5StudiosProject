using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignWave : MonoBehaviour
{
    public float strength = 0.08f;
    public float timeStrength = 2;
    [Space]
    public bool rotate = false;

    float time = 0;
    float originalY;

    bool deactivate = false;

    void Start()
    {
        originalY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (!deactivate)
        {
            time += Time.deltaTime;

            var floatY = transform.position;
            floatY.y = originalY + (Mathf.Sin(time * timeStrength) * strength);
            transform.position = floatY;
        }   

        if (rotate)
        { 
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.x, transform.eulerAngles.y + 45, transform.rotation.z), Time.deltaTime);
        }
    }

    public void DeactivateSignWave()
    {
        deactivate = true;
    }
}
