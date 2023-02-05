using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBoxSystem : MonoBehaviour
{
    public GameObject _nextTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Tank")
        {
            other.gameObject.GetComponent<TankAutoPilotSystem>().SetTargetObject(_nextTarget);
        }
    }

    public void SetNextTarget(GameObject target) { _nextTarget = target; }
}
