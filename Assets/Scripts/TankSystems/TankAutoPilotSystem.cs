using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static TankAutoPilotSystem;
using static UnityEngine.GraphicsBuffer;

public class TankAutoPilotSystem : MonoBehaviour
{
    [Tooltip("Включить выполнение скрипта")]
    public bool _Enable = false;

    public enum SystemMode
    {
        None, AutoMovement, OnStandRotaion
    }

    [Header("Выбор режима работы системы автопилота")]
    [Tooltip("Режим работы автопилота")]
    public SystemMode _autoPilotMode = SystemMode.None;

    [Header("Назначение цели перемещения танка")]
    [Tooltip("Текущая цель перемещения танка")]
    public GameObject _targetObject;
    [Tooltip("Объект-маркер, для задачи текущей цели")]
    public GameObject _targetMarkerOne;                             // объект-маркер, для задачи текущей цели
    [Tooltip("Объект-маркер, для задачи цели следующей за текущей")]
    public GameObject _targetMarkerTwo;                             // объект-маркер, для задачи цели следующей за текущей

    public enum TargetType
    {
        CheckPoint, FinalPoint                                   // тип полученной цели - проходная или конечная точка пути
    }
    [Tooltip("Тип полученной цели - проходная или конечная точка пути")]
    public TargetType _targetType = TargetType.FinalPoint;       // по умолчанию задаётся как конечная - точка остановки

    private GameObject _currentTarget;                           // текущая цель, куда стремимся

    public enum PathMode
    {
        Direct, NavMesh
    }

    [Header("Выбор режима построения пути до цели")]
    [Tooltip("Режим построения пути до цели")]
    public PathMode _pathMode = PathMode.Direct;

    [Header("Блок параметров работы системы автопилота")]
    [Tooltip("Показатель среднего превышения для смены режима навигации")]
    public float _navSysSelectDelta = 2f;
    [Tooltip("Удаление от цели для начала торможения танка")]
    public float _breakingPointDistance = 20f;


    [Header("Отладочная информация из подсистемы навигации")]
    [Tooltip("Включить отображение направлений от бортов танка")]
    public bool ENABLE_TANK_GIZMO_DIRECTION = false;
    [Tooltip("Включить отображение направлений от бортов танка до цели")]
    public bool ENABLE_TANK_TO_TARGET_RAY = false;

    [Header("Отладочная информация из подсистемы движения")]
    [Tooltip("Включить загрузку данных из скрипта перемещения")]
    public bool GET_DEBUG_MOVING_SYSTEM_DATA = false;
    [Tooltip("Показатель множителя ускорения в прямом направлении")]
    public float DEBUG_FORWARD_ACCELERATION;
    [Tooltip("Показатель множителя ускорения поворота")]
    public float DEBUG_ROTATION_ACCELERATION;
    [Tooltip("Показатель множителя тормозного усилия")]
    public float DEBUG_BREAK_ACCELERATION;

    [Space]
    [Tooltip("Показатель текущей скорости")]
    public float DEBUG_CURRENT_VELOCITY;
    [Tooltip("Показатель текущей скорости разворота по оси Y")]
    public float DEBUG_CURRENT_ROTATION;

    [Space]
    [Tooltip("Показатель величины крутящего момента на левом борту")]
    public float DEBUG_LEFT_WHEEL_TORQUE;
    [Tooltip("Показатель величины крутящего момента на правом борту")]
    public float DEBUG_RIGHT_WHEEL_TORQUE;
    [Tooltip("Показатель величины тормозного усилия на левом борту")]
    public float DEBUG_LEFT_BREAK_FORCE;
    [Tooltip("Показатель величины тормозного усилия на правом борту")]
    public float DEBUG_RIGHT_BREAK_FORCE;

    private TankTruckMovingSystem _tankMovingSystem;             // подсистема движения танка
    private float _engineForce = 0f;                             // мощность двигателя танка
    private float _breakForce = 0f;                              // сила торможения
    private float _velocityLimit = 0f;                           // ограничитель скорости
    private float _rotationLimit = 0f;                           // ограничитель угловой скорости

    private TankNavigationPointSystem _navigationPointSystem;    // подсистема навигации на плоскости
    private TankNavigationPathSystem _navigationPathSystem;      // подсистема построения пути

    private OnStandRotation _ModuleOnStandRotation;
    private SR_AutoMovement _Module_SR_AutoMovement;
    private LR_AutoMovement _Module_LR_AutoMovement;


    // ------------------------------------- блок публичных сеттеров и геттеров класса ------------------------------------------------

    // установка и получение текущей основной цели
    public void SetTargetObject(GameObject target) { _targetObject = target; }
    public GameObject GetTargetObject() { return _targetObject; }
    // установка и получение режима работы автопилота
    public void SetSystemMode(SystemMode mode) { _autoPilotMode = mode; }
    public SystemMode GetSystemModee() { return _autoPilotMode; }
    // установка и получение типа цели
    public void SetTargetType(TargetType type) { _targetType = type; }
    public TargetType GetTargetType() { return _targetType; }
    // установка и получение режима построения пути до цели
    public void SetPathMode(PathMode mode) { _pathMode = mode; }
    public PathMode GetPathModee() { return _pathMode; }

    // ------------------------------------- блок публичных методов задачи булевых флагов ---------------------------------------------

    public void SetSubSystemEnable(bool enable) { _Enable = enable; }
    public bool HaveATarget() { return _targetObject != null; }

    // ------------------------------------- погружаемые данные из навигационной системы ----------------------------------------------

    private float _centerToMarkerOneDistance;                    // дистанция до первого маркера от центральной точки танка
    private float _frontToMarkerOneDistance;                     // дистанция до первого маркера от передней кромки
    private float _rearToMarkerOneDistance;                      // дистанция до первого маркера от задней кромки
    private float _leftToMarkerOneDistance;                      // дистанция до первого маркера от левой кромки
    private float _rightToMarkerOneDistance;                     // дистанция до первого маркера от правой кромки

    private float _centerMarkerOneDotProduct;                    // текущее скалярное произведение векторов направления от центра и первого маркера
    private float _frontMarkerOneDotProduct;                     // текущее скалярное произведение векторов направления шасси и первого маркера
    private float _rearMarkerOneDotProduct;                      // текущее скалярное произведение обратного вектора направления и первого маркера
    private float _leftMarkerOneDotProduct;                      // текущее скалярное произведение левого вектора направления и первого маркера
    private float _rightMarkerOneDotProduct;                     // текущее скалярное произведение правого вектора направления и первого маркера

    private float _centerToMarkerTwoDistance;                    // дистанция до второго маркера от центральной точки танка
    private float _frontToMarkerTwoDistance;                     // дистанция до второго маркера от передней кромки
    private float _rearToMarkerTwoDistance;                      // дистанция до второго маркера от задней кромки
    private float _leftToMarkerTwoDistance;                      // дистанция до второго маркера от левой кромки
    private float _rightToMarkerTwoDistance;                     // дистанция до второго маркера от правой кромки

    private float _centerMarkerTwoDotProduct;                    // текущее скалярное произведение векторов направления от центра и второго маркера
    private float _frontMarkerTwoDotProduct;                     // текущее скалярное произведение векторов направления шасси и второго маркера
    private float _rearMarkerTwoDotProduct;                      // текущее скалярное произведение обратного вектора направления и второго маркера
    private float _leftMarkerTwoDotProduct;                      // текущее скалярное произведение левого вектора направления и второго маркера
    private float _rightMarkerTwoDotProduct;                     // текущее скалярное произведение правого вектора направления и второго маркера

