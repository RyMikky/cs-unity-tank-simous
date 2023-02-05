using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TankTruckMovingSystem : MonoBehaviour
{
    public enum InputType
    {
        None, Player, AutoPilot
    }

    [Tooltip("Режим ввода для объекта танка")]
    public InputType _inputType = InputType.Player;

    [Tooltip("Флаг включения скрипта")]
    public bool _Enable = false;                      // включение скрипта

    [Header("Блок наполнения коллайдерами и трансформами")]
    [Tooltip("Коллайдеры ведущих колес левого борта")]
    public WheelCollider[] _leftWheelsColliders;      // коллайдеры ведущих колес левого борта
    [Tooltip("Коллайдеры ведущих колес правого борта")]
    public WheelCollider[] _rightWheelsColliders;     // коллайдеры ведущих колес правого борта

    [Tooltip("Трансформы ведущих колес левого борта")]
    public Transform[] _leftWheelsTransforms;         // трансформы ведущих колес левого борта
    [Tooltip("Трансформы ведущих колес правого борта")]
    public Transform[] _rightWheelsTransforms;        // трансформы ведущих колес правого борта

    [Tooltip("Трансформы зависимых колес левого борта")]
    public Transform[] _leftDependenceWheelsTransforms;         // трансформы зависимых колес левого борта
    [Tooltip("Трансформы зависимых колес правого борта")]
    public Transform[] _rightDependenceWheelsTransforms;        // трансформы зависимых колес правого борта

    public Transform _testTruckBone;

    [Header("Силовые характеристики движетеля")]
    [Tooltip("Мощность двигателя")]
    public float _engineForce = 1800f;                // мощность двигателя
    public void SetEngineForce(float force) { _engineForce = force; }
    public float GetEngineForce() { return _engineForce; }
    [Tooltip("Мощность торможения")]
    public float _brakeForce = 2400f;                 // сила торможения
    public void SetBreakForce(float force) { _brakeForce = force; }
    public float GetBreakForce() { return _brakeForce; }

    [Header("Настройки максимальной скорости и бокового сольжения")]
    [Tooltip("Базовый множитель бокового сольжения гусениц")]
    public float _defaultSideFriction = 0.5f;         // базовый множитель бокового сольжения гусениц
    [Tooltip("Множитель бокового сольжения гусениц комплексного перемещения")]
    public float _completeMoveSideFriction = 0.3f;    // множитель бокового сольжения гусениц комплексного перемещения
    
    [Tooltip("Максимальная скорость движения")]
    public float _directMoveVelocityLimit = 6f;       // максимальная скорость движения
    public void SetDirectVelocityLimit(float limit) { _directMoveVelocityLimit = limit; }
    public float GetDirectVelocityLimit() { return _directMoveVelocityLimit; }
    [Tooltip("Множитель бокового скольжения при движении прямо")]
    public float _directMoveSideFriction = 0.5f;      // множитель бокового скольжения при движении прямо

    [Tooltip("Максимальная скорость вращения на месте")]
    public float _rotateOnStandVelocityLimit = 1f;    // максимальная скорость вращения на месте
    public void SetRotateVelocityLimit(float limit) { _rotateOnStandVelocityLimit = limit; }
    public float GetRotateVelocityLimit() { return _rotateOnStandVelocityLimit; }
    [Tooltip("Множитель бокового скольжения при развороте на месте")]
    public float _rotateOnStandSideFriction = 0.06f;  // множитель бокового скольжения при развороте на месте

    [Header("Погрешности положения осей джойстика")]
    [Tooltip("Погрешность отклонения джойстика по вертикали")]
    public float _deltaVertical = 0.02f;              // погрешность отклонения джойстика по вертикали
    private float _defDeltaVertical;                  // погрешность отклонения джойстика по вертикали
    public void SetDeltaVertical(float deltaV) { _deltaVertical = deltaV; }
    public float GetDeltaVertical() { return _deltaVertical; }
    public void SetDeltaVerticalDefault() { _deltaVertical = _defDeltaVertical;  }
    [Tooltip("Погрешность отклонения джойстика по горизонтали")]
    public float _deltaHorizontal = 0.02f;            // погрешность отклонения джойстика по горизонтали
    private float _defDeltaHorizontal = 0.02f;        // погрешность отклонения джойстика по горизонтали
    public void SetDeltaHorizontal(float deltaV) { _deltaHorizontal = deltaV; }
    public float GetDeltaHorizontal() { return _deltaHorizontal; }
    public void SetDeltaHorizontalDefault() { _deltaHorizontal = _defDeltaHorizontal; }

    private float _forwardAcceleration;               // значения со стрелок
    private float _rotateAcceleration;                // значения для горизонтальной оси
    private float _breakPowerScaler;                  // множитель усилия тормозных механизмов

    private float _currentSpeed;                      // значение текущей скорости
    private float _angularSpeed;                      // значение скорости вращения вокруг оси Y

    private Transform _leftReferenceWheelTransform;   // базовая ссылка на вращение левого борта
    private Transform _rightReferenceWheelTransform;  // базовая ссылка на вращение левого борта
    private Rigidbody _currentBody;                   // тело над которым работает скрипт
    private float _suspentionDistance;                // высота подвески

    [Header("Блок дебаговых переменных")]
    [Tooltip("Показатель текущей скорости")]
    public float DEBUGCURRENTVELOCITY;
    public float GetDebugCurrentVelocity() { return DEBUGCURRENTVELOCITY; }
    [Tooltip("Показатель текущей скорости разворота по оси Y")]
    public float DEBUGCURRENTAXISYVELOCITY;
    public float GetDebugCurrentAxisVelocity() { return DEBUGCURRENTAXISYVELOCITY; }
    [Tooltip("Показатель текущей угловой cкорости")]
    public float DEBUGCURRENTANGULARVELOCITY;
    [Tooltip("Множитель входящего ускорения по вертикальной оси")]
    public float DEBUGFORWARDACCEL;
    public float GetDebugCurrentForwardAcceleration() { return DEBUGFORWARDACCEL; }
    [Tooltip("Множитель входящего ускорения по горизонтальной оси")]
    public float DEBUGROTATIONACCEL;
    public float GetDebugCurrentRotationAcceleration() { return DEBUGROTATIONACCEL; }
    [Tooltip("Множитель текущего тормозного усилия")]
    public float DEBUGBREAKACCEL;
    public float GetDebugCurrentBreakAcceleration() { return DEBUGBREAKACCEL; }

    [Tooltip("Величина текущего вращательного момента на левом колесе")]
    public float DEBUGLEFTTORQUE;
    public float GetDebugLeftWheelTorque() { return DEBUGLEFTTORQUE; }
    [Tooltip("Величина текущего вращательного момента на правом колесе")]
    public float DEBUGRIGHTTORQUE;
    public float GetDebugRightWheelTorque() { return DEBUGRIGHTTORQUE; }
    [Tooltip("Величина текущего тормозящего момента момента на левом колесе")]
    public float DEBUGLEFTBREAKTORQUE;
    public float GetDebugLeftBreakForce() { return DEBUGLEFTBREAKTORQUE; }
    [Tooltip("Величина текущего тормозящего момента момента на правом колесе")]
    public float DEBUGRIGHTBREAKTORQUE;
    public float GetDebugRightBreakForce() { return DEBUGRIGHTBREAKTORQUE; }
    [Tooltip("Величина текущего множителя бокового сольжения на левом колесе")]
    public float DEBUGSIDEWAYSTIFFNESS;

    private enum MovingType
    {
        direct, on_stand, complette, autobreak
    }

    public void SetInputType(InputType type) { _inputType = type; }
    public InputType GetInputType() { return _inputType; }

    public void SetForwardAcceleration(float accel) { if (Mathf.Abs(accel) <= 1) _forwardAcceleration = accel; }
    public float GetForwardAcceleration() { return _forwardAcceleration; }

    public void SetRotateAcceleration(float rotate) { if (Mathf.Abs(rotate) <= 1) _rotateAcceleration = rotate; }
    public float GetRotateAcceleration() { return _rotateAcceleration; }

    public void SetBreakPowerScaler(float bPower) { if (Mathf.Abs(bPower) <= 1) _breakPowerScaler = bPower; }
    public float GetBreakPowerScaler() { return _breakPowerScaler; }

    private void Awake()
    {
        _currentBody = GetComponent<Rigidbody>();

        if (_leftWheelsTransforms.Length != 0)
        {
            _leftReferenceWheelTransform = _leftWheelsTransforms[0];
            _suspentionDistance = _leftWheelsColliders[0].suspensionDistance;
        }

        if (_rightWheelsTransforms.Length != 0)
        {
            _rightReferenceWheelTransform = _rightWheelsTransforms[0];
        }
        
        // запоминаем изначально заданные погрешности отклонения джойстика
        _defDeltaVertical = _deltaVertical;
        _defDeltaHorizontal = _deltaHorizontal;
    }

    void FixedUpdate()
    {
        UpdateCurrentSpeed();                      // записываем показатели текущей скорости
        UpdateInputsData();                        // записываем информацию по положению осей джойстика

        CompletteDrive();                          // комплексная функция движения

        UpdateDebugData();                         // обновления дебаговых переменных
        UpdateWheelTransforms();                   // обновляем положения колёс
        UpdateTruckBonesTransforms();              // обновление положеие костей меша гусениц
    }

    void UpdateDebugData()
    {

        DEBUGCURRENTVELOCITY = _currentSpeed;
        DEBUGCURRENTAXISYVELOCITY = _angularSpeed;
        
        //DEBUGCURRENTANGULARVELOCITY = _angularSpeed.magnitude;
        DEBUGCURRENTANGULARVELOCITY = _angularSpeed;

        DEBUGFORWARDACCEL = _forwardAcceleration;
        DEBUGROTATIONACCEL = _rotateAcceleration;
        DEBUGBREAKACCEL = _breakPowerScaler;

        DEBUGLEFTTORQUE = _leftWheelsColliders[2].motorTorque;
        DEBUGRIGHTTORQUE = _rightWheelsColliders[2].motorTorque;
        DEBUGLEFTBREAKTORQUE = _leftWheelsColliders[2].brakeTorque;
        DEBUGRIGHTBREAKTORQUE = _rightWheelsColliders[2].brakeTorque;
        DEBUGSIDEWAYSTIFFNESS = _leftWheelsColliders[2].sidewaysFriction.stiffness;
    }

    // получение данных по горизонтали и вертикали со стрелок
    void UpdateInputsData()
    {
        switch (_inputType)
        {
            case InputType.Player:
                _forwardAcceleration = Input.GetAxis("Vertical");
                _rotateAcceleration = Input.GetAxis("Horizontal");
                break;
            case InputType.AutoPilot:
                break;
        }
    }

    void UpdateCurrentSpeed()
    {
        _angularSpeed = _currentBody.angularVelocity.magnitude;           // обновляем угловую скорость
        _currentSpeed = _currentBody.velocity.magnitude;        // обновляем обычную скорость
    }

    void UpdateWheelTransforms()
    {
        UpdateWheelTransform(ref _leftWheelsColliders, ref _leftWheelsTransforms);
        UpdateWheelTransform(ref _rightWheelsColliders, ref _rightWheelsTransforms);

        // обновление трансформов зависимых колес по референсу
        UpdateReferenceTransform(_leftReferenceWheelTransform, ref _leftDependenceWheelsTransforms);
        UpdateReferenceTransform(_rightReferenceWheelTransform, ref _rightDependenceWheelsTransforms);
    }

    void UpdateTruckBonesTransforms()
    {
        Vector3 bonePosition = _testTruckBone.position;
        bonePosition.x = (_leftWheelsTransforms[1].position.y - 0.35f);
    }

    void DirectMoving()
    {
        
        DirectDrive(ref _rightWheelsColliders);
        DirectDrive(ref _leftWheelsColliders);

        _forwardAcceleration = 0;
        _rotateAcceleration = 0;
    }

    // базовая функция прямолинейного движения вперед и назад
    void DirectDrive(ref WheelCollider[] collidersArray)
    {
        for (int i = 0; i < collidersArray.Length; i++)
        {
            WheelFrictionCurve frictionSet = collidersArray[i].sidewaysFriction;

            if (Mathf.Abs(_forwardAcceleration) >= 0 && Mathf.Abs(_forwardAcceleration) <= _deltaVertical)
            {
                // если нет усилия акселерации, то тормозим
                frictionSet.stiffness = 0.8f;
                collidersArray[i].sidewaysFriction = frictionSet;
                collidersArray[i].brakeTorque = _brakeForce;
            }
            else if (_currentSpeed > _directMoveVelocityLimit)
            {
                // если текущая скорость превышает максимальную, то тормозим
                frictionSet.stiffness = 0.8f;
                collidersArray[i].sidewaysFriction = frictionSet;
                collidersArray[i].brakeTorque = _brakeForce;
            }
            else
            {
                frictionSet.stiffness = 0.06f;
                collidersArray[i].sidewaysFriction = frictionSet;
                collidersArray[i].brakeTorque = 0;
                collidersArray[i].motorTorque = ((_forwardAcceleration + _rotateAcceleration) * _engineForce);
            }
        }
    }

    void CompletteDrive()
    {
        float totalLeftAcceleration = 0;
        float totalRightAcceleration = 0;
        float breakPowerScaler = 0;

        // если нет ни вертикального ускорения ни горизонтального
        if (Mathf.Abs(_forwardAcceleration) <= _deltaVertical && Mathf.Abs(_rotateAcceleration) <= _deltaHorizontal)
        {
            breakPowerScaler = 1;
            // просто передаем нули и автоматом встаём на тормоз
            SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, 0, _defaultSideFriction, MovingType.autobreak);
            SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, 0, _defaultSideFriction, MovingType.autobreak);
        }

        // если передаётся вертикальное ускорение, но горизонтального нет - прямолинейное движение
        else if (Mathf.Abs(_forwardAcceleration) > _deltaVertical && Mathf.Abs(_rotateAcceleration) <= _deltaHorizontal)
        {
            totalLeftAcceleration = _forwardAcceleration;
            totalRightAcceleration = _forwardAcceleration;
            breakPowerScaler = _breakPowerScaler;

            // передаём прямое ускорение полученное по оси, обычное ограничение скорости, коэффициент скольжения и тип движения
            SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _directMoveVelocityLimit, _directMoveSideFriction, MovingType.direct);
            SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _directMoveVelocityLimit, _directMoveSideFriction, MovingType.direct);
            
        }

        // если передаётся горизонтальное ускорение, но вертикального нет - разворот на пятачке
        else if (Mathf.Abs(_forwardAcceleration) <= _deltaVertical && Mathf.Abs(_rotateAcceleration) > _deltaHorizontal)
        {
            totalLeftAcceleration = _rotateAcceleration;
            totalRightAcceleration = -_rotateAcceleration;
            breakPowerScaler = _breakPowerScaler;

            if (_currentSpeed > 0.1f)
            {
                // передаём прямое ускорение полученное по оси, обычное ограничение скорости, коэффициент скольжения и тип движения
                SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _completeMoveSideFriction, MovingType.on_stand);
                SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _completeMoveSideFriction, MovingType.on_stand);
            }
            else
            {
                SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _rotateOnStandSideFriction, MovingType.on_stand);
                SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _rotateOnStandSideFriction, MovingType.on_stand);
            }
        }
        else
        {
            if (_forwardAcceleration > 0)
            {
                // если поворот налево
                if (_rotateAcceleration < 0)
                {
                    totalLeftAcceleration = _forwardAcceleration + _rotateAcceleration;
                    totalRightAcceleration = _forwardAcceleration;
                }
                else if (_rotateAcceleration > 0)
                {
                    totalLeftAcceleration = _forwardAcceleration;
                    totalRightAcceleration = _forwardAcceleration - _rotateAcceleration;
                }
            }
            else if (_forwardAcceleration < 0)
            {
                // если поворот налево
                if (_rotateAcceleration < 0)
                {
                    totalLeftAcceleration = _forwardAcceleration - _rotateAcceleration;
                    totalRightAcceleration = _forwardAcceleration;
                }
                else if (_rotateAcceleration > 0)
                {
                    totalLeftAcceleration = _forwardAcceleration;
                    totalRightAcceleration = _forwardAcceleration + _rotateAcceleration;
                }
            }

            breakPowerScaler = _breakPowerScaler;

            // передаём прямое ускорение полученное по оси, обычное ограничение скорости, коэффициент скольжения и тип движения
            SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _directMoveVelocityLimit, _completeMoveSideFriction, MovingType.complette);
            SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _directMoveVelocityLimit, _completeMoveSideFriction, MovingType.complette);
        }

    }

    void SelectedSideMoving(ref WheelCollider[] collidersArray, float acceleration, float breakScaler, float maxVelocity, float frictionStiffnes, MovingType type)
    {
        for (int i = 0; i < collidersArray.Length; i++)
        {
            WheelFrictionCurve frictionSet = collidersArray[i].sidewaysFriction;

            switch (type)
            {
                case MovingType.autobreak:
                    frictionSet.stiffness = _defaultSideFriction;
                    collidersArray[i].sidewaysFriction = frictionSet;
                    collidersArray[i].motorTorque = (acceleration * _engineForce);
                    collidersArray[i].brakeTorque = (breakScaler * _brakeForce);
                    break;

                case MovingType.complette:

                case MovingType.direct:
                    if (_currentSpeed > maxVelocity)
                    {
                        frictionSet.stiffness = _defaultSideFriction;
                        collidersArray[i].sidewaysFriction = frictionSet;
                        collidersArray[i].brakeTorque = _brakeForce;
                    }
                    else
                    {
                        frictionSet.stiffness = frictionStiffnes;
                        collidersArray[i].sidewaysFriction = frictionSet;
                        collidersArray[i].brakeTorque = (breakScaler * _brakeForce);
                        collidersArray[i].motorTorque = (acceleration * _engineForce);
                    }
                    break;

                case MovingType.on_stand:
                    //if (Mathf.Abs(_angularSpeed.y) > maxVelocity)
                    if (_angularSpeed > maxVelocity)
                    {
                        frictionSet.stiffness = _defaultSideFriction;
                        collidersArray[i].sidewaysFriction = frictionSet;
                        collidersArray[i].brakeTorque = _brakeForce;
                    }
                    else
                    {
                        frictionSet.stiffness = frictionStiffnes;
                        collidersArray[i].sidewaysFriction = frictionSet;
                        collidersArray[i].brakeTorque = (breakScaler * _brakeForce);
                        collidersArray[i].motorTorque = (acceleration * _engineForce);
                    }
                    break;
            }
        }
    }

    void OnStandRotation()
    {
        for (int i = 0; i < _leftWheelsColliders.Length; i++)
        {
            WheelFrictionCurve frictionSet = _leftWheelsColliders[i].sidewaysFriction;

            if (Mathf.Abs(_rotateAcceleration) >= 0 && Mathf.Abs(_rotateAcceleration) <= _deltaHorizontal)
            {
                frictionSet.stiffness = 0.8f;
                _leftWheelsColliders[i].sidewaysFriction = frictionSet;
                _leftWheelsColliders[i].brakeTorque = _brakeForce;
            }
            else if(_angularSpeed > _rotateOnStandVelocityLimit)
            {
                frictionSet.stiffness = 0.8f;
                _leftWheelsColliders[i].sidewaysFriction = frictionSet;
                _leftWheelsColliders[i].brakeTorque = _brakeForce;
            }
            else
            {
                frictionSet.stiffness = 0.06f;
                _leftWheelsColliders[i].sidewaysFriction = frictionSet;
                _leftWheelsColliders[i].brakeTorque = 0;
                _leftWheelsColliders[i].motorTorque = (_rotateAcceleration * _engineForce);
            }
        }

        for (int i = 0; i < _rightWheelsColliders.Length; i++)
        {
            WheelFrictionCurve frictionSet = _rightWheelsColliders[i].sidewaysFriction;

            if (Mathf.Abs(_rotateAcceleration) >= 0 && Mathf.Abs(_rotateAcceleration) <= _deltaHorizontal)
            {
                frictionSet.stiffness = 0.8f;
                _rightWheelsColliders[i].sidewaysFriction = frictionSet;
                _rightWheelsColliders[i].brakeTorque = _brakeForce;
            }
            else if (_angularSpeed > _rotateOnStandVelocityLimit)
            {
                frictionSet.stiffness = 0.8f;
                _rightWheelsColliders[i].sidewaysFriction = frictionSet;
                _rightWheelsColliders[i].brakeTorque = _brakeForce;
            }
            else
            {
                frictionSet.stiffness = 0.06f;
                _rightWheelsColliders[i].sidewaysFriction = frictionSet;
                _rightWheelsColliders[i].brakeTorque = 0;
                _rightWheelsColliders[i].motorTorque = (-_rotateAcceleration * _engineForce);
            }
        }
    }

    void UpdateWheelTransform(WheelCollider colider, Transform transform)
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        colider.GetWorldPose(out position, out rotation);
        transform.position = position;
        transform.rotation = rotation;
    }

    void UpdateWheelTransform(ref WheelCollider[] coliders, ref Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            Vector3 position = transforms[i].position;
            Quaternion rotation = transforms[i].rotation;

            coliders[i].GetWorldPose(out position, out rotation);
            transforms[i].position = (position + new Vector3(0, _suspentionDistance, 0));
            transforms[i].rotation = rotation;
        }
    }

    void UpdateReferenceTransform(Transform reference, ref Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            Quaternion rotation = reference.rotation;
            transforms[i].rotation = rotation;
        }
    }
}