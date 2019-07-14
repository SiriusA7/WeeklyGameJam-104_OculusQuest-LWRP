using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public GameObject hook;
    public GameObject hookHolder;
    public float hookTravelSpeed;

    public GameObject hookableObjectsParent;
    public float hookedObjTravelSpeed;
    public float hookedObjShootSpeed;

    public static bool fired;

    [HideInInspector]
    public bool hooked;

    [HideInInspector]
    public GameObject hookedObj;

    public float maxDistance;
    private float currentDistance;

    private bool grounded;

    void FixedUpdate()
    {
        //firing the hook
        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger) && fired == false)
        {
            fired = true;
        }

        if (fired)
        {
            LineRenderer rope = hook.GetComponent<LineRenderer>();
            rope.SetVertexCount(2);
            rope.SetPosition(0, hookHolder.transform.position);
            rope.SetPosition(1, hook.transform.position);
        }

        if (fired == true && hooked == false)
        {
            hook.transform.Translate(Vector3.forward * Time.deltaTime * hookTravelSpeed);
            currentDistance = Vector3.Distance(transform.position, hook.transform.position);

            if (currentDistance >= maxDistance)
            {
                ReturnHook();
            }
        }

        if (hooked == true && fired == true)
        {
            hookedObj.transform.parent = hook.transform;
            hook.transform.position = Vector3.MoveTowards(hook.transform.position, transform.position, Time.deltaTime * hookedObjTravelSpeed);
            float distanceToHook = Vector3.Distance(transform.position, hook.transform.position);

            hookedObj.GetComponent<Rigidbody>().useGravity = false;
            hookedObj.GetComponent<Rigidbody>().isKinematic = true;
            
        }

        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && fired == true)
        {
            hookedObj.transform.parent = hookableObjectsParent.transform;
            hookedObj.GetComponent<Rigidbody>().useGravity = true;
            hookedObj.GetComponent<Rigidbody>().isKinematic = false;
            hookedObj.GetComponent<Rigidbody>().AddForce(Vector3.Normalize(hookHolder.transform.forward) * hookedObjShootSpeed);
            ReturnHook();
        }
    }

    void ReturnHook()
    {
        hook.transform.rotation = hookHolder.transform.rotation;
        hook.transform.position = hookHolder.transform.position;
        fired = false;
        hooked = false;

        LineRenderer rope = hook.GetComponent<LineRenderer>();
        rope.SetVertexCount(0);
    }
}