    private float _markerToMarkerDistance;                       // дистанция от первого до второго маркера
    private float _markerDotProduct;                             // текущее скалярное произведение направления до первого маркера и от первого и до второго

    // ------------------------------------- специальные булевые флаги состояния движения ---------------------------------------------

    [SerializeField]
    private bool _OnStandRotation = false;                       // флаг необходимости доворота на точке
    [SerializeField]
    private bool _OnStandPrecise = false;                        // флаг необходимости точного доворота на точке
    [SerializeField]
    private bool _OnMoveRotation = false;                        // флаг необходимости доворота во время движения
    [SerializeField]
    private bool _OnMoveTurning = false;                         // флаг необходимости поворота во время движения

    private enum MovementType
    {
        // типы движения которые необходимо совершить танку, чтобы достичь цели
        // LR - LongRange, SR - ShortRange.
        // OnStandRotation - вращение на месте
        // OnStandPrecise - точное позиционирование на месте
        // OnMoveRotation - позиционирование во время движения
        // OnMoveTurning - большой поворот во время движения
        // OnMovePrecise - точный доворот в движении
        // OnMoveBreaking - торможение в движении
        // OnStangParking - стоянка или остановка
        LR_OnStandRotation, LR_OnStandPrecise, LR_OnMoveRotation, LR_OnMoveTurning, LR_OnMovePrecise, LR_OnMoveBreaking, 
        SR_OnStandRotation, SR_OnStandPrecise, SR_OnMoveRotation, SR_OnMoveTurning, SR_OnMovePrecise, SR_OnMoveBreaking,
        OnStangParking
    }

    [SerializeField]
    private MovementType _movementType = MovementType.OnStangParking;

    private bool _ExtremCrashStop = false;                       // флаг необходимости экстренной остановки

    private void Awake()
    {
        // при запуске подгружаем требуемые системы танка
        _tankMovingSystem = GetComponent<TankTruckMovingSystem>();

        _engineForce = _tankMovingSystem.GetEngineForce();
        _breakForce = _tankMovingSystem.GetBreakForce();
        _velocityLimit = _tankMovingSystem.GetDirectVelocityLimit();
        _rotationLimit = _tankMovingSystem.GetRotateVelocityLimit();

        _navigationPointSystem = GetComponent<TankNavigationPointSystem>();
        _navigationPathSystem = GetComponent<TankNavigationPathSystem>();

        _ModuleOnStandRotation = new OnStandRotation(this);
        _Module_SR_AutoMovement = new SR_AutoMovement(this);
        _Module_LR_AutoMovement = new LR_AutoMovement(this);
    }

    private void FixedUpdate()
    {
        UpdateAutoPilotMode();                    // назначение режима управления в подсистеме движения
        UpdateTargetRedirect();                   // определяем текущую цель для достижения
        UpdateNavSystemTarget();                  // проверяем и загружаем цель в навигационные подсистемы
        UpdateDebugMovingData();                  // подгрузка отладочных данных из подсистемы движения
        UpdateDebugNavigationData();              // подгрузка отладочных данных из подсистемы навигации
        UpdateBreakingPointDistance();            // динамический пересчёт расстояния до начала остановки
        UpdateCurrentMovementType();              // обновление текущего режима движения

        //AutoPilotMovement();
        ToTargetAutoMoving();
    }

    // назначение режима управления в подсистеме движения
    void UpdateAutoPilotMode()
    {
        if (_Enable)
        {
            switch (_autoPilotMode)
            {
                case SystemMode.None:
                    // переводим систему перемещения в режим работы с игроком
                    _tankMovingSystem.SetInputType(TankTruckMovingSystem.InputType.Player);
                    _tankMovingSystem.SetDeltaVerticalDefault();       // восстанавливаем погрешность ввода по вертикальной оси
                    _tankMovingSystem.SetDeltaHorizontalDefault();     // восстанавливаем погрешность ввода по горизонтальной оси
                    break;

                case SystemMode.AutoMovement:

                case SystemMode.OnStandRotaion:
                    // переводим систему перемещения в режим работы с автопилотом
                    // подразумевает работу с точными данными и без упрощения управления
                    _tankMovingSystem.SetInputType(TankTruckMovingSystem.InputType.AutoPilot);
                    _tankMovingSystem.SetDeltaVertical(0);       // убираем погрешность ввода по вертикальной оси
                    _tankMovingSystem.SetDeltaHorizontal(0);     // убираем погрешность ввода по горизонтальной оси
                    break;
            }
        }
    }
    // определяем текущую цель для достижения
    void UpdateTargetRedirect()
    {
        if (_Enable)
        {
            if (_targetObject != null)
            {
                switch (_autoPilotMode)
                {
                    case SystemMode.OnStandRotaion:
                        _pathMode = PathMode.Direct;
                        _navigationPathSystem.SetSubSystemEnable(false);  // отключаем систему построения путей
                        _targetMarkerOne.transform.position = _targetObject.transform.position;
                        _targetMarkerTwo.transform.position = _targetObject.transform.position;
                        _currentTarget = _targetObject;            // передаем в качестве расчётной цели текущий объект
                        _targetType = TargetType.FinalPoint;

                        break;


                    case SystemMode.AutoMovement:

                        switch (_pathMode)
                        {
                            // если выбран режим прямого построения пути
                            case PathMode.Direct:
                                _navigationPathSystem.SetSubSystemEnable(false);  // отключаем систему построения путей
                                _targetMarkerOne.transform.position = _targetObject.transform.position;
                                _targetMarkerTwo.transform.position = _targetObject.transform.position;
                                _currentTarget = _targetObject;            // передаем в качестве расчётной цели текущий объект
                                break;

                            // если обращаемся к системе построения пути
                            case PathMode.NavMesh:
                                _navigationPathSystem.SetSubSystemEnable(true);                                    // активируем работу системы
                                _navigationPathSystem.SetTargetObject(_targetObject);                              // передаем системе конечную цель
                                _targetMarkerOne.transform.position = _navigationPathSystem.GetTargetMarkerOne();  // получаем координаты ближайшего маркера
                                _targetMarkerTwo.transform.position = _navigationPathSystem.GetTargetMarkerTwo();  // получаем координаты следующего маркера
                                _currentTarget = _targetMarkerOne;                                          // назначаем целью первый маркер

                                if (_navigationPathSystem.GetTargetMarkersCount() > 2)
                                {
                                    // если в системе построения пути вершин больше двух
                                    // выставляем тип цели - проходная точка, чтобы не тормозить на ней
                                    _targetType = TargetType.CheckPoint;
                                }
                                else
                                {
                                    // иначе назначаем точку финальной позицией куда надо приехать
                                    _targetType = TargetType.FinalPoint;
                                }

                                break;
                        }

                        break;
                }
            }
            else
            {
                // если цели нет - сбрасываем трансформы маркерам
                _targetMarkerOne.transform.position = transform.position;
                _targetMarkerTwo.transform.position = transform.position;
                _navigationPathSystem.SetTargetObject(null);                                // отключаем цель у системы поиска пути
                _navigationPathSystem.SetSubSystemEnable(false);                            // выключаем систему поиска пути
                _targetType = TargetType.FinalPoint;                                 // переназначаем тип цели
                _currentTarget = null;                                               // обнуляем текушую цель
            }
        }
    }
    // загрузка данных в навигационные подсистемы
    void UpdateNavSystemTarget()
    {
        if (_Enable)
        {
            if (_targetMarkerOne != null && _targetMarkerOne.transform.position != transform.position)
            {
                _navigationPointSystem.SetTargetMarkerOne(_targetMarkerOne);
            }

            if (_targetMarkerTwo != null && _targetMarkerTwo.transform.position != transform.position)
            {
                _navigationPointSystem.SetTargetMarkerTwo(_targetMarkerTwo);
            }
        }
    }

