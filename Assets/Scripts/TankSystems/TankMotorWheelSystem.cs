using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMotorWheelSystem : MonoBehaviour
{
    [Tooltip("Флаг включения скрипта")]
    public bool _Enable = false;                   // включение скрипта

    [Tooltip("Коллайдер мотор-колеса")]
    public WheelCollider _wheelCollider;           // коллайдер левого переднего колеса
    [Tooltip("Трансформ мотор-колеса")]
    public Transform _wheelTransform;              // трансформ левого переднего колеса

    [Tooltip("Мощность двигателя")]
    public float _engineForce = 18000f;            // мощность двигателя
    [Tooltip("Мощность торможения")]
    public float _brakeForce = 12000f;             // сила торможения

    private float _acceleration;                   // значения со стрелок

    void FixedUpdate()
    {
        Inputs();
        Drive();
        _acceleration = 0;
        UpdateWheelPos(_wheelCollider, _wheelTransform);
    }

    // получение данных по горизонтали и вертикали со стрелок
    void Inputs()
    {
        _acceleration = Input.GetAxis("Vertical");
    }

    // базовая функция движения
    void Drive()
    {

        if ((_acceleration >= 0 &&_acceleration <= 0.02f) 
            || (_acceleration <= 0 && _acceleration >= -0.02f))
        {
            Debug.Log("_forwardAcceleration - " + _acceleration + "BREAKING");
            _wheelCollider.brakeTorque = _brakeForce;
        }
        else
        {
            Debug.Log("_forwardAcceleration - " + _acceleration + "MOVING");
            _wheelCollider.brakeTorque = 0;
            _wheelCollider.motorTorque = (_acceleration * _engineForce) / 4;
        }
    }

    void UpdateWheelPos(WheelCollider colider, Transform transform)
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        colider.GetWorldPose(out position, out rotation);
        transform.position = position;
        transform.rotation = rotation;
    }
}
