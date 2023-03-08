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
    public bool _Enable = false;                                       // включение скрипта

    [Header("Блок наполнения коллайдерами и трансформами")]
    [Tooltip("Коллайдеры ведущих колес левого борта")]
    public WheelCollider[] _leftWheelsColliders;                       // коллайдеры ведущих колес левого борта
    [Tooltip("Коллайдеры ведущих колес правого борта")]
    public WheelCollider[] _rightWheelsColliders;                      // коллайдеры ведущих колес правого борта

    [Tooltip("Трансформы ведущих колес левого борта")]
    public Transform[] _leftWheelsTransforms;                          // трансформы ведущих колес левого борта
    [Tooltip("Трансформы ведущих колес правого борта")]
    public Transform[] _rightWheelsTransforms;                         // трансформы ведущих колес правого борта

    [Tooltip("Трансформы зависимых колес левого борта")]
    public Transform[] _leftDependenceWheelsTransforms;                // трансформы зависимых колес левого борта
    [Tooltip("Трансформы зависимых колес правого борта")]
    public Transform[] _rightDependenceWheelsTransforms;               // трансформы зависимых колес правого борта

    [Tooltip("Трансформы костей гусеницы левого борта")]
    public Transform[] _leftTruckBones;                                // трансформы костей левого борта
    [Tooltip("Трансформы костей гусеницы правого борта")]
    public Transform[] _rightTruckBones;                               // трансформы костей правого борта

    [Tooltip("Базовый объект левой гусеницы")]
    public GameObject _leftTankTruck;                                  // базовый объект левой гусеницы
    [Tooltip("Базовый объект правой гусеницы")]
    public GameObject _rightTankTruck;                                 // базовый объект правой гусеницы

    [Header("Силовые характеристики двигателя и тормозов")]
    [Tooltip("Мощность двигателя")]
    public float _engineForce = 3400f;                // мощность двигателя
    public void SetEngineForce(float force) { _engineForce = force; }
    public float GetEngineForce() { return _engineForce; }
    [Tooltip("Мощность торможения")]
    public float _brakeForce = 5200f;                 // сила торможения
    public void SetBreakForce(float force) { _brakeForce = force; }
    public float GetBreakForce() { return _brakeForce; }

    [Header("Настройки максимальной скорости и бокового сольжения")]
    [Tooltip("Базовый множитель бокового сольжения гусениц")]
    public float _defaultSideFriction = 1f;           // базовый множитель бокового сольжения гусениц
    [Tooltip("Множитель бокового сольжения гусениц комплексного перемещения")]
    public float _completeMoveSideFriction = 0.6f;    // множитель бокового сольжения гусениц комплексного перемещения

    [Tooltip("Максимальная скорость движения")]
    public float _directMoveVelocityLimit = 10f;      // максимальная скорость движения
    public void SetDirectVelocityLimit(float limit) { _directMoveVelocityLimit = limit; }
    public float GetDirectVelocityLimit() { return _directMoveVelocityLimit; }
    [Tooltip("Множитель бокового скольжения при движении прямо")]
    public float _directMoveSideFriction = 0.9f;      // множитель бокового скольжения при движении прямо

    [Tooltip("Максимальная скорость вращения на месте")]
    public float _rotateOnStandVelocityLimit = 1f;    // максимальная скорость вращения на месте
    public void SetRotateVelocityLimit(float limit) { _rotateOnStandVelocityLimit = limit; }
    public float GetRotateVelocityLimit() { return _rotateOnStandVelocityLimit; }
    [Tooltip("Множитель бокового скольжения при развороте на месте")]
    public float _rotateOnStandSideFriction = 0.2f;   // множитель бокового скольжения при развороте на месте

    [Header("Настройки масс элементов танка и подвески")]
    [Tooltip("Активация заданных ниже параметров при начале симуляции")]
    public bool _useCustomData = false;               // активация заданных ниже параметров при начале симуляции
    [Tooltip("Общая масса танка в килограммах (базовый Rigidbody)")]
    public float _unitCustomMass = 50000f;            // общая масса танка в килограммах (базовый Rigidbody)
    [Space]
    [Tooltip("Сила воздействия пружины в Ньютонах. Для танка оптимально в 10 раз больше массы")]
    public float _wheelCustomSpring = 500000f;            // сила воздействия пружины в Ньютонах. Для танка оптимально в 10 раз больше массы
    [Tooltip("Демпфирующее сопротивление в Ньютонах. Для танка оптимально равное массе")]
    public float _wheelCustomDamper = 50000f;             // демпфирующее сопротивление в Ньютонах. Для танка оптимально равное массе
    [Tooltip("Высота подъема подвески в метрах. Для танка равна высоте трака гусеницы")]
    public float _wheelCustomTarget = 1f;               // высота подъема подвески в метрах. Для танка равна высоте трака гусеницы

    [Header("Погрешности положения осей джойстика")]
    [Tooltip("Погрешность отклонения джойстика по вертикали")]
    public float _deltaVertical = 0.02f;              // погрешность отклонения джойстика по вертикали
    private float _defDeltaVertical;                  // погрешность отклонения джойстика по вертикали
    public void SetDeltaVertical(float deltaV) { _deltaVertical = deltaV; }
    public float GetDeltaVertical() { return _deltaVertical; }
    public void SetDeltaVerticalDefault() { _deltaVertical = _defDeltaVertical; }
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

    private Quaternion _leftCalculateRotation;        // вращение для всех колес левого борта
    private Quaternion _rightCalculateRotation;       // вращение для всех колес правого борта
    private Rigidbody _currentBody;                   // тело над которым работает скрипт
    private float _suspentionDistance;                // высота подвески

    private float _leftTrackTextureOffset = 0.0f;     // оффсет текстуры левого трака
    private float _rightTrackTextureOffset = 0.0f;    // оффсет текстуры правого трака


    [Header("Блок дебаговых переменных")]
    [Tooltip("Показатель текущей скорости")]
    public float VELOCITY;
    public float GetDebugCurrentVelocity() { return VELOCITY; }
    [Tooltip("Показатель текущей скорости разворота по оси Y")]
    public float AXIS_VELOCITY;
    public float GetDebugCurrentAxisVelocity() { return AXIS_VELOCITY; }
    [Tooltip("Показатель текущей угловой cкорости")]
    public float ANGULAR_VELOCITY;
    [Tooltip("Множитель входящего ускорения по вертикальной оси")]
    public float FORWARD_ACCEL;
    public float GetDebugCurrentForwardAcceleration() { return FORWARD_ACCEL; }
    [Tooltip("Множитель входящего ускорения по горизонтальной оси")]
    public float ROTATION_ACCEL;
    public float GetDebugCurrentRotationAcceleration() { return ROTATION_ACCEL; }
    [Tooltip("Множитель текущего тормозного усилия")]
    public float BREAK_ACCEL;
    public float GetDebugCurrentBreakAcceleration() { return BREAK_ACCEL; }

    [Tooltip("Величина текущего вращательного момента на левом колесе")]
    public float LEFT_TORQUE;
    public float GetDebugLeftWheelTorque() { return LEFT_TORQUE; }
    [Tooltip("Величина текущего вращательного момента на правом колесе")]
    public float RIGHT_TORQUE;
    public float GetDebugRightWheelTorque() { return RIGHT_TORQUE; }
    [Tooltip("Величина текущего тормозящего момента момента на левом колесе")]
    public float LEFT_BREAK_TORQUE;
    public float GetDebugLeftBreakForce() { return LEFT_BREAK_TORQUE; }
    [Tooltip("Величина текущего тормозящего момента момента на правом колесе")]
    public float RIGHT_BREAK_TORQUE;
    public float GetDebugRightBreakForce() { return RIGHT_BREAK_TORQUE; }
    [Tooltip("Величина текущего множителя бокового сольжения на левом колесе")]
    public float SIDEWAY_STIFFNESS;

    private enum MovingType
    {
        direct, on_stand, complette, autobreak, out_of_hit
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
        // запись текущего рижитбади
        _currentBody = GetComponent<Rigidbody>();

        // запоминаем изначально заданные погрешности отклонения джойстика
        _defDeltaVertical = _deltaVertical;
        _defDeltaHorizontal = _deltaHorizontal;

        UpdateCustomData();                        // записываем заданные кастомные параметры объекта

        if (_leftWheelsTransforms.Length != 0)
        {
            // если есть колеса, то присваеваем основному общему вращению левого борта стартовый поворот среднего колеса
            _leftCalculateRotation = _leftWheelsTransforms[_leftWheelsTransforms.Length / 2].localRotation;
        }

        if (_rightWheelsTransforms.Length != 0)
        {
            // если есть колеса, то присваеваем основному общему вращению правого борта стартовый поворот среднего колеса
            _rightCalculateRotation = _rightWheelsTransforms[_rightWheelsTransforms.Length / 2].localRotation;
        }
    }

    void FixedUpdate()
    {
        if (_Enable)
        {
            UpdateCurrentSpeed();                      // записываем показатели текущей скорости
            UpdateInputsData();                        // записываем информацию по положению осей джойстика

            CompletteDrive();                          // комплексная функция движения

            UpdateDebugData();                         // обновления дебаговых переменных
            UpdateWheelTransforms();                   // обновляем положения колёс
        }
    }

    // ----------------------------------------------- блок основных апдейтов по ходу действия скрипта ------------------------------

    // обновление кастомных параметров объекта
    void UpdateCustomData()
    {
        if (_useCustomData)
        {
            // если есть физическое тело, то присваиваем ему данные из поля массы
            if (_currentBody != null) _currentBody.mass = _unitCustomMass;

            if (_leftWheelsColliders.Length != 0)
            {
                // если есть коллайдеры колес, то обновляем подвеску в них
                for (int i = 0; i != _leftWheelsColliders.Length; ++i)
                {
                    JointSpring sp = _leftWheelsColliders[i].suspensionSpring;
                    sp.spring = _wheelCustomSpring;
                    sp.damper = _wheelCustomDamper;
                    sp.targetPosition = _wheelCustomTarget;

                    _leftWheelsColliders[i].suspensionSpring = sp;
                }
            }

            if (_rightWheelsColliders.Length != 0)
            {
                // если есть коллайдеры колес, то обновляем подвеску в них
                for (int i = 0; i != _rightWheelsColliders.Length; ++i)
                {
                    JointSpring sp = _rightWheelsColliders[i].suspensionSpring;
                    sp.spring = _wheelCustomSpring;
                    sp.damper = _wheelCustomDamper;
                    sp.targetPosition = _wheelCustomTarget;

                    _rightWheelsColliders[i].suspensionSpring = sp;
                }
            }
        }
    }
    // обновление дебаговых данных
    void UpdateDebugData()
    {
        VELOCITY = _currentSpeed;
        AXIS_VELOCITY = _angularSpeed;

        FORWARD_ACCEL = _forwardAcceleration;
        ROTATION_ACCEL = _rotateAcceleration;
        BREAK_ACCEL = _breakPowerScaler;

        LEFT_TORQUE = _leftWheelsColliders[2].motorTorque;
        RIGHT_TORQUE = _rightWheelsColliders[2].motorTorque;
        LEFT_BREAK_TORQUE = _leftWheelsColliders[2].brakeTorque;
        RIGHT_BREAK_TORQUE = _rightWheelsColliders[2].brakeTorque;
        SIDEWAY_STIFFNESS = _leftWheelsColliders[2].sidewaysFriction.stiffness;
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
    // получение данных по текущей прямолинейной и угловой скоростям
    void UpdateCurrentSpeed()
    {
        _angularSpeed = _currentBody.angularVelocity.magnitude;           // обновляем угловую скорость
        _currentSpeed = _currentBody.velocity.magnitude;                  // обновляем обычную скорость
    }
    // вызов обновления положения колес танка
    void UpdateWheelTransforms()
    {
        // комплексная функция обновления возвышения колес и костей гусеницы, а также расчёта ссылок на вращение
        UpdateWheelTransform(ref _leftTrackTextureOffset, ref _leftCalculateRotation, ref _leftWheelsColliders, ref _leftWheelsTransforms, ref _leftTruckBones);
        UpdateWheelTransform(ref _rightTrackTextureOffset, ref _rightCalculateRotation, ref _rightWheelsColliders, ref _rightWheelsTransforms, ref _rightTruckBones);

        // обновляем вращение колес левого борта
        UpdateWheelsRotation(ref _leftCalculateRotation, ref _leftWheelsTransforms);
        UpdateWheelsRotation(ref _leftCalculateRotation, ref _leftDependenceWheelsTransforms);
        UpdateTruckTextureOffset(ref _leftTrackTextureOffset, ref _leftTankTruck);

        // обновляем вращение колес правого борта
        UpdateWheelsRotation(ref _rightCalculateRotation, ref _rightWheelsTransforms);
        UpdateWheelsRotation(ref _rightCalculateRotation, ref _rightDependenceWheelsTransforms);
        UpdateTruckTextureOffset(ref _rightTrackTextureOffset, ref _rightTankTruck);
    }

    // ----------------------------------------------- блок расчёта движения танка и обновления трансформов -------------------------

    // основная функция вызова движения, определяет требуемый тип движения в зависимости от данных полученных по осям управления и текущей скорости юнита
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
            UpdateWheelColliders(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, 0, _defaultSideFriction, MovingType.autobreak);
            UpdateWheelColliders(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, 0, _defaultSideFriction, MovingType.autobreak);
        }

        // если передаётся вертикальное ускорение, но горизонтального нет - прямолинейное движение
        else if (Mathf.Abs(_forwardAcceleration) > _deltaVertical && Mathf.Abs(_rotateAcceleration) <= _deltaHorizontal)
        {
            totalLeftAcceleration = _forwardAcceleration;
            totalRightAcceleration = _forwardAcceleration;
            breakPowerScaler = _breakPowerScaler;

            // передаём прямое ускорение полученное по оси, обычное ограничение скорости, коэффициент скольжения и тип движения
            UpdateWheelColliders(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _directMoveVelocityLimit, _directMoveSideFriction, MovingType.direct);
            UpdateWheelColliders(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _directMoveVelocityLimit, _directMoveSideFriction, MovingType.direct);

        }

        // если передаётся горизонтальное ускорение, но вертикального нет - разворот на пятачке
        else if (Mathf.Abs(_forwardAcceleration) <= _deltaVertical && Mathf.Abs(_rotateAcceleration) > _deltaHorizontal)
        {
            totalLeftAcceleration = _rotateAcceleration;
            totalRightAcceleration = -_rotateAcceleration;
            breakPowerScaler = _breakPowerScaler;

            if (_currentSpeed > 1f)
            {
                // передаём прямое ускорение полученное по оси, обычное ограничение скорости, коэффициент скольжения и тип движения
                UpdateWheelColliders(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _completeMoveSideFriction, MovingType.on_stand);
                UpdateWheelColliders(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _completeMoveSideFriction, MovingType.on_stand);
            }
            else
            {
                UpdateWheelColliders(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _rotateOnStandSideFriction, MovingType.on_stand);
                UpdateWheelColliders(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _rotateOnStandSideFriction, MovingType.on_stand);
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
            UpdateWheelColliders(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _directMoveVelocityLimit, _completeMoveSideFriction, MovingType.complette);
            UpdateWheelColliders(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _directMoveVelocityLimit, _completeMoveSideFriction, MovingType.complette);
        }

    }
    // обновление параметров крутящего момента и торможения коллайдеров
    void UpdateWheelColliders(ref WheelCollider[] collidersArray, float acceleration, float breakScaler, float maxVelocity, float frictionStiffnes, MovingType type)
    {
        for (int i = 0; i < collidersArray.Length; i++)
        {
            WheelFrictionCurve frictionSet = collidersArray[i].sidewaysFriction;

            if (!collidersArray[i].GetGroundHit(out WheelHit hit))
            {
                frictionSet.stiffness = _defaultSideFriction;
                collidersArray[i].sidewaysFriction = frictionSet;
                collidersArray[i].motorTorque = 0;
                collidersArray[i].brakeTorque = _brakeForce;
            }

            else
            {
                switch (type)
                {
                    case MovingType.autobreak:
                        frictionSet.stiffness = _defaultSideFriction;
                        collidersArray[i].sidewaysFriction = frictionSet;
                        collidersArray[i].motorTorque = (acceleration * _engineForce);
                        collidersArray[i].brakeTorque = (breakScaler * _brakeForce);
                        break;

                    case MovingType.complette:
                        // проваливаемся вниз и работаем так же как и директ мув
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

                    case MovingType.out_of_hit:
                        frictionSet.stiffness = _defaultSideFriction;
                        collidersArray[i].sidewaysFriction = frictionSet;
                        collidersArray[i].motorTorque = 0;
                        collidersArray[i].brakeTorque = _brakeForce;
                        break;
                }
            }
        }
    }
    // комплексное обновление трансформов колес
    void UpdateWheelTransform(ref float texture_offset, ref Quaternion rotation, ref WheelCollider[] coliders, ref Transform[] transforms, ref Transform[] bones)
    {
        float CalculateRPM = 0;
        float dev_rpm = 0.0f;
        int dev_count = 0;
        
        // начинаем проход по всем колесам
        for (int i = 0; i < transforms.Length; i++)
        {
            // получаем данные по текущей позиции и вращению коллайдера
            Vector3 col_position = transforms[i].position;
            Quaternion col_rotation = transforms[i].rotation;
            coliders[i].GetWorldPose(out col_position, out col_rotation);

            // если коллайдер колеса имеет касание с поверхностью, то он будет крутиться корреткно
            if (coliders[i].GetGroundHit(out WheelHit hit) && coliders[i].rpm != 0)
            {
                dev_rpm += coliders[i].rpm;
                dev_count++;
            }

            // оброаботка возвышения соответствующего меша колеса
            UpdateWheelPosition(ref col_position, 0.03f, ref transforms[i]);

            // обработка возвышения соответсвующей кости гусеницы
            UpdateTrackBonePosition(ref col_position, coliders[i].radius, ref bones[i]);
        }

        if (dev_count != 0)
        {
            // если есть хоть одно колесо имеющее касание с поверхностью
            CalculateRPM = dev_rpm / dev_count;   // высчитываем среднюю скорость вращения
        }

        rotation = Quaternion.Euler((CalculateRPM / 10), 0, 0) * rotation;
        texture_offset = texture_offset + (CalculateRPM / 3600);
    }
    // обновление позиции колеса
    void UpdateWheelPosition(ref Vector3 col_position, float track_hight, ref Transform wheel_transform)
    {
        Vector3 wheel_position = wheel_transform.position;
        // берем возвышение от колайдера и прибавляем высоту полотна гусеницы
        wheel_position.y = col_position.y + track_hight;
        // перезаписываем данные
        wheel_transform.position = wheel_position;
    }
    // обновление поцизии кости гусеницы
    void UpdateTrackBonePosition(ref Vector3 col_position, float col_radius, ref Transform bone_transform)
    {
        Vector3 bone_position = bone_transform.position;
        // берем возвышение от коллайдера, вычитаем радиус диска и высоту подвески
        bone_position.y = (col_position.y - col_radius);
        // перезаписываем данные
        bone_transform.position = bone_position;
    } 
    // обновление вращения колес
    void UpdateWheelsRotation(ref Quaternion rotation, ref Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].localRotation = rotation;
        }
    }
    // обвновлеие положения текстуры гусеницы
    void UpdateTruckTextureOffset(ref float offset, ref GameObject track)
    {
        track.GetComponent<SkinnedMeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0, offset));
    }
}