    // загрузка данных из скрипта перемещения
    void UpdateDebugMovingData()
    {
        if (_Enable)
        {
            // если включен флаг загрузки данных и подсистема движения танка активна
            if (GET_DEBUG_MOVING_SYSTEM_DATA && _tankMovingSystem != null)
            {
                DEBUG_CURRENT_VELOCITY = _tankMovingSystem.GetDebugCurrentVelocity();
                DEBUG_CURRENT_ROTATION = _tankMovingSystem.GetDebugCurrentAxisVelocity();
                DEBUG_FORWARD_ACCELERATION = _tankMovingSystem.GetDebugCurrentForwardAcceleration();
                DEBUG_ROTATION_ACCELERATION = _tankMovingSystem.GetDebugCurrentRotationAcceleration();
                DEBUG_BREAK_ACCELERATION = _tankMovingSystem.GetDebugCurrentBreakAcceleration();

                DEBUG_LEFT_WHEEL_TORQUE = _tankMovingSystem.GetDebugLeftWheelTorque();
                DEBUG_RIGHT_WHEEL_TORQUE = _tankMovingSystem.GetDebugRightWheelTorque();
                DEBUG_LEFT_BREAK_FORCE = _tankMovingSystem.GetDebugLeftBreakForce();
                DEBUG_RIGHT_BREAK_FORCE = _tankMovingSystem.GetDebugRightBreakForce();
            }
            else
            {
                DEBUG_CURRENT_VELOCITY = 0; DEBUG_CURRENT_ROTATION = 0;
                DEBUG_FORWARD_ACCELERATION = 0; DEBUG_ROTATION_ACCELERATION = 0;
                DEBUG_BREAK_ACCELERATION = 0;

                DEBUG_LEFT_WHEEL_TORQUE = 0; DEBUG_RIGHT_WHEEL_TORQUE = 0;
                DEBUG_LEFT_BREAK_FORCE = 0; DEBUG_RIGHT_BREAK_FORCE = 0;
            }
        }
    }
    // подгрузка отладочных данных из подсистемы навигации
    void UpdateDebugNavigationData()
    {
        if (_Enable)
        {
            // включение отображение навигационных линий

            _navigationPointSystem.SetDirectionRayEnable(ENABLE_TANK_GIZMO_DIRECTION);
            _navigationPointSystem.SetToTargetRayEnable(ENABLE_TANK_TO_TARGET_RAY);

            // подгрузка данных по первому маркеру

            _centerToMarkerOneDistance = _navigationPointSystem.GetCenterToMarkerOneDistance();
            _frontToMarkerOneDistance = _navigationPointSystem.GetFrontToMarkerOneDistance();
            _rearToMarkerOneDistance = _navigationPointSystem.GetRearToMarkerOneDistance();
            _leftToMarkerOneDistance = _navigationPointSystem.GetLeftToMarkerOneDistance();
            _rightToMarkerOneDistance = _navigationPointSystem.GetRightToMarkerOneDistance();

            _centerMarkerOneDotProduct = _navigationPointSystem.GetCenterMarkerOneDotProduct();
            _frontMarkerOneDotProduct = _navigationPointSystem.GetFrontMarkerOneDotProduct();
            _rearMarkerOneDotProduct = _navigationPointSystem.GetRearMarkerOneDotProduct();
            _leftMarkerOneDotProduct = _navigationPointSystem.GetLeftMarkerOneDotProduct();
            _rightMarkerOneDotProduct = _navigationPointSystem.GetRightMarkerOneDotProduct();

            // подгрузка данных по второму маркеру

            _centerToMarkerTwoDistance = _navigationPointSystem.GetCenterToMarkerTwoDistance();
            _frontToMarkerTwoDistance = _navigationPointSystem.GetFrontToMarkerTwoDistance();
            _rearToMarkerTwoDistance = _navigationPointSystem.GetRearToMarkerTwoDistance();
            _leftToMarkerTwoDistance = _navigationPointSystem.GetLeftToMarkerTwoDistance();
            _rightToMarkerTwoDistance = _navigationPointSystem.GetRightToMarkerTwoDistance();

            _centerMarkerTwoDotProduct = _navigationPointSystem.GetCenterMarkerTwoDotProduct();
            _frontMarkerTwoDotProduct = _navigationPointSystem.GetFrontMarkerTwoDotProduct();
            _rearMarkerTwoDotProduct = _navigationPointSystem.GetRearMarkerTwoDotProduct();
            _leftMarkerTwoDotProduct = _navigationPointSystem.GetLeftMarkerTwoDotProduct();
            _rightMarkerTwoDotProduct = _navigationPointSystem.GetRightMarkerTwoDotProduct();

            // подгрузка прочих данных навигационной системы

            _markerToMarkerDistance = _navigationPointSystem.GetMarkerToMarkerDistance();
            _markerDotProduct = _navigationPointSystem.GetMarkerDotProduct();
        }
    }
    // динамический пересчёт расстояния до начала остановки
    void UpdateBreakingPointDistance()
    {
        _breakingPointDistance = ((_velocityLimit / 2) * 10) * (DEBUG_CURRENT_VELOCITY / _velocityLimit);
    }
    // обновление текущего режима движения
    void UpdateCurrentMovementType()
    {
        if (_autoPilotMode == SystemMode.AutoMovement)
        {
            //switch (_Module_LR_AutoMovement.GetCurrentMovementType())
            //{
            //    case LR_AutoMovement.MovementType.LR_OnStandRotation:
            //        _movementType = MovementType.LR_OnStandRotation; break;

            //    case LR_AutoMovement.MovementType.LR_OnStandPrecise:
            //        _movementType = MovementType.LR_OnStandPrecise; break;

            //    case LR_AutoMovement.MovementType.LR_OnMoveTurning:
            //        _movementType = MovementType.LR_OnMoveTurning; break;

            //    case LR_AutoMovement.MovementType.LR_OnMoveRotation:
            //        _movementType = MovementType.LR_OnMoveRotation; break;

            //    case LR_AutoMovement.MovementType.LR_OnMovePrecise:
            //        _movementType = MovementType.LR_OnMovePrecise; break;

            //    case LR_AutoMovement.MovementType.LR_OnMoveBreaking:
            //        _movementType = MovementType.LR_OnMoveBreaking; break;
            //}

            switch (_Module_SR_AutoMovement.GetCurrentMovementType())
            {
                case SR_AutoMovement.MovementType.SR_OnStandRotation:
                    _movementType = MovementType.SR_OnStandRotation; break;

                case SR_AutoMovement.MovementType.SR_OnStandPrecise:
                    _movementType = MovementType.SR_OnStandPrecise; break;

                case SR_AutoMovement.MovementType.SR_OnMoveTurning:
                    _movementType = MovementType.SR_OnMoveTurning; break;

                case SR_AutoMovement.MovementType.SR_OnMoveRotation:
                    _movementType = MovementType.SR_OnMoveRotation; break;

                case SR_AutoMovement.MovementType.SR_OnMovePrecise:
                    _movementType = MovementType.SR_OnMovePrecise; break;

                case SR_AutoMovement.MovementType.SR_OnMoveBreaking:
                    _movementType = MovementType.SR_OnMoveBreaking; break;


                case SR_AutoMovement.MovementType.OnTargetParking:
                    _movementType = MovementType.OnStangParking; break;
            }
        }
    }

