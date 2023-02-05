using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Tooltip("Массив коллайдеров передних колёс")]
    public WheelCollider[] WColForward;
    [Tooltip("Массив коллайдеров задних колёс")]
    public WheelCollider[] WColBack;

    [Tooltip("Массив трансформов передних колёс")]
    public Transform[] wheelsF;
    [Tooltip("Массив трансформов задних колёс")]
    public Transform[] wheelsB;

    public float wheelOffset = 0.1f; 
    public float wheelRadius = 0.4f; 

    [Tooltip("Максимальный угол поворота колес")]
    public float maxSteer = 30;
    [Tooltip("Максимальный крутящий момент передающийся на колесо")]
    public float maxAccel = 25;
    [Tooltip("Максимальный тормозной момент")]
    public float maxBrake = 50;

    [Tooltip("Центр масс для автомобиля")]
    public GameObject CenterOfMass;


    public class WheelData
    { 
        public Transform wheelTransform;              // трансформ колеса
        public WheelCollider wheelCollider;           // коллайдер колеса
        public Vector3 wheelStartPos;                 // стартовая позиция колеса
        public float rotation = 0.0f;                 // угол поворота колеса
    }

    protected WheelData[] wheels;                     // массив данных о колесах

    void Start()
    {
        // ставим нужный нам центр масс
        GetComponent<Rigidbody>().centerOfMass = CenterOfMass.transform.localPosition;

        // создаём массив по количеству коллайдеров колес
        wheels = new WheelData[WColForward.Length + WColBack.Length];

        for (int i = 0; i < WColForward.Length; i++)
        {
            // заполняем данные по передним колесам
            wheels[i] = SetupWheels(wheelsF[i], WColForward[i]);
        }

        for (int i = 0; i < WColBack.Length; i++)
        {
            // заполняем данные по задним колесам
            wheels[i + WColForward.Length] = SetupWheels(wheelsB[i], WColBack[i]);
        }
    }


    private WheelData SetupWheels(Transform wheel, WheelCollider collider)
    {
        WheelData result = new WheelData();                    // создаём новый объект данных

        result.wheelTransform = wheel;                         // записываем трансформ колеса
        result.wheelCollider = collider;                       // записываем коллайдер колеса
        result.wheelStartPos = wheel.transform.localPosition;       // берем данные глобальной позиции

        return result;
    }

    void FixedUpdate()
    {

        float accel = 0;
        float steer = 0;

        // получаем данные по горизонтальной и вертикальной осям, также работает с WASD
        accel = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");

        // производим движение обновлением колайдеров
        CarMove(accel, steer);

        // обновляем положение колес
        UpdateWheels();
    }

    private void UpdateWheels()
    { 
        float delta = Time.fixedDeltaTime; 

        foreach (WheelData wheel in wheels)
        {

            Vector3 position = wheel.wheelTransform.position;
            Quaternion rotation = wheel.wheelTransform.rotation;

            wheel.wheelCollider.GetWorldPose(out position, out rotation);
            wheel.wheelTransform.position = position;
            wheel.wheelTransform.rotation = rotation * Quaternion.Euler(new Vector3(0f,0f,90f));

            //// касание коллайдера с поверхностью земли
            //WheelHit hit; 
            //// текущая позиция колеса
            //Vector3 localPosition = wheel.wheelTransform.localPosition;

            //if (wheel.wheelCollider.GetGroundHit(out hit))
            //{
            //    // если есть касание колеса с поверхностью
            //    localPosition.y -= Vector3.Dot(wheel.wheelTransform.position - hit.point, transform.up) - wheelRadius;
            //}
            //else
            //{ //18

            //    localPosition.y = wheel.wheelStartPos.y - wheelOffset; //18
            //}
            //wheel.wheelTransform.localPosition = localPosition; //19


            //wheel.rotation = Mathf.Repeat(wheel.rotation + delta * wheel.wheelCollider.rpm * 360.0f / 60.0f, 360.0f); //20
            //wheel.wheelTransform.localRotation = Quaternion.Euler(wheel.rotation, wheel.wheelCollider.steerAngle, 90.0f); //21
        }

    }

    private void CarMove(float accel, float steer)
    { 

        foreach (WheelCollider col in WColForward)
        { 
            // записывем угол поворота передним колесам
            col.steerAngle = steer * maxSteer; 
        }

        if (accel == 0)
        { 
            foreach (WheelCollider col in WColBack)
            {  
                // при отсутствии ускорения по вертикальной оси тормозим
                col.brakeTorque = maxBrake; 
            }
        }
        else
        { 
            foreach (WheelCollider col in WColBack)
            { 
                col.brakeTorque = 0; 
                col.motorTorque = accel * maxAccel; 
            }
        }
    }
}