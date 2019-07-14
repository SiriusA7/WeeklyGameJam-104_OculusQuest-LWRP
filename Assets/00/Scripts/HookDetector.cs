using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookDetector : MonoBehaviour
{
    public GrapplingHook playerGrapplingHook;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hookable")
        {
            playerGrapplingHook.hooked = true;
            playerGrapplingHook.hookedObj = other.gameObject;
        }
    }
}