    // алгоритм точного доворота на месте, вызывается обычной функцией разворота
    void ToTargetOnStandPrecision()
    {
        if (_targetObject != null && _OnStandPrecise)           // дополнительная проверка на всякий случай
        {
            _tankMovingSystem.SetForwardAcceleration(0);        // ставим ускорение прямого хода на ноль
            _tankMovingSystem.SetBreakPowerScaler(0);           // ставим множитель тормозного усилия в ноль

            // если почти смотрим на объект, то останавливаем движение танка и снимаем флаг необходимости разворота на месте
            if ((1 - _frontMarkerOneDotProduct) < 0.00001f || (1 - _frontMarkerOneDotProduct) > 0.001f)
            {
                _tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
                _tankMovingSystem.SetBreakPowerScaler(1);       // тормозное усилие на макисмум
                _OnStandPrecise = false;
            }
            // иначе мелкими импульсами довершаем вращение
            else
            {
                // проверяем разницу в скалярных произведений боковых векторов
                if (_leftMarkerOneDotProduct > _rightMarkerOneDotProduct)
                {
                    // для поворота налево передаем отрицательное вычисленное значение ускорения
                    _tankMovingSystem.SetRotateAcceleration(-CalculateOnStandRotationScaler() * 500);
                }
                else if (_leftMarkerOneDotProduct < _rightMarkerOneDotProduct)
                {
                    _tankMovingSystem.SetRotateAcceleration(CalculateOnStandRotationScaler() * 500);
                }
                else
                {
                    _tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
                    _tankMovingSystem.SetBreakPowerScaler(1);       // тормозное усилие на макисмум
                }
            }
        }
    }

    // алгоритм доворота в процессе горизонтального перемещения
    void ToTargetOnMoveRotation()
    {
        if (_targetObject != null && _OnMoveRotation)           // дополнительная проверка на всякий случай
        {
            // для начала начинаем движение
            _tankMovingSystem.SetForwardAcceleration(CalculateRelativeForwardScaler());
            _tankMovingSystem.SetBreakPowerScaler(CalculateRelativeBreakPowerScaler());

            // если довернули, то снимаем флаг, дальше будет езда чисто по прямой
            if ((1 - _frontMarkerOneDotProduct) < 0.00001f /*|| _frontToMarkerOneDistance < _breakingPointDistance*//*25f*/ || _frontMarkerOneDotProduct < 0)
            {
                _OnMoveRotation = false;
            }

            else if (_leftMarkerOneDotProduct > _rightMarkerOneDotProduct)
            {
                // для поворота налево передаем отрицательное вычисленное значение ускорения
                _tankMovingSystem.SetRotateAcceleration(-CalculateOnMoveRotationScaler());
            }
            else if (_leftMarkerOneDotProduct < _rightMarkerOneDotProduct)
            {
                _tankMovingSystem.SetRotateAcceleration(CalculateOnMoveRotationScaler());
            }
        }
    }
    // основной алгоритм разворота на месте - главная система поворотов, назначает дальнейшие функции по поворотам
    void ToTargetOnStandRotation()
    {
        if (_targetObject != null && _OnStandRotation)          // дополнительная проверка на всякий случай
        {
            _tankMovingSystem.SetForwardAcceleration(0);        // ставим ускорение прямого хода на ноль

            // если куда-то движемся, то останавливаем движение танка
            if (DEBUG_CURRENT_VELOCITY > 0.1f)
            {
                _tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
                _tankMovingSystem.SetBreakPowerScaler(1);       // тормозное усилие на макисмум
            }
            // если до цели далеко, то передаем управление алгоритму поворота в движении
            else if (_frontToMarkerOneDistance > 20f && _frontMarkerOneDotProduct > 0 && (1 - _frontMarkerOneDotProduct) < 0.1f)
            {
                _tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
                _OnStandRotation = false;                       // снимаем флаг обычного разворота
                _OnMoveRotation = true;                         // поднимаем флаг необходимости доворота в движении
            }
            // если почти смотрим на объект, то останавливаем движение танка и снимаем флаг необходимости разворота на месте
            else if (_frontMarkerOneDotProduct > 0 && (1 - _frontMarkerOneDotProduct) < 0.001f)
            {
                _tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
                // TODO Необходимо написать метод определения тормозного усилия для плавного дотормаживания перед точным доворотом
                _tankMovingSystem.SetBreakPowerScaler(1);       // тормозное усилие на макисмум

                _OnStandRotation = false;                       // снимаем флаг обычного разворота

                if (DEBUG_CURRENT_VELOCITY < 0.01f)
                {
                    _OnStandPrecise = true;                     // поднимаем флаг точного доворота
                }
            }
            // после окончательной остановки начинаем вращение
            else
            {
                // проверяем разницу в скалярных произведений боковых векторов
                if (_leftMarkerOneDotProduct > _rightMarkerOneDotProduct)
                {
                    // для поворота налево передаем отрицательное вычисленное значение ускорения
                    _tankMovingSystem.SetRotateAcceleration(-CalculateOnStandRotationScaler());
                    // в случа необходимости будет добавлен высчитанный множитель тормозного усилия
                    _tankMovingSystem.SetBreakPowerScaler(CalculateOnStandRotationBreakPowerScaler());
                }
                else if (_leftMarkerOneDotProduct < _rightMarkerOneDotProduct)
                {
                    _tankMovingSystem.SetRotateAcceleration(CalculateOnStandRotationScaler());
                    _tankMovingSystem.SetBreakPowerScaler(CalculateOnStandRotationBreakPowerScaler());
                }
                else
                {
                    _tankMovingSystem.SetRotateAcceleration(0);
                }
            }
        }
    }
    // базовый алгоритм прямолинейного движения до цели
    void ToTargetDirectMoving()
    {
        if (_targetObject != null && !_OnStandRotation && !_OnStandPrecise)      // дополнительная проверка на всякий случай
        {
            _tankMovingSystem.SetForwardAcceleration(CalculateRelativeForwardScaler());
            _tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
            _tankMovingSystem.SetBreakPowerScaler(CalculateRelativeBreakPowerScaler());
        }
    }

    void ToTargetAutoMoving()
    {
        // если скрипт включен и установлен режим "Автопилот"
        if ( _Enable && _autoPilotMode == SystemMode.AutoMovement)
        {
            if (_targetObject == null)     // если текущей цели нет, то танк стоит и не двигается
            {
                OnStandParking();
            }

            else if (_ExtremCrashStop)
            {
                if (DEBUG_CURRENT_ROTATION  >= 0 && DEBUG_CURRENT_VELOCITY >= 0)
                {
                    OnStandParking();
                }
                else
                {
                    _ExtremCrashStop = false;
                }
            }

            else          // если имеется цель выполняем движение согласно комплексного алгоритма
            {
                // TODO Необходимо сделать подсистему проверки "проскока мимо цели" в случае внезапной смены цели

                // если ранее вызвали флаг необходимости доворота на точке
                if (_OnStandRotation)
                {
                    ToTargetOnStandRotation();                  // выполняем доворот на точке
                }
                // если ранее вызвали флаг необходимости точного доворота на точке
                // данный флаг может быть вызван только из функции ToTargetOnStandRotation()
                else if (_OnStandPrecise)
                {
                    ToTargetOnStandPrecision();                 // выполняем точную подстройку на точке
                }
                // если ранее вызвали флаг необходимости точного доворота в процессе движения
                // данный флаг может быть вызван только из функции ToTargetOnStandRotation() или ниже по коду
                else if (_OnMoveRotation)
                {
                    ToTargetOnMoveRotation();                   // выполняем доворот в процессе перемещения
                }
                // если цель находится в задней полусфере или требуется доворот, при этом скорость движения ниже определенного порога
                else if (_frontMarkerOneDotProduct < 0 || (DEBUG_CURRENT_VELOCITY < 0.1f && _frontMarkerOneDotProduct < 0.9f))
                {
                    _OnStandRotation = true;                    // запрашиваем доворот на точке
                }

                // если цель находится в передней полусфере и требуется доворот, при этом скорость движения выше определенного порога
                else if (_frontMarkerOneDotProduct > 0 && (1 - _frontMarkerOneDotProduct) > 0.002f && _frontToMarkerOneDistance > 10f)
                {
                    _OnMoveRotation = true;                     // запрашиваем доворот в процессе перемещения
                }

                else if (_frontMarkerOneDotProduct > 0 && (1 - _frontMarkerOneDotProduct) < 0.002f && _frontToMarkerOneDistance > 10f)
                {

                    ToTargetDirectMoving();                     // выполняем прямолинейное движение до цели
                }
                else
                {
                    //_targetMarkerOne = null;
                    OnStandParking();
                }
            }
        }
    }

