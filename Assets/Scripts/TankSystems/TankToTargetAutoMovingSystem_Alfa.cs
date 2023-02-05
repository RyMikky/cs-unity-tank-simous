using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class TankToTargetAutoMovingSystem_Alfa : MonoBehaviour
{
    [Tooltip("Включить выполнение скрипта")]
    public bool _Enable = false;

    public enum NavigationMode
    {
        PlaneMode, SpaceMode
    }

    [Tooltip("Режим навигации 2D / 3D")]
    public NavigationMode _navigationMode = NavigationMode.PlaneMode;

    [Header("Блок определения допущений в расчётах перемещений")]
    [Tooltip("Удаление до цели для начала торможения танка")]
    public float _breakingTargetRemoval = 20f;

    [Header("Блок определения геометрии танка")]
    [Tooltip("Точка для отрисовки направления куда смотрит танковое шасси")]
    public GameObject _tankFrontDirectionPoint;
    [Tooltip("Точка отправки лучей с передней кромки танка")]
    public GameObject _tankFrontEdgePoint;
    [Tooltip("Точка для отрисовки направления назад")]
    public GameObject _tankRearDirectionPoint;
    [Tooltip("Точка отправки лучей с задней кромки танка")]
    public GameObject _tankRearEdgePoint;
    [Tooltip("Точка для отрисовки направления влево")]
    public GameObject _tankLeftDirectionPoint;
    [Tooltip("Точка отправки лучей с левой кромки танка")]
    public GameObject _tankLeftEdgePoint;
    [Tooltip("Точка для отрисовки направления вправо")]
    public GameObject _tankRightDirectionPoint;
    [Tooltip("Точка отправки лучей с правой кромки танка")]
    public GameObject _tankRightEdgePoint;
    [Tooltip("Точка отправки лучей с правой кромки танка")]
    public GameObject _tankCentralPoint;

    [Header("Назначение цели перемещения танка")]
    [Tooltip("Текущая цель перемещения танка")]
    public GameObject _targetObject;

    [Header("Флаги отображения направления разворота шасси танка на плоскости")]
    [Tooltip("Включить отображение луча прямого направления танка на плоскости")]
    public bool SHOW_PLANE_FORWARD_DIRECTION = false;
    [Tooltip("Включить отображение луча обратного направления танка на плоскости")]
    public bool SHOW_PLANE_REAR_DIRECTION = false;
    [Tooltip("Включить отображение луча левого направления танка на плоскости")]
    public bool SHOW_PLANE_LEFT_DIRECTION = false;
    [Tooltip("Включить отображение луча правого направления танка на плоскости")]
    public bool SHOW_PLANE_RIGHT_DIRECTION = false;
    [Tooltip("Возвышение луча отрисовки направления танка на плоскости")]
    public float _elevationPlaneRayDirectionRendering = 0.5f;

    [Header("Флаги отображения направления разворота шасси танка в пространстве")]
    [Tooltip("Включить отображение луча прямого направления танка в пространстве")]
    public bool SHOW_GLOBAL_FORWARD_DIRECTION = false;
    [Tooltip("Включить отображение луча обратного направления танка в пространстве")]
    public bool SHOW_GLOBAL_REAR_DIRECTION = false;
    [Tooltip("Включить отображение луча левого направления танка в пространстве")]
    public bool SHOW_GLOBAL_LEFT_DIRECTION = false;
    [Tooltip("Включить отображение луча правого направления танка в пространстве")]
    public bool SHOW_GLOBAL_RIGHT_DIRECTION = false;
    [Tooltip("Возвышение луча отрисовки направления танка в пространстве")]
    public float _elevationGlobalRayDirectionRendering = 0.5f;

    [Header("Навигационные данные в проекции на двухмерную плоскость")]
    [Tooltip("Включить отображение луча до цели на плоскости")]
    public bool SHOW_DEBUG_PLANE_TO_TARGET_RAY = false;
    [Tooltip("Показатель текущей позиции танка на двухмерной плоскости")]
    public Vector3 DEBUG_CURRENT_TANK_PLANE_POSITION;
    [Tooltip("Показатель текущей позиции цели на двухмерной плоскости")]
    public Vector3 DEBUG_CURRENT_TARGET_PLANE_POSITION;
    [Tooltip("Отображает текущую дистанцию до цели от переднего борта на плоскости")]
    public float DEBUG_FORWARD_TO_TARGET_PLANE_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до цели от заднего борта на плоскости")]
    public float DEBUG_REAR_TO_TARGET_PLANE_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до цели от левого борта на плоскости")]
    public float DEBUG_LEFT_TO_TARGET_PLANE_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до цели от правого борта на плоскости")]
    public float DEBUG_RIGHT_TO_TARGET_PLANE_DISTANCE;
    [Tooltip("Скалярное произведение направления к цели и переднего борта на плоскости")]
    public float DEBUG_PLANE_FRONT_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к цели и заднего борта на плоскости")]
    public float DEBUG_PLANE_REAR_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к цели и левого борта на плоскости")]
    public float DEBUG_PLANE_LEFT_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к цели и правого борта на плоскости")]
    public float DEBUG_PLANE_RIGHT_DOT_PRODUCT;


    [Header("Навигационные данные объектов в пространстве")]
    [Tooltip("Включить отображение луча до цели в пространстве")]
    public bool SHOW_DEBUG_GLOBAL_TO_TARGET_RAY = false;
    [Tooltip("Показатель текущей позиции танка в пространстве")]
    public Vector3 DEBUG_CURRENT_TANK_GLOBAL_POSITION;
    [Tooltip("Показатель текущей позиции цели в пространстве")]
    public Vector3 DEBUG_CURRENT_TARGET_GLOBAL_POSITION;
    [Tooltip("Отображает текущую глобальную дистанцию до цели от переднего борта")]
    public float DEBUG_FORWARD_TO_TARGET_GLOBAL_DISTANCE;
    [Tooltip("Отображает текущую глобальную дистанцию до цели от заднего борта")]
    public float DEBUG_REAR_TO_TARGET_GLOBAL_DISTANCE;
    [Tooltip("Отображает текущую глобальную дистанцию до цели от левого борта")]
    public float DEBUG_LEFT_TO_TARGET_GLOBAL_DISTANCE;
    [Tooltip("Отображает текущую глобальную дистанцию до цели от правого борта")]
    public float DEBUG_RIGHT_TO_TARGET_GLOBAL_DISTANCE;
    [Tooltip("Скалярное произведение направления к цели и переднего борта в пространстве")]
    public float DEBUG_GLOBAL_FRONT_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к цели и заднего борта в пространстве")]
    public float DEBUG_GLOBAL_REAR_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к цели и левого борта в пространстве")]
    public float DEBUG_GLOBAL_LEFT_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к цели и правого борта в пространстве")]
    public float DEBUG_GLOBAL_RIGHT_DOT_PRODUCT;


    [Header("Отладочная информация из подсистемы движения")]
    [Tooltip("Включить загрузку данных из скрипта перемещения")]
    public bool GET_DEBUG_MOVING_SYSTEM_DATA = false;

    [Tooltip("Показатель множителя ускорения в прямом направлении")]
    public float DEBUG_CURRENT_FORWARD_ACCELERATION;
    [Tooltip("Показатель множителя ускорения поворота")]
    public float DEBUG_CURRENT_ROTATION_ACCELERATION;

    [Tooltip("Показатель текущей скорости")]
    public float DEBUG_CURRENT_VELOCITY;
    [Tooltip("Показатель текущей скорости разворота по оси Y")]
    public float DEBUG_CURRENT_AXIS_Y_VELOCITY;

    // ------------------------------------- основные внутренние поля класса ----------------------------------------------------------

    private TankTruckMovingSystem _movingSystem;                 // подсистема движения танка

    // ------------------------------------- навигационные данные для работы в плоскости ----------------------------------------------

    private Vector3 _tankForwardPlaneDirection;                  // текущее направление куда смотрит шасси танка на плоскости (c Axis Y = 0);
    private Vector3 _tankRearPlaneDirection;                     // текущее обратное направление на плоскости
    private Vector3 _tankLeftPlaneDirection;                     // текущее левое направление на плоскости
    private Vector3 _tankRightPlaneDirection;                    // текущее правое направление на плоскости

    private Vector3 _forwardToTargetPlaneDirection;              // текущее направление от передней грани танка до цели на плоскости
    private Vector3 _rearToTargetPlaneDirection;                 // текущее направление от задней грани танка до цели на плоскости
    private Vector3 _leftToTargetPlaneDirection;                 // текущее направление от левой грани танка до цели на плоскости
    private Vector3 _rightToTargetPlaneDirection;                // текущее направление от правой грани танка до цели на плоскости

    private float _forwardToTargetPlaneDistance;                 // дистанция до цели от передней кромки в двухмерной плоскости
    private float _rearToTargetPlaneDistance;                    // дистанция до цели от задней кромки в двухмерной плоскости
    private float _leftToTargetPlaneDistance;                    // дистанция до цели от левой кромки в двухмерной плоскости
    private float _rightToTargetPlaneDistance;                   // дистанция до цели от правой кромки в двухмерной плоскости

    private float _currentPlaneForwardDotProduct;                // текущее скалярное произведение векторов направления шасси и цели на плоскости
    private float _currentPlaneRearDotProduct;                   // текущее скалярное произведение обратного вектора направления и цели на плоскости
    private float _currentPlaneLeftDotProduct;                   // текущее скалярное произведение левого вектора направления и цели на плоскости
    private float _currentPlaneRightDotProduct;                  // текущее скалярное произведение правого вектора направления и цели на плоскости

    private Vector3 _frontDirectionPointPlanePosition;           // проекция передней опорной точки танка на двухмерную плоскость
    private Vector3 _frontEdgePointPlanePosition;                // проекция точки передней границы танка на двухмерную плоскость
    private Vector3 _rearDirectionPointPlanePosition;            // проекция задней опорной точки танка на двухмерную плоскость
    private Vector3 _rearEdgePointPlanePosition;                 // проекция точки задней границы танка на двухмерную плоскость
    private Vector3 _leftDirectionPointPlanePosition;            // проекция левой опорной точки танка на двухмерную плоскость
    private Vector3 _leftEdgePointPlanePosition;                 // проекция точки левой границы танка на двухмерную плоскость
    private Vector3 _rightDirectionPointPlanePosition;           // проекция правой опорной точки танка на двухмерную плоскость
    private Vector3 _rightEdgePointPlanePosition;                // проекция точки правой границы танка на двухмерную плоскость
    private Vector3 _centralPointPlanePosition;                  // проекция центрально точки танка на двухмерную плоскость
    private Vector3 _targetPlanePosition;                        // проекция позиции цели на двухмерную плоскость

    // ------------------------------------- навигационные данные для работы в пространстве -------------------------------------------

    private Vector3 _tankForwardGlobalDirection;                 // текущее направление куда смотрит шасси танка в пространстве
    private Vector3 _tankRearGlobalDirection;                    // текущее обратное направление в пространстве
    private Vector3 _tankLeftGlobalDirection;                    // текущее левое направление в пространстве
    private Vector3 _tankRightGlobalDirection;                   // текущее правое направление в пространстве

    private Vector3 _forwardToTargetGlobalDirection;             // текущее направление от передней грани танка до цели в пространстве
    private Vector3 _rearToTargetGlobalDirection;                // текущее направление от задней грани танка до цели в пространстве
    private Vector3 _leftToTargetGlobalDirection;                // текущее направление от левой грани танка до цели в пространстве
    private Vector3 _rightToTargetGlobalDirection;               // текущее направление от правой грани танка до цели в пространстве

    private float _forwardToTargetGlobalDistance;                // дистанция до цели от передней кромки в пространстве
    private float _rearToTargetGlobalDistance;                   // дистанция до цели от задней кромки в пространстве
    private float _leftToTargetGlobalDistance;                   // дистанция до цели от левой кромки в пространстве
    private float _rightToTargetGlobalDistance;                  // дистанция до цели от правой кромки в пространстве

    private float _currentGlobalForwardDotProduct;               // текущее скалярное произведение векторов направления шасси и цели в пространстве
    private float _currentGlobalRearDotProduct;                  // текущее скалярное произведение обратного вектора направления и цели в пространстве
    private float _currentGlobalLeftDotProduct;                  // текущее векторное произведение левого вектора направления и цели в пространстве
    private float _currentGlobalRightDotProduct;                 // текущее векторное произведение правого вектора направления и цели в пространстве

    private Vector3 _frontDirectionPointPosition;                // позиция передней опорной точки танка в пространстве
    private Vector3 _frontEdgePointPosition;                     // позиция точки передней границы танка в пространстве
    private Vector3 _rearDirectionPointPosition;                 // позиция задней опорной точки танка в пространстве
    private Vector3 _rearEdgePointPosition;                      // позиция точки задней границы танка в пространстве
    private Vector3 _leftDirectionPointPosition;                 // позиция левой опорной точки танка в пространстве
    private Vector3 _leftEdgePointPosition;                      // позиция точки левой границы танка в пространстве
    private Vector3 _rightDirectionPointPosition;                // позиция правой опорной точки танка в пространстве
    private Vector3 _rightEdgePointPosition;                     // позиция точки правой границы танка в пространстве
    private Vector3 _centralPointPosition;                       // позиция центральной точки танка в пространстве
    private Vector3 _targetPosition;                             // позиция цели в пространстве


    private void Awake()
    {
        // получение подсистемы движения танка
        _movingSystem = GetComponent<TankTruckMovingSystem>();
    }

    void Update()
    {
        // ------------------------- подготовка навигационных данных по позициям и направлениям ---------------------

        UpdateNavigationPoints();                    // определение навигационных координат танка и цели
        UpdateNavigationDirection();                 // определение навигационных направлений бортов танка
        UpdateTankToTargetDirection();               // определение направлений от бортов танка до цели 

        UpdateToTargetDotProduct();                  // вычисление скалярный произведений направлений

        UpdateToTargetDistance();                    // расчёт дистанции до цели
        UpdateDebugDataUpload();                     // загрузка данных из скрипта перемещения

        UpdateDebugToTargetRayRendering();           // отображение луча до цели с передней кромки танка
        UpdateTankPlaneDirectionRendering();         // рендерим дебаговые гизмо-направления на плоскости
        UpdateTankGlobalDirectionRendering();        // рендерим дебаговые гизмо-направления в пространстве

        TestMoving();
    }

    // определение навигационных координат танка и цели
    void UpdateNavigationPoints()
    {
        if (_Enable)
        {
            // ------------- работа с центральной точкой ---------------------------

            // позиция центральной точки танка в глобальной системе
            _centralPointPosition = _tankCentralPoint.transform.position;
            // проекция центральной опорной точки танка на двухмерную плоскость
            _centralPointPlanePosition = _centralPointPosition;
            _centralPointPlanePosition.y = 0;

            // ------------- работа с передними точками ----------------------------

            // позиция передней опорной точки танка в глобальной системе
            _frontDirectionPointPosition = _tankFrontDirectionPoint.transform.position;
            // проекция передней опорной точки танка на двухмерную плоскость
            _frontDirectionPointPlanePosition = _frontDirectionPointPosition;
            _frontDirectionPointPlanePosition.y = 0;

            // позиция точки передней границы танка в глобальной системе
            _frontEdgePointPosition = _tankFrontEdgePoint.transform.position;
            // проекция точки передней границы танка на двухмерную плоскость
            _frontEdgePointPlanePosition = _frontEdgePointPosition;
            _frontEdgePointPlanePosition.y = 0;

            // ------------- работа с задними точками -----------------------------

            // позиция задней опорной точки танка в глобальной системе
            _rearDirectionPointPosition = _tankRearDirectionPoint.transform.position;
            // проекция задней опорной точки танка на двухмерную плоскость
            _rearDirectionPointPlanePosition = _rearDirectionPointPosition;
            _rearDirectionPointPlanePosition.y = 0;

            // позиция точки задней границы танка в глобальной системе
            _rearEdgePointPosition = _tankRearEdgePoint.transform.position;
            // проекция точки задней границы танка на двухмерную плоскость
            _rearEdgePointPlanePosition = _rearEdgePointPosition;
            _rearEdgePointPlanePosition.y = 0;

            // ------------- работа с левыми точками ------------------------------

            // позиция левой опорной точки танка в глобальной системе
            _leftDirectionPointPosition = _tankLeftDirectionPoint.transform.position;
            // проекция левой опорной точки танка на двухмерную плоскость
            _leftDirectionPointPlanePosition = _leftDirectionPointPosition;
            _leftDirectionPointPlanePosition.y = 0;

            // позиция точки левой границы танка в глобальной системе
            _leftEdgePointPosition = _tankLeftEdgePoint.transform.position;
            // проекция точки левой границы танка на двухмерную плоскость
            _leftEdgePointPlanePosition = _leftEdgePointPosition;
            _leftEdgePointPlanePosition.y = 0;

            // ------------- работа с правыми точками ------------------------------

            // позиция правой опорной точки танка в глобальной системе
            _rightDirectionPointPosition = _tankRightDirectionPoint.transform.position;
            // проекция правой опорной точки танка на двухмерную плоскость
            _rightDirectionPointPlanePosition = _rightDirectionPointPosition;
            _rightDirectionPointPlanePosition.y = 0;

            // позиция точки правой границы танка в глобальной системе
            _rightEdgePointPosition = _tankRightEdgePoint.transform.position;
            // проекция точки правой границы танка на двухмерную плоскость
            _rightEdgePointPlanePosition = _rightEdgePointPosition;
            _rightEdgePointPlanePosition.y = 0;


            if (_targetObject != null)
            {
                // позиция цели в глобальной системе
                _targetPosition = _targetObject.transform.position;
                // проекция позиции цели на двухмерную плоскость
                _targetPlanePosition = _targetPosition;
                _targetPlanePosition.y = 0;

            }
        }
    }
    // определение навигационных направлений бортов танка
    void UpdateNavigationDirection()
    {
        // --------------------------------- работа в прямом направлении ----------------------------------------------------
        
        // определяем вектор направления поворота шасси танка - куда смотрит танковое шасси в проекции на плоскость (с Axis Y = 0)
        _tankForwardPlaneDirection = _frontDirectionPointPlanePosition - _frontEdgePointPlanePosition;
        // определяем вектор направления поворота шасси танка - куда смотрит танковое шасси в глобальной системе
        _tankForwardGlobalDirection = _frontDirectionPointPosition - _frontEdgePointPosition;


        // --------------------------------- работа в обратном направлении --------------------------------------------------

        // определяем вектор обратного направления шасси танка на плоскости
        _tankRearPlaneDirection = _rearDirectionPointPlanePosition - _rearEdgePointPlanePosition;
        // определяем вектор обратного направления шасси танка в глобальной системе
        _tankRearGlobalDirection = _rearDirectionPointPosition - _rearEdgePointPosition;


        // --------------------------------- работа в левом направлении -----------------------------------------------------

        // определяем вектор обратного направления шасси танка на плоскости
        _tankLeftPlaneDirection = _leftDirectionPointPlanePosition - _leftEdgePointPlanePosition;
        // определяем вектор обратного направления шасси танка в глобальной системе
        _tankLeftGlobalDirection = _leftDirectionPointPosition - _leftEdgePointPosition;


        // --------------------------------- работа в правом направлении ----------------------------------------------------

        // определяем вектор обратного направления шасси танка на плоскости
        _tankRightPlaneDirection = _rightDirectionPointPlanePosition - _rightEdgePointPlanePosition;
        // определяем вектор обратного направления шасси танка в глобальной системе
        _tankRightGlobalDirection = _rightDirectionPointPosition - _rightEdgePointPosition;
    }
    // определение направлений от бортов танка до цели 
    void UpdateTankToTargetDirection()
    {
        if (_targetObject != null)
        {
            // определяем вектор направления до цели от переднего борта на плоскости
            _forwardToTargetGlobalDirection = _targetPosition - _frontEdgePointPosition;
            // определяем вектор направления до цели от переднего борта в пространстве
            _forwardToTargetPlaneDirection = _targetPlanePosition - _frontEdgePointPlanePosition;

            // определяем вектор направления до цели от заднего борта на плоскости
            _rearToTargetGlobalDirection = _targetPosition - _rearEdgePointPosition;
            // определяем вектор направления до цели от заднего борта в пространстве
            _rearToTargetPlaneDirection = _targetPlanePosition - _rearEdgePointPlanePosition;

            // определяем вектор направления до цели от левого борта на плоскости
            _leftToTargetGlobalDirection = _targetPosition - _leftEdgePointPosition;
            // определяем вектор направления до цели от левого борта в пространстве
            _leftToTargetPlaneDirection = _targetPlanePosition - _leftEdgePointPlanePosition;

            // определяем вектор направления до цели от правого борта на плоскости
            _rightToTargetGlobalDirection = _targetPosition - _rightEdgePointPosition;
            // определяем вектор направления до цели от правого борта в пространстве
            _rightToTargetPlaneDirection = _targetPlanePosition - _rightEdgePointPlanePosition;
        }
        else
        {
            _forwardToTargetGlobalDirection = Vector3.zero;
            _forwardToTargetPlaneDirection = Vector3.zero;

            _rearToTargetGlobalDirection = Vector3.zero;
            _rearToTargetPlaneDirection = Vector3.zero;

            _leftToTargetGlobalDirection = Vector3.zero;
            _leftToTargetPlaneDirection = Vector3.zero;

            _rightToTargetGlobalDirection = Vector3.zero;
            _rightToTargetPlaneDirection = Vector3.zero;
        }
    }

    // рендерим дебаговые гизмо-направления на плоскости
    void UpdateTankPlaneDirectionRendering()
    {
        if (SHOW_PLANE_FORWARD_DIRECTION)
        {
            // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление на плоскости
            Debug.DrawRay(_frontEdgePointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0),
                (_frontDirectionPointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0)) -
                (_frontEdgePointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0)), Color.green);
        }

        if (SHOW_PLANE_REAR_DIRECTION)
        {
            // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление на плоскости
            Debug.DrawRay(_rearEdgePointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0),
                (_rearDirectionPointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0)) -
                (_rearEdgePointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0)), Color.green);
        }

        if (SHOW_PLANE_LEFT_DIRECTION)
        {
            // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление на плоскости
            Debug.DrawRay(_leftEdgePointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0),
                (_leftDirectionPointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0)) -
                (_leftEdgePointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0)), Color.green);
        }

        if (SHOW_PLANE_RIGHT_DIRECTION)
        {
            // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление на плоскости
            Debug.DrawRay(_rightEdgePointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0),
                (_rightDirectionPointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0)) -
                (_rightEdgePointPlanePosition + new Vector3(0, _elevationPlaneRayDirectionRendering, 0)), Color.green);
        }
    }
    // рендерим дебаговые гизмо-направления в пространстве
    void UpdateTankGlobalDirectionRendering()
    {
        if (SHOW_GLOBAL_FORWARD_DIRECTION)
        {
            // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление в глобальной системе
            Debug.DrawRay(_frontEdgePointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0),
                (_frontDirectionPointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0)) -
                (_frontEdgePointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0)), Color.green);
        }

        if (SHOW_GLOBAL_REAR_DIRECTION)
        {
            // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление в глобальной системе
            Debug.DrawRay(_rearEdgePointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0),
                (_rearDirectionPointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0)) -
                (_rearEdgePointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0)), Color.green);
        }

        if (SHOW_GLOBAL_LEFT_DIRECTION)
        {
            // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление в глобальной системе
            Debug.DrawRay(_leftEdgePointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0),
                (_leftDirectionPointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0)) -
                (_leftEdgePointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0)), Color.green);
        }

        if (SHOW_GLOBAL_RIGHT_DIRECTION)
        {
            // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление в глобальной системе
            Debug.DrawRay(_rightEdgePointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0),
                (_rightDirectionPointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0)) -
                (_rightEdgePointPosition + new Vector3(0, _elevationGlobalRayDirectionRendering, 0)), Color.green);
        }
    }
    // вычисление скалярный произведений направлений
    void UpdateToTargetDotProduct()
    {
        if (_Enable)
        {
            if (_targetObject != null)
            {
                // --------------------------------- работа в прямом направлении ----------------------------------------------------

                _currentPlaneForwardDotProduct = Vector3.Dot(_tankForwardPlaneDirection.normalized, _forwardToTargetPlaneDirection.normalized);
                _currentGlobalForwardDotProduct = Vector3.Dot(_tankForwardGlobalDirection.normalized, _forwardToTargetGlobalDirection.normalized);


                // --------------------------------- работа в обратном направлении --------------------------------------------------

                _currentPlaneRearDotProduct = Vector3.Dot(_tankRearPlaneDirection.normalized, _rearToTargetPlaneDirection.normalized);
                _currentGlobalRearDotProduct = Vector3.Dot(_tankRearGlobalDirection.normalized, _rearToTargetGlobalDirection.normalized);

                // --------------------------------- работа в левом направлении -----------------------------------------------------

                _currentPlaneLeftDotProduct = Vector3.Dot(_tankLeftPlaneDirection.normalized, _leftToTargetPlaneDirection.normalized);
                _currentGlobalLeftDotProduct = Vector3.Dot(_tankLeftGlobalDirection.normalized, _leftToTargetGlobalDirection.normalized);

                // --------------------------------- работа в правом направлении ----------------------------------------------------

                _currentPlaneRightDotProduct = Vector3.Dot(_tankRightPlaneDirection.normalized, _rightToTargetPlaneDirection.normalized);
                _currentGlobalRightDotProduct = Vector3.Dot(_tankRightGlobalDirection.normalized, _rightToTargetGlobalDirection.normalized);
            }
            else
            {
                _currentPlaneForwardDotProduct = 0; _currentGlobalForwardDotProduct = 0;
                _currentPlaneRearDotProduct = 0; _currentGlobalRearDotProduct = 0;
                _currentPlaneLeftDotProduct = 0; _currentGlobalLeftDotProduct = 0;
                _currentPlaneRightDotProduct = 0; _currentGlobalRightDotProduct = 0;
            }

            DEBUG_PLANE_FRONT_DOT_PRODUCT = _currentPlaneForwardDotProduct;
            DEBUG_GLOBAL_FRONT_DOT_PRODUCT = _currentGlobalForwardDotProduct;

            DEBUG_PLANE_REAR_DOT_PRODUCT = _currentPlaneRearDotProduct;
            DEBUG_GLOBAL_REAR_DOT_PRODUCT = _currentGlobalRearDotProduct;

            DEBUG_PLANE_LEFT_DOT_PRODUCT = _currentPlaneLeftDotProduct;
            DEBUG_GLOBAL_LEFT_DOT_PRODUCT = _currentPlaneLeftDotProduct;

            DEBUG_PLANE_RIGHT_DOT_PRODUCT = _currentPlaneRightDotProduct;
            DEBUG_GLOBAL_RIGHT_DOT_PRODUCT = _currentGlobalRightDotProduct;
        }
        
    }

    // отображение луча до цели с передней кромки танка
    void UpdateDebugToTargetRayRendering()
    {
        if (_Enable && _targetObject != null) 
        {
            if (SHOW_DEBUG_PLANE_TO_TARGET_RAY)
            {
                Debug.DrawRay(_frontEdgePointPlanePosition, _forwardToTargetPlaneDirection, Color.red);
                Debug.DrawRay(_rearEdgePointPlanePosition, _rearToTargetPlaneDirection, Color.red);
                Debug.DrawRay(_leftEdgePointPlanePosition, _leftToTargetPlaneDirection, Color.red);
                Debug.DrawRay(_rightEdgePointPlanePosition, _rightToTargetPlaneDirection, Color.red);
            }

            if (SHOW_DEBUG_GLOBAL_TO_TARGET_RAY)
            {
                Debug.DrawRay(_frontEdgePointPosition, _forwardToTargetGlobalDirection, Color.red);
                Debug.DrawRay(_rearEdgePointPosition, _rearToTargetGlobalDirection, Color.red);
                Debug.DrawRay(_leftEdgePointPosition, _leftToTargetGlobalDirection, Color.red);
                Debug.DrawRay(_rightEdgePointPosition, _rightToTargetGlobalDirection, Color.red);
            }
        }
        
    }
    // обновление дистанции до цели, если она задана
    void UpdateToTargetDistance()
    {
        if (_Enable)
        {
            // если скрипт включен и имеется заданная цель
            if (_targetObject != null)
            {
                // записываем проекционную дистанцию от передней кромки до цели на плоскости 
                _forwardToTargetPlaneDistance = Vector3.Distance(_targetPlanePosition, _frontEdgePointPlanePosition);
                // записываем глобальюную дистанцию от передней кромки до цели в пространстве
                _forwardToTargetGlobalDistance = Vector3.Distance(_targetPosition, _frontEdgePointPosition);

                // записываем проекционную дистанцию от задней кромки до цели на плоскости 
                _rearToTargetPlaneDistance = Vector3.Distance(_targetPlanePosition, _rearEdgePointPlanePosition);
                // записываем глобальюную дистанцию от задней кромки до цели в пространстве
                _rearToTargetGlobalDistance = Vector3.Distance(_targetPosition, _rearEdgePointPosition);

                // записываем проекционную дистанцию от левой кромки до цели на плоскости 
                _leftToTargetPlaneDistance = Vector3.Distance(_targetPlanePosition, _leftEdgePointPlanePosition);
                // записываем глобальюную дистанцию от левой кромки до цели в пространстве
                _leftToTargetGlobalDistance = Vector3.Distance(_targetPosition, _leftEdgePointPosition);

                // записываем проекционную дистанцию от правой кромки до цели на плоскости 
                _rightToTargetPlaneDistance = Vector3.Distance(_targetPlanePosition, _rightEdgePointPlanePosition);
                // записываем глобальюную дистанцию от правой кромки до цели в пространстве
                _rightToTargetGlobalDistance = Vector3.Distance(_targetPosition, _rightEdgePointPosition);


                // записываем данные о позиции цели на двухмерной плоскости
                DEBUG_CURRENT_TARGET_PLANE_POSITION = _targetPlanePosition;
                // записываем данные о глобальной позиции цели
                DEBUG_CURRENT_TARGET_GLOBAL_POSITION = _targetPosition;
            }
            // если цели нет
            else 
            {
                // обнуляем данные о глобальной позиции цели, глобальную и проекционную дистанции до цели
                _forwardToTargetGlobalDistance = 0;
                _forwardToTargetPlaneDistance = 0;

                _rearToTargetGlobalDistance = 0;
                _rearToTargetPlaneDistance = 0;

                _leftToTargetGlobalDistance = 0;
                _leftToTargetPlaneDistance = 0;

                _rightToTargetGlobalDistance = 0;
                _rightToTargetPlaneDistance = 0;

                DEBUG_CURRENT_TARGET_PLANE_POSITION = Vector3.zero;
                DEBUG_CURRENT_TARGET_GLOBAL_POSITION = Vector3.zero;
            }

            // записываем деббаговые поля согласно полученным данным в условии выше
            DEBUG_CURRENT_TANK_PLANE_POSITION = _frontEdgePointPlanePosition;
            DEBUG_CURRENT_TANK_GLOBAL_POSITION = _frontEdgePointPosition;

            DEBUG_FORWARD_TO_TARGET_GLOBAL_DISTANCE = _forwardToTargetGlobalDistance;
            DEBUG_FORWARD_TO_TARGET_PLANE_DISTANCE = _forwardToTargetPlaneDistance;

            DEBUG_REAR_TO_TARGET_GLOBAL_DISTANCE = _rearToTargetGlobalDistance;
            DEBUG_REAR_TO_TARGET_PLANE_DISTANCE = _rearToTargetPlaneDistance;

            DEBUG_LEFT_TO_TARGET_GLOBAL_DISTANCE = _leftToTargetGlobalDistance;
            DEBUG_LEFT_TO_TARGET_PLANE_DISTANCE = _leftToTargetPlaneDistance;

            DEBUG_RIGHT_TO_TARGET_GLOBAL_DISTANCE = _rightToTargetGlobalDistance;
            DEBUG_RIGHT_TO_TARGET_PLANE_DISTANCE = _rightToTargetPlaneDistance;
        }
        
    }
    // загрузка данных из скрипта перемещения
    void UpdateDebugDataUpload()
    {
        if (_Enable)
        {
            // если включен флаг загрузки данных и подсистема движения танка активна
            if (GET_DEBUG_MOVING_SYSTEM_DATA && _movingSystem != null)
            {
                DEBUG_CURRENT_VELOCITY = _movingSystem.GetDebugCurrentVelocity();
                DEBUG_CURRENT_AXIS_Y_VELOCITY = _movingSystem.GetDebugCurrentAxisVelocity();
                DEBUG_CURRENT_FORWARD_ACCELERATION = _movingSystem.GetDebugCurrentForwardAcceleration();
                DEBUG_CURRENT_ROTATION_ACCELERATION = _movingSystem.GetDebugCurrentRotationAcceleration();
            }
            else
            {
                DEBUG_CURRENT_VELOCITY = 0; DEBUG_CURRENT_AXIS_Y_VELOCITY = 0;
                DEBUG_CURRENT_FORWARD_ACCELERATION = 0; DEBUG_CURRENT_ROTATION_ACCELERATION = 0;
            }
        }
    }

    void ToTargetOnStandRotation()
    {
        _movingSystem.SetForwardAcceleration(0);
        _movingSystem.SetRotateAcceleration(1);
    }
    void TestMoving()
    {
        if (_currentPlaneForwardDotProduct < 0 || (1 - _currentPlaneForwardDotProduct) > 0.02f)
        {
            ToTargetOnStandRotation();
        }

        else if(_currentPlaneForwardDotProduct > 0 && (1 - _currentPlaneForwardDotProduct) < 0.02f && _forwardToTargetPlaneDistance > 1f && _movingSystem != null)
        {
            _movingSystem.SetInputType(TankTruckMovingSystem.InputType.AutoPilot);
            _movingSystem.SetForwardAcceleration(CalculateRelativeForwardScaler());
            _movingSystem.SetBreakPowerScaler(CalculateRelativeBreakPowerScaler());
        }
        else
        {
            _targetObject = null;
            _movingSystem.SetForwardAcceleration(0);
        }
    }
    // упрощенная формула получения реливного множителя ускорения
    float CalculateRelativeForwardScaler()
    {
        if (_forwardToTargetGlobalDistance > _breakingTargetRemoval)
        {
            return 1f;
        }
        else
        {
            return (_forwardToTargetGlobalDistance / 2) * 0.1f;
        }
    }
    // упрощенная формула получения реливного множителя силы торможения
    float CalculateRelativeBreakPowerScaler()
    {
        if (_forwardToTargetGlobalDistance > _breakingTargetRemoval)
        {
            return 0;
        }
        else
        {
            return 1 - ((_forwardToTargetGlobalDistance / 2) * 0.1f);
        }
    }
}