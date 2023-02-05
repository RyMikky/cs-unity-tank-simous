using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRotation : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject _cyl_01;

    private float _h, _v;

    // Update is called once per frame
    void FixedUpdate()
    {
        Inputs();
        Moving();
    }

    void Inputs()
    {
        _h = Input.GetAxis("Horizontal"); _v = Input.GetAxis("Vertical");
    }

    void Moving()
    {
        var body = _cyl_01.GetComponent<Rigidbody>();
        Debug.Log("Vertical = " + _v);
        body.rotation = Quaternion.Euler(new Vector3(_v, 0, 0)) * body.rotation;
        body.position += new Vector3(0, 0, _v * 0.01f);
    }
}