    // упрощенная формула получения реливного множителя ускорения
    float CalculateRelativeForwardScaler()
    {
        if (_targetType == TargetType.CheckPoint)
        {
            return 1f;    // даём полную мощность
        }
        else
        {
            // если расстояние до цели больше дистанции начала торможения
            if (_frontToMarkerOneDistance > _breakingPointDistance)
            {
                return 1f;    // даём полную мощность
            }
            else
            {
                // иначе, даём
                return (_frontToMarkerOneDistance) * 0.01f;
            }
        }
    }
    // упрощенная формула получения реливного множителя силы торможения
    float CalculateRelativeBreakPowerScaler()
    {
        if (_targetType == TargetType.CheckPoint)
        {
            return 0f;    
        }
        else
        {
            if (_frontToMarkerOneDistance > _breakingPointDistance)
            {
                return 0;
            }
            else
            {
                return Mathf.Abs(1 - ((_frontToMarkerOneDistance) * DEBUG_FORWARD_ACCELERATION * 2));
            }
        }
    }


    float CalculateOnStandRotationScaler()
    {
        if (_frontMarkerOneDotProduct < 0)
        {
            return 1;                          // если цель в обратной полусфере то крутимся на полную катушку
        }
        else if(_frontMarkerOneDotProduct < 1)
        {
            return 1 - _frontMarkerOneDotProduct;       // если цель в передней полусфере, ускорение уменьшает по мере приближения направлению
        }
        else
        {
            return 0;                          // если смотрим на объект, то никуда не крутимся
        }
    }
    float CalculateOnStandRotationBreakPowerScaler()
    {
        // если направление почти на цель или еще крутиться, то тормоза на нуле
        if (_frontMarkerOneDotProduct >= 0.9f || _frontMarkerOneDotProduct < 0f)
        {
            return 0;
        }
        else
        {
            if (_frontMarkerOneDotProduct < -0.9f) 
            { 
            }

            if (DEBUG_CURRENT_ROTATION < 0.25f)
            {
                return 0;
            }
            else
            {
                return DEBUG_CURRENT_ROTATION;
            }
        }
    }

    float CalculateOnMoveRotationScaler()
    {
        if (_frontMarkerOneDotProduct < 1)
        {
            return Mathf.Min((1 - _frontMarkerOneDotProduct) * _frontToMarkerOneDistance * 2 * Mathf.Abs(_leftMarkerOneDotProduct + _rightMarkerOneDotProduct), 1);       // если цель в передней полусфере, ускорение уменьшает по мере приближения направлению
        }
        else
        {
            return 0;                          // если смотрим на объект, то никуда не крутимся
        }
    }

    // функция стоянки на месте
    void OnStandParking()
    {
        _tankMovingSystem.SetForwardAcceleration(0);    // ускорение прямого хода на нуле
        _tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
        _tankMovingSystem.SetBreakPowerScaler(1);       // тормоза на макисмум
    }

    // функция определения скорости в процентах от максимально заданного лимита
    float GetSpeedLimmitFromPrecent(float precent)
    {
        return (_velocityLimit * precent) / 100;
    }

    // функция определения угловой скорости в процентах от максимально заданного лимита
    float GetSpeedRotationFromPrecent(float precent)
    {
        return (_rotationLimit * precent) / 100;
    }

    // базовая функция автопилотирования, определяет какой встроенный класс пустить в работу
    void AutoPilotMovement()
    {
        switch (_autoPilotMode)
        {
            case SystemMode.OnStandRotaion:
                _ModuleOnStandRotation.MainMovmentFuction();
                break;

            case SystemMode.AutoMovement:

                // в зависимости от дистанции до цели определяется флаг LR/SR
                if (_centerToMarkerOneDistance > 20f)
                {
                    _Module_LR_AutoMovement.MainMovmentFuction();               // передаем управление на дальней дистанции
                }
                else
                {
                    _Module_SR_AutoMovement.MainMovmentFuction();               // передаём управление на ближней дистанции
                }
                
                break;
        }
    }

    // класс для режима стоянки и вращения на месте
    public class OnStandRotation
    {
        // данный алгоритм поддерживает вращение на заданной точке, вынесено в отдельный класс с сылкой на основной автопилот
        // TODO реализовать возвращение на место, так как во время вращения танк всё же не прямо на пятачке крутится

        TankAutoPilotSystem _autoPilotSystem;                                            // ссылка на автопилот
        private bool _OnStandRotation = false;
        private bool _OnStandPrecise = false;
        private bool _OnStandParking = false;
        public OnStandRotation(TankAutoPilotSystem system)
        {
            _autoPilotSystem = system;
        }

        private float CalculateRotationScaler()
        {
            if (_autoPilotSystem._frontMarkerOneDotProduct < 0)
            {
                return 1;                          // если цель в обратной полусфере то крутимся на полную катушку
            }
            else if (_autoPilotSystem._frontMarkerOneDotProduct < 1)
            {
                // если цель в передней полусфере, ускорение уменьшает по мере приближения направлению
                return 1 - _autoPilotSystem._frontMarkerOneDotProduct;       
            }
            else
            {
                return 0;                          // если смотрим на объект, то никуда не крутимся
            }
        }
        private float CalculateBreakPowerScaler()
        {
            // если направление почти на цель или еще крутиться, то тормоза на нуле
            if (_autoPilotSystem._frontMarkerOneDotProduct >= 0.9f || _autoPilotSystem._frontMarkerOneDotProduct < 0f)
            {
                return 0;
            }
            else
            {
                if (_autoPilotSystem.DEBUG_CURRENT_ROTATION < 0.25f)
                {
                    return 0;
                }
                else
                {
                    return _autoPilotSystem.DEBUG_CURRENT_ROTATION;
                }
            }
        }

        // функция стоянки на месте
        private void ToTargetOnStandParking()
        {
            _autoPilotSystem._tankMovingSystem.SetForwardAcceleration(0);    // ускорение прямого хода на нуле
            _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
            _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(1);       // тормоза на макисмум
        }

        // функция остановки на месте
        private void ToTargetOnStandBreaking()
        {
            // если скорость больше 0.1% от максимальной
            if (_autoPilotSystem.DEBUG_CURRENT_VELOCITY > _autoPilotSystem.GetSpeedLimmitFromPrecent(0.1f))
            {
                _autoPilotSystem._tankMovingSystem.SetForwardAcceleration(0);    // ускорение прямого хода на нуле
                _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
                _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(1);       // тормоза на макисмум
            }
            else
            {
                _OnStandParking = false;
            }
        }

