using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankNavigationGlobalSubSystem : MonoBehaviour
{
    [Tooltip("Включить выполнение скрипта")]
    public bool _Enable = false;

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
    public GameObject _tankCenterPoint;

    [Header("Назначение цели перемещения танка")]
    [Tooltip("Текущая маркер перемещения танка")]
    public GameObject _targetMarkerOne;
    [Tooltip("Следующий маркер перемещения танка")]
    public GameObject _targetMarkerTwo;

    [Header("Флаги отображения направления разворота шасси танка в пространстве")]
    [Tooltip("Включить отображение луча прямого направления танка в пространстве")]
    public bool SHOW_FRONT_DIRECTION_RAY = false;
    [Tooltip("Включить отображение луча обратного направления танка в пространстве")]
    public bool SHOW_REAR_DIRECTION_RAY = false;
    [Tooltip("Включить отображение луча левого направления танка в пространстве")]
    public bool SHOW_LEFT_DIRECTION_RAY = false;
    [Tooltip("Включить отображение луча правого направления танка в пространстве")]
    public bool SHOW_RIGHT_DIRECTION_RAY = false;
    [Tooltip("Возвышение луча отрисовки направления танка в пространстве")]
    public float DIRECTION_RAY_ELEVATION = 0.5f;

    [Header("Флаги отображения лучей до цели с разных бортов танка в пространстве")]
    [Tooltip("Включить отображение луча до цели от передней кромки")]
    public bool SHOW_FRONT_TO_TARGET_RAY = false;
    [Tooltip("Включить отображение луча до цели от задней кромки")]
    public bool SHOW_REAR_TO_TARGET_RAY = false;
    [Tooltip("Включить отображение луча до цели от левой кромки")]
    public bool SHOW_LEFT_TO_TARGET_RAY = false;
    [Tooltip("Включить отображение луча до цели от правой кромки")]
    public bool SHOW_RIGHT_TO_TARGET_RAY = false;
    [Tooltip("Возвышение луча отрисовки направления танка в пространстве")]
    public float TO_TARGET_RAY_ELEVATION = 0.5f;

    [Tooltip("Включить отображение луча от первого до второго маркера")]
    public bool SHOW_TO_SECOND_TARGET_RAY = false;

    [Header("Отладочные данные расчётов по текущим позициям в пространстве")]
    [Tooltip("Показатель текущей позиции танка в пространстве")]
    public Vector3 DEBUG_TANK_POSITION;
    [Tooltip("Показатель текущей позиции цели в пространстве")]
    public Vector3 DEBUG_TARGET_POSITION;
    [Tooltip("Отображает текущую дистанцию до цели от центральной точки в пространстве")]
    public float DEBUG_CENTRAL_TO_TARGET_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до цели от переднего борта в пространстве")]
    public float DEBUG_FRONT_TO_TARGET_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до цели от заднего борта в пространстве")]
    public float DEBUG_REAR_TO_TARGET_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до цели от левого борта в пространстве")]
    public float DEBUG_LEFT_TO_TARGET_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до цели от правого борта в пространстве")]
    public float DEBUG_RIGHT_TO_TARGET_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до первого от второго маркера в пространстве")]
    public float DEBUG_MARKER_TO_MARKER_DISTANCE;
    [Tooltip("Скалярное произведение направления к цели и переднего борта в пространстве")]
    public float DEBUG_FRONT_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к цели и заднего борта в пространстве")]
    public float DEBUG_REAR_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к цели и левого борта в пространстве")]
    public float DEBUG_LEFT_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к цели и правого борта в пространстве")]
    public float DEBUG_RIGHT_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к первому и второму маркеру от переднего борта в пространстве")]
    public float DEBUG_MARKER_DOT_PRODUCT;

    // ------------------------------------- навигационные данные для работы в пространстве -------------------------------------------

    private Vector3 _tankFrontDirection;                         // текущее направление куда смотрит шасси танка в пространстве
    private Vector3 _tankRearDirection;                          // текущее обратное направление в пространстве
    private Vector3 _tankLeftDirection;                          // текущее левое направление в пространстве
    private Vector3 _tankRightDirection;                         // текущее правое направление в пространстве

    private Vector3 _frontToTargetDirection;                     // текущее направление от передней грани танка до цели в пространстве
    private Vector3 _rearToTargetDirection;                      // текущее направление от задней грани танка до цели в пространстве
    private Vector3 _leftToTargetDirection;                      // текущее направление от левой грани танка до цели в пространстве
    private Vector3 _rightToTargetDirection;                     // текущее направление от правой грани танка до цели в пространстве

    private Vector3 _markerToMarkerDirection;                    // текущее направление от первого до второго маркера цели в пространстве

    private float _frontToTargetDistance;                        // дистанция до цели от передней кромки в пространстве
    private float _rearToTargetDistance;                         // дистанция до цели от задней кромки в пространстве
    private float _leftToTargetDistance;                         // дистанция до цели от левой кромки в пространстве
    private float _rightToTargetDistance;                        // дистанция до цели от правой кромки в пространстве
    private float _centralToTargetDistance;                      // дистанция до цели от центральной точки в пространстве
    private float _markerToMarkerDistance;                       // дистанция до цели от первого до второго маркера в пространстве

    private float _frontDotProduct;                              // текущее скалярное произведение векторов направления шасси и цели в пространстве
    private float _rearDotProduct;                               // текущее скалярное произведение обратного вектора направления и цели в пространстве
    private float _leftDotProduct;                               // текущее скалярное произведение левого вектора направления и цели в пространстве
    private float _rightDotProduct;                              // текущее скалярное произведение правого вектора направления и цели в пространстве

    private float _markerDotProduct;                             // текущее скалярное произведение направления к первому и второму маркеру от переднего борта

    private Vector3 _frontDirectionPointPosition;                // проекция передней опорной точки танка в пространстве
    private Vector3 _frontEdgePointPosition;                     // проекция точки передней границы танка в пространстве

    private Vector3 _rearDirectionPointPosition;                 // проекция задней опорной точки танка в пространстве
    private Vector3 _rearEdgePointPosition;                      // проекция точки задней границы танка в пространстве

    private Vector3 _leftDirectionPointPosition;                 // проекция левой опорной точки танка в пространстве
    private Vector3 _leftEdgePointPosition;                      // проекция точки левой границы танка в пространстве

    private Vector3 _rightDirectionPointPosition;                // проекция правой опорной точки танка в пространстве
    private Vector3 _rightEdgePointPosition;                     // проекция точки правой границы танка в пространстве

    private Vector3 _centralPointPosition;                       // проекция центрально точки танка в пространстве
    private Vector3 _targetMarkerOnePosition;                    // проекция позиции первого марера цели на двухмерную плоскость
    private Vector3 _targetMarkerTwoPosition;                    // проекция позиции второго марера цели на двухмерную плоскость

    // ------------------------------------- блок публичных сеттеров и геттеров класса ------------------------------------------------

    public void SetTargetMarkerOne(GameObject markerOne) { _targetMarkerOne = markerOne; }
    public GameObject GetTargetMarkerOne() { return _targetMarkerOne; }

    public void SetTargetMarkerTwo(GameObject markerTwo) { _targetMarkerTwo = markerTwo; }
    public GameObject GetTargetMarkerTwo() { return _targetMarkerTwo; }

    public void SetDirectionRayElevation(float elevation) { DIRECTION_RAY_ELEVATION = elevation; }
    public float GetDirectionRayElevation() { return DIRECTION_RAY_ELEVATION; }
    public void SetToTargetRayElevation(float elevation) { TO_TARGET_RAY_ELEVATION = elevation; }
    public float GetToTargetRayElevation() { return TO_TARGET_RAY_ELEVATION; }

    public float GetCentralToTargetDistance() { return _centralToTargetDistance; }
    public float GetFrontToTargetDistance() { return _frontToTargetDistance; }
    public float GetRearToTargetDistance() { return _rearToTargetDistance; }
    public float GetLeftToTargetDistance() { return _leftToTargetDistance; }
    public float GetRightToTargetDistance() { return _rightToTargetDistance; }
    public float GetMarkerToMarkerDistance() { return _markerToMarkerDistance; }

    public float GetFrontDotProduct() { return _frontDotProduct; }
    public float GetRearDotProduct() { return _rearDotProduct; }
    public float GetLeftDotProduct() { return _leftDotProduct; }
    public float GetRightDotProduct() { return _rightDotProduct; }
    public float GetMarkerDotProduct() { return _markerDotProduct; }

    // ------------------------------------- блок публичных методов задачи булевых флагов ---------------------------------------------

    public void SetSubSystemEnable(bool enable) { _Enable = enable; }
    public void SetDirectionRayEnable(bool enable) 
    {
        SHOW_FRONT_DIRECTION_RAY = enable;
        SHOW_REAR_DIRECTION_RAY = enable;
        SHOW_LEFT_DIRECTION_RAY = enable;
        SHOW_RIGHT_DIRECTION_RAY = enable;
    }
    public void SetToTargetRayEnable(bool enable)
    {
        SHOW_FRONT_TO_TARGET_RAY = enable;
        SHOW_REAR_TO_TARGET_RAY = enable;
        SHOW_LEFT_TO_TARGET_RAY = enable;
        SHOW_RIGHT_TO_TARGET_RAY = enable;

        SHOW_TO_SECOND_TARGET_RAY = enable;
    }

    void FixedUpdate()
    {
        // ------------------------- подготовка навигационных данных по позициям и направлениям ---------------------

        UpdateNavigationPoints();                    // определение навигационных координат танка и цели
        UpdateNavigationDirection();                 // определение навигационных направлений бортов танка
        UpdateTankToTargetDirection();               // определение направлений от бортов танка до цели 

        // ------------------------- блок вычисления навигационных показателей --------------------------------------

        UpdateToTargetDotProduct();                  // вычисление скалярных произведений направлений
        UpdateToTargetDistance();                    // расчёт дистанции до цели

        // ------------------------- блок рендеринга навигационных направлений --------------------------------------

        UpdateDebugToTargetRayRendering();           // отображение луча до цели с передней кромки танка
        UpdateTankDirectionRendering();              // рендерим дебаговые гизмо-направления в пространстве
    }

    // определение навигационных координат танка и цели
    void UpdateNavigationPoints()
    {
        if (_Enable)
        {
            // проекция центральной опорной точки танка в пространстве
            _centralPointPosition = _tankCenterPoint.transform.position;

            // проекция передней опорной точки танка в пространстве
            _frontDirectionPointPosition = _tankFrontDirectionPoint.transform.position;

            // проекция точки передней границы танка в пространстве
            _frontEdgePointPosition = _tankFrontEdgePoint.transform.position;

            // проекция задней опорной точки танка в пространстве
            _rearDirectionPointPosition = _tankRearDirectionPoint.transform.position;

            // проекция точки задней границы танка в пространстве
            _rearEdgePointPosition = _tankRearEdgePoint.transform.position;

            // проекция левой опорной точки танка в пространстве
            _leftDirectionPointPosition = _tankLeftDirectionPoint.transform.position;

            // проекция точки левой границы танка в пространстве
            _leftEdgePointPosition = _tankLeftEdgePoint.transform.position;

            // проекция правой опорной точки танка в пространстве
            _rightDirectionPointPosition = _tankRightDirectionPoint.transform.position;

            // проекция точки правой границы танка в пространстве
            _rightEdgePointPosition = _tankRightEdgePoint.transform.position;

            if (_targetMarkerOne != null)
            {
                // проекция позиции первого марекера цели в пространстве
                _targetMarkerOnePosition = _targetMarkerOne.transform.position;
            }

            if (_targetMarkerTwo != null)
            {
                // проекция позиции первого марекера цели в пространстве
                _targetMarkerTwoPosition = _targetMarkerTwo.transform.position;
            }
        }
    }
    // определение навигационных направлений бортов танка
    void UpdateNavigationDirection()
    {
        if (_Enable)
        {
            // определяем вектор направления поворота шасси танка фронтальной плоскости
            _tankFrontDirection = _frontDirectionPointPosition - _frontEdgePointPosition;

            // определяем вектор направления поворота шасси танка задней плоскости
            _tankRearDirection = _rearDirectionPointPosition - _rearEdgePointPosition;

            // определяем вектор направления поворота шасси танка левой плоскости
            _tankLeftDirection = _leftDirectionPointPosition - _leftEdgePointPosition;

            // определяем вектор направления поворота шасси танка правой плоскости
            _tankRightDirection = _rightDirectionPointPosition - _rightEdgePointPosition;
        }
    }
    // определение направлений от бортов танка до цели 
    void UpdateTankToTargetDirection()
    {
        if (_Enable)
        {
            if (_targetMarkerOne != null)
            {
                // определяем вектор направления до цели от переднего борта в пространстве
                _frontToTargetDirection = _targetMarkerOnePosition - _frontEdgePointPosition;

                // определяем вектор направления до цели от заднего борта в пространстве
                _rearToTargetDirection = _targetMarkerOnePosition - _rearEdgePointPosition;

                // определяем вектор направления до цели от левого борта в пространстве
                _leftToTargetDirection = _targetMarkerOnePosition - _leftEdgePointPosition;

                // определяем вектор направления до цели от правого борта в пространстве
                _rightToTargetDirection = _targetMarkerOnePosition - _rightEdgePointPosition;

                // если есть второй маркер и его позиция не совпадает с позицией первого маркера
                if (_targetMarkerTwo != null && _targetMarkerTwoPosition != _targetMarkerOnePosition)
                {
                    _markerToMarkerDirection = _targetMarkerTwoPosition - _targetMarkerOnePosition;
                }
                else
                {
                    _markerToMarkerDirection = Vector3.zero;
                }
            }
            else
            {
                _frontToTargetDirection = Vector3.zero;
                _rearToTargetDirection = Vector3.zero;
                _leftToTargetDirection = Vector3.zero;
                _rightToTargetDirection = Vector3.zero;

                _markerToMarkerDirection = Vector3.zero;
            }
        }
    }

    // вычисление скалярный произведених направлений
    void UpdateToTargetDotProduct()
    {
        if (_targetMarkerOne != null)
        {
            _frontDotProduct = Vector3.Dot(_tankFrontDirection.normalized, _frontToTargetDirection.normalized);
            _rearDotProduct = Vector3.Dot(_tankRearDirection.normalized, _rearToTargetDirection.normalized);
            _leftDotProduct = Vector3.Dot(_tankLeftDirection.normalized, _leftToTargetDirection.normalized);
            _rightDotProduct = Vector3.Dot(_tankRightDirection.normalized, _rightToTargetDirection.normalized);

            if (_targetMarkerTwo != null && _targetMarkerTwoPosition != _targetMarkerOnePosition)
            {
                _markerDotProduct = Vector3.Dot(_frontToTargetDirection.normalized, _markerToMarkerDirection.normalized);
            }
            else
            {
                _markerDotProduct = 0;
            }

        }
        else
        {
            _frontDotProduct = 0; _rearDotProduct = 0;
            _leftDotProduct = 0; _rightDotProduct = 0;
            _markerDotProduct = 0;
        }

        DEBUG_FRONT_DOT_PRODUCT = _frontDotProduct;
        DEBUG_REAR_DOT_PRODUCT = _rearDotProduct;
        DEBUG_LEFT_DOT_PRODUCT = _leftDotProduct;
        DEBUG_RIGHT_DOT_PRODUCT = _rightDotProduct;

        DEBUG_MARKER_DOT_PRODUCT = _markerDotProduct;
    }
    // обновление дистанции до цели, если она задана
    void UpdateToTargetDistance()
    {
        if (_Enable)
        {
            // если скрипт включен и имеется заданная цель
            if (_targetMarkerOne != null)
            {
                // записываем проекционную дистанцию от центральной точки до цели в пространстве
                _centralToTargetDistance = Vector3.Distance(_targetMarkerOnePosition, _centralPointPosition);

                // записываем проекционную дистанцию от передней кромки до цели в пространстве
                _frontToTargetDistance = Vector3.Distance(_targetMarkerOnePosition, _frontEdgePointPosition);

                // записываем проекционную дистанцию от задней кромки до цели в пространстве
                _rearToTargetDistance = Vector3.Distance(_targetMarkerOnePosition, _rearEdgePointPosition);

                // записываем проекционную дистанцию от левой кромки до цели в пространстве
                _leftToTargetDistance = Vector3.Distance(_targetMarkerOnePosition, _leftEdgePointPosition);

                // записываем проекционную дистанцию от правой кромки до цели в пространстве
                _rightToTargetDistance = Vector3.Distance(_targetMarkerOnePosition, _rightEdgePointPosition);

                // записываем данные о позиции цели в пространстве
                DEBUG_TARGET_POSITION = _targetMarkerOnePosition;

                if (_targetMarkerTwo != null && _targetMarkerOnePosition != _targetMarkerTwoPosition)
                {
                    _markerToMarkerDistance = Vector3.Distance(_targetMarkerTwoPosition, _targetMarkerOnePosition);
                }
                else
                {
                    _markerToMarkerDistance = 0;
                }
            }
            // если цели нет
            else
            {
                // обнуляем данные о глобальной позиции цели, глобальные дистанции до цели
                _centralToTargetDistance = 0;
                _frontToTargetDistance = 0; _rearToTargetDistance = 0;
                _leftToTargetDistance = 0; _rightToTargetDistance = 0;
                _markerToMarkerDistance = 0;

                DEBUG_TARGET_POSITION = Vector3.zero;
            }

            // записываем деббаговые поля согласно полученным данным в условии выше
            DEBUG_TANK_POSITION = _frontEdgePointPosition;
            DEBUG_CENTRAL_TO_TARGET_DISTANCE = _centralToTargetDistance;
            DEBUG_FRONT_TO_TARGET_DISTANCE = _frontToTargetDistance;
            DEBUG_REAR_TO_TARGET_DISTANCE = _rearToTargetDistance;
            DEBUG_LEFT_TO_TARGET_DISTANCE = _leftToTargetDistance;
            DEBUG_RIGHT_TO_TARGET_DISTANCE = _rightToTargetDistance;
            DEBUG_MARKER_TO_MARKER_DISTANCE = _markerToMarkerDistance;
        }
    }

    // отображение луча до цели с передней кромки танка
    void UpdateDebugToTargetRayRendering()
    {
        if (_Enable && _targetMarkerOne != null)
        {
            if (SHOW_FRONT_TO_TARGET_RAY)
            {
                Debug.DrawRay(_frontEdgePointPosition
                    + new Vector3(0, TO_TARGET_RAY_ELEVATION, 0), _frontToTargetDirection, Color.red);
            }

            if (SHOW_REAR_TO_TARGET_RAY)
            {
                Debug.DrawRay(_rearEdgePointPosition
                    + new Vector3(0, TO_TARGET_RAY_ELEVATION, 0), _rearToTargetDirection, Color.red);
            }

            if (SHOW_LEFT_TO_TARGET_RAY)
            {
                Debug.DrawRay(_leftEdgePointPosition
                    + new Vector3(0, TO_TARGET_RAY_ELEVATION, 0), _leftToTargetDirection, Color.red);
            }

            if (SHOW_RIGHT_TO_TARGET_RAY)
            {
                Debug.DrawRay(_rightEdgePointPosition
                    + new Vector3(0, TO_TARGET_RAY_ELEVATION, 0), _rightToTargetDirection, Color.red);
            }

            if (_targetMarkerTwo != null && SHOW_TO_SECOND_TARGET_RAY)
            {
                Debug.DrawRay(_targetMarkerOnePosition
                    + new Vector3(0, TO_TARGET_RAY_ELEVATION, 0), _markerToMarkerDirection, Color.red);
            }

        }
    }
    // рендерим дебаговые гизмо-направления на плоскости
    void UpdateTankDirectionRendering()
    {
        if (_Enable)
        {
            if (SHOW_FRONT_DIRECTION_RAY)
            {
                // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление в пространстве
                Debug.DrawRay(_frontEdgePointPosition
                    + new Vector3(0, DIRECTION_RAY_ELEVATION, 0), _tankFrontDirection, Color.green);
            }

            if (SHOW_REAR_DIRECTION_RAY)
            {
                // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление в пространстве
                Debug.DrawRay(_rearEdgePointPosition
                    + new Vector3(0, DIRECTION_RAY_ELEVATION, 0), _tankRearDirection, Color.green);
            }

            if (SHOW_LEFT_DIRECTION_RAY)
            {
                // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление в пространстве
                Debug.DrawRay(_leftEdgePointPosition
                    + new Vector3(0, DIRECTION_RAY_ELEVATION, 0), _tankLeftDirection, Color.green);
            }

            if (SHOW_RIGHT_DIRECTION_RAY)
            {
                // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление в пространстве
                Debug.DrawRay(_rightEdgePointPosition
                    + new Vector3(0, DIRECTION_RAY_ELEVATION, 0), _tankRightDirection, Color.green);
            }
        }
    }
}