        // алгоритм точного доворота на месте
        private void ToTargetOnStandPrecision()
        {
            _autoPilotSystem._tankMovingSystem.SetForwardAcceleration(0);        // ставим ускорение прямого хода на ноль
            _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(0);           // ставим множитель тормозного усилия в ноль

            // если почти смотрим на объект, то останавливаем движение танка и снимаем флаг необходимости разворота на месте
            if ((1 - _autoPilotSystem._frontMarkerOneDotProduct) < 0.00001f || (1 - _autoPilotSystem._frontMarkerOneDotProduct) > 0.001f)
            {
                _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
                _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(1);       // тормозное усилие на макисмум
                _OnStandPrecise = false;
            }
            // иначе мелкими импульсами довершаем вращение
            else
            {
                // проверяем разницу в скалярных произведений боковых векторов
                if (_autoPilotSystem._leftMarkerOneDotProduct > _autoPilotSystem._rightMarkerOneDotProduct)
                {
                    // для поворота налево передаем отрицательное вычисленное значение ускорения
                    _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(-CalculateRotationScaler() * 500);
                }
                else if (_autoPilotSystem._leftMarkerOneDotProduct < _autoPilotSystem._rightMarkerOneDotProduct)
                {
                    _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(CalculateRotationScaler() * 500);
                }
                else
                {
                    _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
                    _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(1);       // тормозное усилие на макисмум
                }
            }
        }

        // алгоритм разворота на месте
        private void ToTargetOnStandRotation()
        {
            _autoPilotSystem._tankMovingSystem.SetForwardAcceleration(0);        // ставим ускорение прямого хода на ноль

            // если почти смотрим на объект, то останавливаем движение танка и снимаем флаг необходимости разворота на месте
            if (_autoPilotSystem._frontMarkerOneDotProduct > 0 && (1 - _autoPilotSystem._frontMarkerOneDotProduct) < 0.001f)
            {
                _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
                
                // TODO Необходимо написать метод определения тормозного усилия для плавного дотормаживания перед точным доворотом
                _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(1);       // тормозное усилие на макисмум

                _OnStandRotation = false;                       // снимаем флаг обычного разворота

                // если текущая скорость движения танка меньше 5% от максимальной
                if (_autoPilotSystem.DEBUG_CURRENT_VELOCITY < _autoPilotSystem.GetSpeedLimmitFromPrecent(5))
                {
                    _OnStandPrecise = true;                     // поднимаем флаг точного доворота
                }
            }
            // после окончательной остановки начинаем вращение
            else
            {
                // проверяем разницу в скалярных произведений боковых векторов
                if (_autoPilotSystem._leftMarkerOneDotProduct > _autoPilotSystem._rightMarkerOneDotProduct)
                {
                    // для поворота налево передаем отрицательное вычисленное значение ускорения
                    _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(-CalculateRotationScaler());
                    // в случа необходимости будет добавлен высчитанный множитель тормозного усилия
                    _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(CalculateBreakPowerScaler());
                }
                else if (_autoPilotSystem._leftMarkerOneDotProduct < _autoPilotSystem._rightMarkerOneDotProduct)
                {
                    _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(CalculateRotationScaler());
                    _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(CalculateBreakPowerScaler());
                }
                else
                {
                    _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(0);
                }
            }
        }

        // базовая функция вращения
        public void MainMovmentFuction()
        {

            if (_autoPilotSystem._targetObject == null)     // если текущей цели нет, то танк стоит и не двигается
            {
                ToTargetOnStandParking();
            }

            else          // если имеется цель выполняем движение согласно комплексного алгоритма
            {
                if (_OnStandParking)
                {
                    ToTargetOnStandBreaking();    // если вызван флаг остановки - останавливаемся
                }
                // если ранее вызвали флаг необходимости точного доворота на точке
                // данный флаг может быть вызван только из функции ToTargetOnStandRotation()
                else if (_OnStandPrecise)
                {
                    ToTargetOnStandPrecision();                 // выполняем точную подстройку на точке
                }
                // если ранее вызвали флаг необходимости доворота на точке
                else if (_OnStandRotation)
                {
                    ToTargetOnStandRotation();                  // выполняем доворот на точке
                }
                // если текущая скорость движения танка больше 5% от максимальной
                else if (_autoPilotSystem.DEBUG_CURRENT_VELOCITY > _autoPilotSystem.GetSpeedLimmitFromPrecent(5))
                {
                    _OnStandParking = true;      // запрашиваем остановку
                }
                // если текущий разворот меньше чем 0,998f
                else if ((1 - _autoPilotSystem._frontMarkerOneDotProduct) > 0.001f)
                {
                    _OnStandRotation = true;     // запрашиваем разворот
                }
                // если текущий разворот больше чем 0,998f
                else if ((1 - _autoPilotSystem._frontMarkerOneDotProduct) < 0.001f)
                {
                    _OnStandPrecise = true;      // запрашиваем точный доворот 
                }
                // иначе просто паркуемся
                else
                {
                    ToTargetOnStandParking();
                }
            }
        }
    }

    // класс для режима автоматического достижения цели
    public class SR_AutoMovement
    {
        TankAutoPilotSystem _autoPilotSystem;                                            // ссылка на автопилот

        public SR_AutoMovement(TankAutoPilotSystem system)
        {
            _autoPilotSystem = system;
        }

        public enum MovementType
        {
            // типы движения которые необходимо совершить танку, чтобы достичь цели
            // OnStandRotation - вращение на месте
            // OnStandPrecise - точное позиционирование на месте
            // OnMoveRotation - позиционирование во время движения
            // OnMoveTurning - большой поворот во время движения
            // OnMovePrecise - точный доворот в движении
            // OnMoveBreaking - торможение в движении
            // OnStangParking - стоянка или остановка
            SR_OnStandRotation, SR_OnStandPrecise, SR_OnMoveRotation, SR_OnMoveTurning, SR_OnMovePrecise, SR_OnMoveBreaking, OnTargetParking
        }

        [SerializeField]
        public MovementType _movementType = MovementType.OnTargetParking;
        public MovementType GetCurrentMovementType() { return _movementType; }

        


        private void OnTargetParking()
        {
            _autoPilotSystem._tankMovingSystem.SetForwardAcceleration(0);    // ускорение прямого хода на нуле
            _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
            _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(1);       // тормоза на макисмум
        }
        private void OnMoveBreaking()
        {

        }

        private float OnMoveTurninForwardScaler()
        {

            if (_autoPilotSystem._targetType == TargetType.CheckPoint)
            {
                if (_autoPilotSystem._frontMarkerOneDotProduct < 0)
                {
                    return 0;                          // если цель в обратной полусфере то вперед точно не надо
                }
                else
                {
                    return 1;
                    // если цель в передней полусфере, то лишь слегка поддаем если есть дистанция
                    return Mathf.Min((_autoPilotSystem._frontToMarkerOneDistance) * 0.1f, 1);
                }
            }
            else
            {
                return Mathf.Min((_autoPilotSystem._frontToMarkerOneDistance) * 0.1f, 1);
            }
        }
        private float OnMoveTurninRotationScaler()
        {
            return 1;
        }
        private float OnMoveTurninBreakPowerScaler()
        {
            // если направление почти на цель или еще крутиться, то тормоза на нуле
            if (_autoPilotSystem._frontMarkerOneDotProduct >= 0.9f || _autoPilotSystem._frontMarkerOneDotProduct < 0f)
            {
                return 0;
            }
            else
            {
                if (_autoPilotSystem.DEBUG_CURRENT_ROTATION < _autoPilotSystem.GetSpeedRotationFromPrecent(60))
                {
                    return 0;
                }
                else
                {
                    return (1 - _autoPilotSystem.DEBUG_CURRENT_ROTATION) / 2;
                }
            }
        }
        private void OnMoveTurning()
        {
            // так как нам всё таки надо доехать, то выставляем множитель прямого хода
            _autoPilotSystem._tankMovingSystem.SetForwardAcceleration(OnMoveTurninForwardScaler());

            // проверяем разницу в скалярных произведений боковых векторов
            if (_autoPilotSystem._leftMarkerOneDotProduct > _autoPilotSystem._rightMarkerOneDotProduct)
            {
                // для поворота налево передаем отрицательное вычисленное значение ускорения
                _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(-OnMoveTurninRotationScaler());
                // в случа необходимости будет добавлен высчитанный множитель тормозного усилия
                _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(OnMoveTurninBreakPowerScaler());
            }
            else if (_autoPilotSystem._leftMarkerOneDotProduct < _autoPilotSystem._rightMarkerOneDotProduct)
            {
                _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(OnMoveTurninRotationScaler());
                _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(OnMoveTurninBreakPowerScaler());
            }
        }

        private void OnMovePrecise()
        {

        }


        private float OnMoveRotationForwardScaler()
        {

            if (_autoPilotSystem._targetType == TargetType.CheckPoint)
            {
                if (_autoPilotSystem._frontMarkerOneDotProduct < 0)
                {
                    return 0;                          // если цель в обратной полусфере то вперед точно не надо
                }
                else
                {
                    // если цель в передней полусфере, то лишь слегка поддаем если есть дистанция
                    return _autoPilotSystem._frontToMarkerOneDistance;
                }
            }
            else
            {
                return Mathf.Min((_autoPilotSystem._frontToMarkerOneDistance / 2) * 0.1f, 1);
            }
        }
        private float OnMoveRotationRotationScaler()
        {
            if (_autoPilotSystem._frontMarkerOneDotProduct <= 0.9f)
            {
                return Mathf.Min((1 - _autoPilotSystem._frontMarkerOneDotProduct) * 2, 1);
            }

            return 0;
        }
        private float OnMoveRotationBreakPowerScaler()
        {
            if (_autoPilotSystem._targetType == TargetType.CheckPoint)
            {
                return 0;      // если цель проходная то вообще не тормозим
            }
            else
            {
                return (_autoPilotSystem._frontToMarkerOneDistance * 0.1f) * _autoPilotSystem.DEBUG_CURRENT_ROTATION * 0.1f;
            }
        }
        private void OnMoveRotation()
        {
            // так как нам всё таки надо доехать, то выставляем множитель прямого хода
            _autoPilotSystem._tankMovingSystem.SetForwardAcceleration(OnMoveRotationForwardScaler());

            // проверяем разницу в скалярных произведений боковых векторов
            if (_autoPilotSystem._leftMarkerOneDotProduct > _autoPilotSystem._rightMarkerOneDotProduct)
            {
                // для поворота налево передаем отрицательное вычисленное значение ускорения
                _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(-OnMoveRotationRotationScaler());
                // в случа необходимости будет добавлен высчитанный множитель тормозного усилия
                _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(OnMoveRotationBreakPowerScaler());
            }
            else if (_autoPilotSystem._leftMarkerOneDotProduct < _autoPilotSystem._rightMarkerOneDotProduct)
            {
                _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(OnMoveRotationRotationScaler());
                _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(OnMoveRotationBreakPowerScaler());
            }
        }

        private void OnStandPrecise()
        {

        }


        private float OnStandRotationForwardScaler()
        {

            if (_autoPilotSystem._targetType == TargetType.CheckPoint)
            {
                if (_autoPilotSystem._frontMarkerOneDotProduct < 0)
                {
                    return 0;                          // если цель в обратной полусфере то вперед точно не надо
                }
                else
                {
                    // если цель в передней полусфере, то лишь слегка поддаем если есть дистанция
                    return Mathf.Min((_autoPilotSystem._frontToMarkerOneDistance / 2) * 0.1f, 1);
                }
            }
            else
            {
                // если цель конечная то, "доезд" совершит другая функция
                return 0;
            }
        }
        private float OnStandRotationRotationScaler()
        {
            if (_autoPilotSystem._frontMarkerOneDotProduct < 0)
            {
                return 1;                          // если цель в обратной полусфере то крутимся на полную катушку
            }
            else
            {
                // если цель в передней полусфере, ускорение уменьшается по мере приближения направлению
                return 1 - _autoPilotSystem._frontMarkerOneDotProduct;
            }
        }
        private float OnStandRotationBreakPowerScaler()
        {
            // если направление почти на цель или еще крутиться, то тормоза на нуле
            if (_autoPilotSystem._frontMarkerOneDotProduct >= 0.9f || _autoPilotSystem._frontMarkerOneDotProduct < 0f)
            {
                return 0;
            }
            else
            {
                if (_autoPilotSystem.DEBUG_CURRENT_ROTATION < 0.25f)
                {
                    return 0;
                }
                else
                {
                    return _autoPilotSystem.DEBUG_CURRENT_ROTATION;
                }
            }
        }
        private void OnStandRotation()
        {
            // так как нам всё таки надо доехать, то выставляем множитель прямого хода
            _autoPilotSystem._tankMovingSystem.SetForwardAcceleration(OnStandRotationForwardScaler());

            // проверяем разницу в скалярных произведений боковых векторов
            if (_autoPilotSystem._leftMarkerOneDotProduct > _autoPilotSystem._rightMarkerOneDotProduct)
            {
                // для поворота налево передаем отрицательное вычисленное значение ускорения
                _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(-OnStandRotationRotationScaler());
                // в случа необходимости будет добавлен высчитанный множитель тормозного усилия
                _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(OnStandRotationBreakPowerScaler());
            }
            else if (_autoPilotSystem._leftMarkerOneDotProduct < _autoPilotSystem._rightMarkerOneDotProduct)
            {
                _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(OnStandRotationRotationScaler());
                _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(OnStandRotationBreakPowerScaler());
            }
        }


        // управление движением на ближней дистанции
        private void UpdateMovementType()
        {
            // если короткая дистанция и точка проходная
            if (_autoPilotSystem._targetType == TargetType.CheckPoint)
            {
                // если цель находится в задней полусфере и скорость ниже 15% от максимальной
                if (_autoPilotSystem._frontMarkerOneDotProduct < 0 && _autoPilotSystem.DEBUG_CURRENT_VELOCITY <= _autoPilotSystem.GetSpeedLimmitFromPrecent(15))
                {
                    _movementType = MovementType.SR_OnStandRotation;          // запрашиваем разворот на месте
                }
            }
            // если короткая дистанция и точка конечная
            else
            {
                // если цель находится в задней полусфере
                if (_autoPilotSystem._frontMarkerOneDotProduct < 0)
                {

                }
                else if (_autoPilotSystem._frontMarkerOneDotProduct > 0 && _autoPilotSystem._frontMarkerOneDotProduct <= 0.7f)
                {

                }
                else if (_autoPilotSystem._frontMarkerOneDotProduct > 0.7f && _autoPilotSystem._frontMarkerOneDotProduct <= 0.9f)
                {

                }
                else if (_autoPilotSystem._frontMarkerOneDotProduct > 0.9f && _autoPilotSystem._frontMarkerOneDotProduct <= 0.99f)
                {

                }
                else if (_autoPilotSystem._frontMarkerOneDotProduct > 0.99f && _autoPilotSystem._frontMarkerOneDotProduct <= 1f)
                {

                }
                else
                {

                }

                // если цель находится в задней полусфере и скорость ниже 10% от максимальной
                if (_autoPilotSystem._frontMarkerOneDotProduct < 0 && 
                    _autoPilotSystem.DEBUG_CURRENT_VELOCITY <= _autoPilotSystem.GetSpeedLimmitFromPrecent(10))
                {
                    
                    _movementType = MovementType.SR_OnStandRotation;          // запрашиваем разворот на месте
                }
                // если цель находится в задней полусфере и скорость ниже 50% от максимальной
                else if (_autoPilotSystem._frontMarkerOneDotProduct < 0 && 
                    _autoPilotSystem.DEBUG_CURRENT_VELOCITY <= _autoPilotSystem.GetSpeedLimmitFromPrecent(50))
                {
                    _movementType = MovementType.SR_OnMoveTurning;            // запрашиваем резкий разворот в движении
                }
                // если цель находится в задней полусфере и скорость выше 50% от максимальной
                else if (_autoPilotSystem._frontMarkerOneDotProduct < 0 && 
                    _autoPilotSystem.DEBUG_CURRENT_VELOCITY > _autoPilotSystem.GetSpeedLimmitFromPrecent(50))
                {
                    _movementType = MovementType.SR_OnMoveBreaking;           // запрашиваем торможение
                }
                // если цель уже находится в передней полусфере, но под достаточно острым углом и скорость выше 70% от максимальной
                else if (_autoPilotSystem._frontMarkerOneDotProduct <= 0.7f && 
                    _autoPilotSystem.DEBUG_CURRENT_VELOCITY > _autoPilotSystem.GetSpeedLimmitFromPrecent(70))
                {
                    _movementType = MovementType.SR_OnMoveBreaking;           // запрашиваем торможение
                }
                // если цель уже находится в передней полусфере, но под достаточно острым углом и скорость ниже 50% от максимальной
                else if (_autoPilotSystem._frontMarkerOneDotProduct <= 0.7f && 
                    _autoPilotSystem.DEBUG_CURRENT_VELOCITY <= _autoPilotSystem.GetSpeedLimmitFromPrecent(50))
                {
                    _movementType = MovementType.SR_OnMoveTurning;            // запрашиваем резкий разворот в движении
                }
                // если цель уже находится в передней полусфере, траектория почти куда надо и скорость не более 50% от максимальной
                else if (_autoPilotSystem._frontMarkerOneDotProduct >= 0.9f && 
                    _autoPilotSystem.DEBUG_CURRENT_VELOCITY <= _autoPilotSystem.GetSpeedLimmitFromPrecent(50))
                {
                    _movementType = MovementType.SR_OnMoveRotation;           // запрашиваем доворот в движении
                }
                // если цель уже находится в передней полусфере, траектория близка к идеальной и скорость не более 25% от максимальной
                else if (_autoPilotSystem._frontMarkerOneDotProduct <= 0.99f && 
                    _autoPilotSystem.DEBUG_CURRENT_VELOCITY <= _autoPilotSystem.GetSpeedLimmitFromPrecent(25))
                {
                    _movementType = MovementType.SR_OnMovePrecise;            // запрашиваем точный доворот в движении
                }
                // если цель уже находится в передней полусфере, траектория близка к идеальной и скорость не более 25% от максимальной
                else if (_autoPilotSystem._frontMarkerOneDotProduct >= 0.99f && 
                    _autoPilotSystem.DEBUG_CURRENT_VELOCITY <= _autoPilotSystem.GetSpeedLimmitFromPrecent(10))
                {
                    _movementType = MovementType.SR_OnStandPrecise;           // запрашиваем точный доворот на месте
                }
            }
        }

        // базовая функция пилотирования, определяет флаг стоянки и передаёт управление соответствующей подсистеме по удалению от цели
        public void MainMovmentFuction()
        {
            // если цели нет, или уже стоим на точке цели
            if (_autoPilotSystem._currentTarget == null || _autoPilotSystem._targetMarkerOne.transform.position == _autoPilotSystem.transform.position)
            {
                OnTargetParking();                                 // паркуемся на точке
            }

            else
            {
                UpdateMovementType();                              // для начала уточняем какой тип движения необходим в данный момент

                switch (_movementType)                             // работаем в зависимости от требуемого типа движения
                {

                    case MovementType.SR_OnStandRotation:
                        OnStandRotation(); break;

                    case MovementType.SR_OnStandPrecise:
                        OnStandPrecise(); break;

                    case MovementType.SR_OnMoveRotation:
                        OnMoveRotation();  break;

                    case MovementType.SR_OnMoveTurning:
                        OnMoveTurning(); break;

                    case MovementType.SR_OnMoveBreaking:
                        OnMoveBreaking();  break;

                    case MovementType.OnTargetParking:
                        OnTargetParking();
                        break;
                }
            }
        }
    }

    public class LR_AutoMovement
    {
        TankAutoPilotSystem _autoPilotSystem;                                            // ссылка на автопилот

        public LR_AutoMovement(TankAutoPilotSystem system)
        {
            _autoPilotSystem = system;
        }

        public enum MovementType
        {
            // типы движения которые необходимо совершить танку, чтобы достичь цели
            // OnStandRotation - вращение на месте
            // OnStandPrecise - точное позиционирование на месте
            // OnMoveRotation - позиционирование во время движения
            // OnMoveTurning - большой поворот во время движения
            // OnMovePrecise - точный доворот в движении
            // OnMoveBreaking - торможение в движении
            // OnStangParking - стоянка или остановка
            LR_OnStandRotation, LR_OnStandPrecise, LR_OnMoveRotation, LR_OnMoveTurning, LR_OnMovePrecise, LR_OnMoveBreaking, OnStangParking
        }

        [SerializeField]
        public MovementType _movementType = MovementType.OnStangParking;
        public MovementType GetCurrentMovementType() { return _movementType; }

        // функция стоянки на точке
        private void OnTargetParking()
        {
            _autoPilotSystem._tankMovingSystem.SetForwardAcceleration(0);    // ускорение прямого хода на нуле
            _autoPilotSystem._tankMovingSystem.SetRotateAcceleration(0);     // ускорение разворота на нуле
            _autoPilotSystem._tankMovingSystem.SetBreakPowerScaler(1);       // тормоза на макисмум
        }

        // управление движением на дальней дистанции
        private void UpdateMovmentTypeFlag()
        {
            // система работает по разному в зависимости от типа текущей заданой цели
            switch (_autoPilotSystem._targetType)
            {
                // если текущая цель - промежуточная точка, то ожидаем получить следующую цель перемещения
                case TargetType.CheckPoint:
                    break;
                // если текущая цель - конечная точка, то по достижению точки необходимо остановиться
                case TargetType.FinalPoint:
                    break;
            }
        }

        // базовая функция пилотирования, определяет флаг стоянки и передаёт управление соответствующей подсистеме по удалению от цели
        public void MainMovmentFuction()
        {
            // если цели нет, или уже стоим на точке цели
            if (_autoPilotSystem._currentTarget == null || _autoPilotSystem._targetMarkerOne.transform.position == _autoPilotSystem.transform.position)
            {
                OnTargetParking();                                 // паркуемся на точке
            }

            else
            {
                UpdateMovmentTypeFlag();                           // для начала уточняем какой тип движения необходим в данный момент

                switch (_movementType)                             // работаем в зависимости от требуемого типа движения
                {
                    case MovementType.LR_OnStandRotation:
                        break;

                    case MovementType.LR_OnStandPrecise:
                        break;

                    case MovementType.LR_OnMoveRotation:
                        break;

                    case MovementType.LR_OnMoveTurning:
                        break;

                    case MovementType.LR_OnMoveBreaking:
                        break;
                }
            }
        }
    }
}