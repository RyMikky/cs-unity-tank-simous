using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankNavigationPointSystem : MonoBehaviour
{
    [Tooltip("Включить выполнение скрипта")]
    public bool _Enable = false;

    public enum NavigationMode
    {
        Auto, Plane, Space
    }

    [Tooltip("Показатель среднего превышения высоты для смены режима навигации")]
    public float _navigationSelectDelta = 2f;
    [Tooltip("Тип работы навигационной системы - на плоскости или в пространстве")]
    public NavigationMode _navigationMode = NavigationMode.Auto;

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

    [Header("Флаги отображения направления разворота шасси танка на плоскости")]
    [Tooltip("Включить отображение луча прямого направления танка на плоскости")]
    public bool SHOW_FRONT_DIRECTION_RAY = false;
    [Tooltip("Включить отображение луча обратного направления танка на плоскости")]
    public bool SHOW_REAR_DIRECTION_RAY = false;
    [Tooltip("Включить отображение луча левого направления танка на плоскости")]
    public bool SHOW_LEFT_DIRECTION_RAY = false;
    [Tooltip("Включить отображение луча правого направления танка на плоскости")]
    public bool SHOW_RIGHT_DIRECTION_RAY = false;
    [Tooltip("Возвышение луча отрисовки направления танка на плоскости")]
    public float DIRECTION_RAY_ELEVATION = 0.5f;

    [Header("Флаги отображения лучей до цели с разных бортов танка")]
    [Tooltip("Включить отображение луча до цели от передней кромки")]
    public bool SHOW_FRONT_TO_TARGET_RAY = false;
    [Tooltip("Включить отображение луча до цели от задней кромки")]
    public bool SHOW_REAR_TO_TARGET_RAY = false;
    [Tooltip("Включить отображение луча до цели от левой кромки")]
    public bool SHOW_LEFT_TO_TARGET_RAY = false;
    [Tooltip("Включить отображение луча до цели от правой кромки")]
    public bool SHOW_RIGHT_TO_TARGET_RAY = false;
    [Tooltip("Возвышение луча отрисовки направления танка на плоскости")]
    public float TO_TARGET_RAY_ELEVATION = 0.5f;

    [Tooltip("Включить отображение луча от первого до второго маркера")]
    public bool SHOW_TO_SECOND_TARGET_RAY = false;

    [Header("Отладочные данные расчётов по первому маркеру")]
    [Tooltip("Отображает текущую дистанцию до первого маркера от центральной точки на плоскости")]
    public float DEBUG_CENTER_TO_MARKER_ONE_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до первого маркера от переднего борта на плоскости")]
    public float DEBUG_FRONT_TO_MARKER_ONE_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до первого маркера от заднего борта на плоскости")]
    public float DEBUG_REAR_TO_MARKER_ONE_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до первого маркера от левого борта на плоскости")]
    public float DEBUG_LEFT_TO_MARKER_ONE_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до первого маркера от правого борта на плоскости")]
    public float DEBUG_RIGHT_TO_MARKER_ONE_DISTANCE;
    
    [Tooltip("Скалярное произведение направления к первому маркеру от центрального вектора на плоскости")]
    public float DEBUG_CENTER_MARKER_ONE_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к первому маркеру от переднего вектора на плоскости")]
    public float DEBUG_FRONT_MARKER_ONE_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к первому маркеру от заднего вектора на плоскости")]
    public float DEBUG_REAR_MARKER_ONE_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к первому маркеру от левого вектора на плоскости")]
    public float DEBUG_LEFT_MARKER_ONE_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления к первому маркеру от правого вектора на плоскости")]
    public float DEBUG_RIGHT_MARKER_ONE_DOT_PRODUCT;

    [Header("Отладочные данные расчётов по второму маркеру")]
    [Tooltip("Отображает текущую дистанцию до второго маркера от центральной точки на плоскости")]
    public float DEBUG_CENTER_TO_MARKER_TWO_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до второго маркера от переднего борта на плоскости")]
    public float DEBUG_FRONT_TO_MARKER_TWO_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до второго маркера от заднего борта на плоскости")]
    public float DEBUG_REAR_TO_MARKER_TWO_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до второго маркера от левого борта на плоскости")]
    public float DEBUG_LEFT_TO_MARKER_TWO_DISTANCE;
    [Tooltip("Отображает текущую дистанцию до второго маркера от правого борта на плоскости")]
    public float DEBUG_RIGHT_TO_MARKER_TWO_DISTANCE;

    [Tooltip("Скалярное произведение направления ко второму маркеру от центрального вектора на плоскости")]
    public float DEBUG_CENTER_MARKER_TWO_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления ко второму маркеру от переднего вектора на плоскости")]
    public float DEBUG_FRONT_MARKER_TWO_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления ко второму маркеру от заднего вектора на плоскости")]
    public float DEBUG_REAR_MARKER_TWO_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления ко второму маркеру от левого вектора на плоскости")]
    public float DEBUG_LEFT_MARKER_TWO_DOT_PRODUCT;
    [Tooltip("Скалярное произведение направления ко второму маркеру от правого вектора на плоскости")]
    public float DEBUG_RIGHT_MARKER_TWO_DOT_PRODUCT;

    [Header("Прочая отладочная информация")]
    [Tooltip("Показатель текущей позиции танка на двухмерной плоскости")]
    public Vector3 DEBUG_TANK_POSITION;
    [Tooltip("Показатель текущей позиции первого маркера на двухмерной плоскости")]
    public Vector3 DEBUG_MARKER_ONE_POSITION;
    [Tooltip("Показатель текущей позиции второго маркера на двухмерной плоскости")]
    public Vector3 DEBUG_MARKER_TWO_POSITION;
    [Tooltip("Отображает текущую дистанцию от первого дт второго маркера на плоскости")]
    public float DEBUG_MARKER_TO_MARKER_DISTANCE;
    [Tooltip("Скалярное произведение направления к первому и второму маркеру от переднего борта на плоскости")]
    public float DEBUG_MARKER_DOT_PRODUCT;

    // ------------------------------------- навигационные данные для работы в плоскости ----------------------------------------------

    private Vector3 _tankFrontDirection;                         // текущее направление куда смотрит шасси танка на плоскости (c Axis Y = 0);
    private Vector3 _tankRearDirection;                          // текущее обратное направление на плоскости
    private Vector3 _tankLeftDirection;                          // текущее левое направление на плоскости
    private Vector3 _tankRightDirection;                         // текущее правое направление на плоскости
    private Vector3 _tankСenterDirection;                        // текущее направление куда смотрит шасси танка от центральной точки

    private Vector3 _frontToMarkerOneDirection;                  // текущее направление от передней грани танка до первого маркера на плоскости
    private Vector3 _rearToMarkerOneDirection;                   // текущее направление от задней грани танка до первого маркера на плоскости
    private Vector3 _leftToMarkerOneDirection;                   // текущее направление от левой грани танка до первого маркера на плоскости
    private Vector3 _rightToMarkerOneDirection;                  // текущее направление от правой грани танка до первого маркера на плоскости
    private Vector3 _centerToMarkerOneDirection;                 // текущее направление от центральной точки танка до первого маркера на плоскости

    private Vector3 _frontToMarkerTwoDirection;                  // текущее направление от передней грани танка до второго маркера на плоскости
    private Vector3 _rearToMarkerTwoDirection;                   // текущее направление от задней грани танка до второго маркера на плоскости
    private Vector3 _leftToMarkerTwoDirection;                   // текущее направление от левой грани танка до второго маркера на плоскости
    private Vector3 _rightToMarkerTwoDirection;                  // текущее направление от правой грани танка до второго маркера на плоскости
    private Vector3 _centerToMarkerTwoDirection;                 // текущее направление от центральной точки танка до второго маркера на плоскости

    private float _frontToMarkerOneDistance;                     // дистанция до первого маркера от передней кромки в двухмерной плоскости
    private float _rearToMarkerOneDistance;                      // дистанция до первого маркера от задней кромки в двухмерной плоскости
    private float _leftToMarkerOneDistance;                      // дистанция до первого маркера от левой кромки в двухмерной плоскости
    private float _rightToMarkerOneDistance;                     // дистанция до первого маркера от правой кромки в двухмерной плоскости
    private float _centerToMarkerOneDistance;                    // дистанция до первого маркера от центральной точки в двухмерной плоскости

    private float _frontToMarkerTwoDistance;                     // дистанция до второго маркера от передней кромки в двухмерной плоскости
    private float _rearToMarkerTwoDistance;                      // дистанция до второго маркера от задней кромки в двухмерной плоскости
    private float _leftToMarkerTwoDistance;                      // дистанция до второго маркера от левой кромки в двухмерной плоскости
    private float _rightToMarkerTwoDistance;                     // дистанция до второго маркера от правой кромки в двухмерной плоскости
    private float _centerToMarkerTwoDistance;                    // дистанция до второго маркера от центральной точки в двухмерной плоскости

    private float _frontMarkerOneDotProduct;                     // текущее скалярное произведение векторов направления шасси и первого маркера на плоскости
    private float _rearMarkerOneDotProduct;                      // текущее скалярное произведение обратного вектора направления и первого маркера на плоскости
    private float _leftMarkerOneDotProduct;                      // текущее скалярное произведение левого вектора направления и первого маркера на плоскости
    private float _rightMarkerOneDotProduct;                     // текущее скалярное произведение правого вектора направления и первого маркера на плоскости
    private float _centerMarkerOneDotProduct;                    // текущее скалярное произведение центрального вектора направления и первого маркера на плоскости

    private float _frontMarkerTwoDotProduct;                     // текущее скалярное произведение векторов направления шасси и первого маркера на плоскости
    private float _rearMarkerTwoDotProduct;                      // текущее скалярное произведение обратного вектора направления и первого маркера на плоскости
    private float _leftMarkerTwoDotProduct;                      // текущее скалярное произведение левого вектора направления и первого маркера на плоскости
    private float _rightMarkerTwoDotProduct;                     // текущее скалярное произведение правого вектора направления и первого маркера на плоскости
    private float _centerMarkerTwoDotProduct;                    // текущее скалярное произведение центрального вектора направления и первого маркера на плоскости

    private Vector3 _markerToMarkerDirection;                    // текущее направление от первого до второго маркера цели на плоскости
    private float _markerToMarkerDistance;                       // дистанция до цели от первого до второго маркера в двухмерной плоскости
    private float _markerDotProduct;                             // текущее скалярное произведение направления к первому и второму маркеру от переднего борта

    private Vector3 _frontDirectionPointPosition;                // проекция передней опорной точки танка на двухмерную плоскость
    private Vector3 _frontEdgePointPosition;                     // проекция точки передней границы танка на двухмерную плоскость

    private Vector3 _rearDirectionPointPosition;                 // проекция задней опорной точки танка на двухмерную плоскость
    private Vector3 _rearEdgePointPosition;                      // проекция точки задней границы танка на двухмерную плоскость

    private Vector3 _leftDirectionPointPosition;                 // проекция левой опорной точки танка на двухмерную плоскость
    private Vector3 _leftEdgePointPosition;                      // проекция точки левой границы танка на двухмерную плоскость

    private Vector3 _rightDirectionPointPosition;                // проекция правой опорной точки танка на двухмерную плоскость
    private Vector3 _rightEdgePointPosition;                     // проекция точки правой границы танка на двухмерную плоскость

    private Vector3 _centerPointPosition;                        // проекция центрально точки танка на двухмерную плоскость
    private Vector3 _targetMarkerOnePosition;                    // проекция позиции первого марера цели на двухмерную плоскость
    private Vector3 _targetMarkerTwoPosition;                    // проекция позиции второго марера цели на двухмерную плоскость

    private enum NaviModeExe
    {
        Plane, Space
    }
    [SerializeField]
    private NaviModeExe mode = NaviModeExe.Plane;

    // ------------------------------------- блок публичных сеттеров и геттеров класса ------------------------------------------------

    public void SetNavigationMode(NavigationMode mode) { _navigationMode = mode; }
    public NavigationMode GetNavigationMode() { return _navigationMode; }

    public void SetNavigationSelectDelta(float delta) { _navigationSelectDelta = delta; }
    public float GetNavigationSelectDelta() { return _navigationSelectDelta; }

    public void SetTargetMarkerOne(GameObject markerOne) { _targetMarkerOne = markerOne; }
    public GameObject GetTargetMarkerOne() { return _targetMarkerOne; }
    public void RemoveTargetMarkerOne() { _targetMarkerOne = null; }

    public void SetTargetMarkerTwo(GameObject markerTwo) { _targetMarkerTwo = markerTwo; }
    public GameObject GetTargetMarkerTwo() { return _targetMarkerTwo; }
    public void RemoveTargetMarkerTwo() { _targetMarkerTwo = null; }

    public void SetDirectionRayElevation(float elevation) { DIRECTION_RAY_ELEVATION = elevation; }
    public float GetDirectionRayElevation() { return DIRECTION_RAY_ELEVATION; }
    public void SetToTargetRayElevation(float elevation) { TO_TARGET_RAY_ELEVATION = elevation; }
    public float GetToTargetRayElevation() { return TO_TARGET_RAY_ELEVATION; }

    public float GetCenterToMarkerOneDistance() { return _centerToMarkerOneDistance; }
    public float GetFrontToMarkerOneDistance() { return _frontToMarkerOneDistance; }
    public float GetRearToMarkerOneDistance() { return _rearToMarkerOneDistance; }
    public float GetLeftToMarkerOneDistance() { return _leftToMarkerOneDistance; }
    public float GetRightToMarkerOneDistance() { return _rightToMarkerOneDistance; }
    
    public float GetFrontMarkerOneDotProduct() { return _frontMarkerOneDotProduct; }
    public float GetRearMarkerOneDotProduct() { return _rearMarkerOneDotProduct; }
    public float GetLeftMarkerOneDotProduct() { return _leftMarkerOneDotProduct; }
    public float GetRightMarkerOneDotProduct() { return _rightMarkerOneDotProduct; }
    public float GetCenterMarkerOneDotProduct() { return _centerMarkerOneDotProduct; }

    public float GetCenterToMarkerTwoDistance() { return _centerToMarkerTwoDistance; }
    public float GetFrontToMarkerTwoDistance() { return _frontToMarkerTwoDistance; }
    public float GetRearToMarkerTwoDistance() { return _rearToMarkerTwoDistance; }
    public float GetLeftToMarkerTwoDistance() { return _leftToMarkerTwoDistance; }
    public float GetRightToMarkerTwoDistance() { return _rightToMarkerTwoDistance; }

    public float GetFrontMarkerTwoDotProduct() { return _frontMarkerTwoDotProduct; }
    public float GetRearMarkerTwoDotProduct() { return _rearMarkerTwoDotProduct; }
    public float GetLeftMarkerTwoDotProduct() { return _leftMarkerTwoDotProduct; }
    public float GetRightMarkerTwoDotProduct() { return _rightMarkerTwoDotProduct; }
    public float GetCenterMarkerTwoDotProduct() { return _centerMarkerTwoDotProduct; }

    public float GetMarkerToMarkerDistance() { return _markerToMarkerDistance; }
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

        UpdateNavigationMode();                      // определение режима работы навигации
        UpdateNavigationPoints();                    // определение навигационных координат танка и цели
        UpdateNavigationDirection();                 // определение навигационных направлений бортов танка
        UpdateTankToTargetDirection();               // определение направлений от бортов танка до цели 

        // ------------------------- блок вычисления навигационных показателей --------------------------------------

        UpdateToTargetDotProduct();                  // вычисление скалярных произведений направлений
        UpdateToTargetDistance();                    // расчёт дистанции до цели

        // ------------------------- блок рендеринга навигационных направлений --------------------------------------

        UpdateDebugToTargetRayRendering();           // отображение луча до цели с передней кромки танка
        UpdateTankDirectionRendering();              // рендерим дебаговые гизмо-направления на плоскости
    }

    // определение режима работы навигации
    void UpdateNavigationMode()
    {
        if (_Enable)
        {
            switch (_navigationMode)
            {
                case NavigationMode.Plane:
                    mode = NaviModeExe.Plane; 
                    break;

                case NavigationMode.Space:
                    mode = NaviModeExe.Space;
                    break;

                case NavigationMode.Auto:

                    if (_targetMarkerOne != null)
                    {

                        if ((Mathf.Abs(_tankCenterPoint.transform.position.y - _targetMarkerOne.transform.position.y) < _navigationSelectDelta))
                        {
                            mode = NaviModeExe.Plane;
                        }
                        else
                        {
                            mode = NaviModeExe.Space;
                        }
                    }
                    else
                    {
                        mode = NaviModeExe.Plane;
                    }

                break;
            }
        }
    }

    // определение навигационных координат танка и цели
    void UpdateNavigationPoints()
    {
        if (_Enable)
        {
            // проекция центральной опорной точки танка на двухмерную плоскость
            _centerPointPosition = _tankCenterPoint.transform.position;
            if (mode == NaviModeExe.Plane) _centerPointPosition.y = 0;

            // проекция передней опорной точки танка на двухмерную плоскость
            _frontDirectionPointPosition = _tankFrontDirectionPoint.transform.position;
            if (mode == NaviModeExe.Plane) _frontDirectionPointPosition.y = 0;

            // проекция точки передней границы танка на двухмерную плоскость
            _frontEdgePointPosition = _tankFrontEdgePoint.transform.position;
            if (mode == NaviModeExe.Plane) _frontEdgePointPosition.y = 0;

            // проекция задней опорной точки танка на двухмерную плоскость
            _rearDirectionPointPosition = _tankRearDirectionPoint.transform.position;
            if (mode == NaviModeExe.Plane) _rearDirectionPointPosition.y = 0;

            // проекция точки задней границы танка на двухмерную плоскость
            _rearEdgePointPosition = _tankRearEdgePoint.transform.position;
            if (mode == NaviModeExe.Plane) _rearEdgePointPosition.y = 0;

            // проекция левой опорной точки танка на двухмерную плоскость
            _leftDirectionPointPosition = _tankLeftDirectionPoint.transform.position;
            if (mode == NaviModeExe.Plane) _leftDirectionPointPosition.y = 0;

            // проекция точки левой границы танка на двухмерную плоскость
            _leftEdgePointPosition = _tankLeftEdgePoint.transform.position;
            if (mode == NaviModeExe.Plane) _leftEdgePointPosition.y = 0;

            // проекция правой опорной точки танка на двухмерную плоскость
            _rightDirectionPointPosition = _tankRightDirectionPoint.transform.position;
            if (mode == NaviModeExe.Plane) _rightDirectionPointPosition.y = 0;

            // проекция точки правой границы танка на двухмерную плоскость
            _rightEdgePointPosition = _tankRightEdgePoint.transform.position;
            if (mode == NaviModeExe.Plane) _rightEdgePointPosition.y = 0;


            if (_targetMarkerOne != null)
            {
                // проекция позиции первого марекера цели на двухмерную плоскость
                _targetMarkerOnePosition = _targetMarkerOne.transform.position;
                if (mode == NaviModeExe.Plane) _targetMarkerOnePosition.y = 0;
            }

            if (_targetMarkerTwo != null)
            {
                // проекция позиции второго марекера цели на двухмерную плоскость
                _targetMarkerTwoPosition = _targetMarkerTwo.transform.position;
                if (mode == NaviModeExe.Plane) _targetMarkerTwoPosition.y = 0;
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

            // определяем вектор направления поворота шасси танка относительно центральной точки
            _tankСenterDirection = _frontDirectionPointPosition - _centerPointPosition;
        }
    }
    // определение направлений от бортов танка до цели 
    void UpdateTankToTargetDirection()
    {
        if (_Enable)
        {
            if (_targetMarkerOne != null)
            {
                // определяем вектор направления до первого маркера от переднего борта на плоскости
                _frontToMarkerOneDirection = _targetMarkerOnePosition - _frontEdgePointPosition;

                // определяем вектор направления до первого маркера от заднего борта на плоскости
                _rearToMarkerOneDirection = _targetMarkerOnePosition - _rearEdgePointPosition;

                // определяем вектор направления до первого маркера от левого борта на плоскости
                _leftToMarkerOneDirection = _targetMarkerOnePosition - _leftEdgePointPosition;

                // определяем вектор направления до первого маркера от правого борта на плоскости
                _rightToMarkerOneDirection = _targetMarkerOnePosition - _rightEdgePointPosition;

                // определяем вектор направления до первого маркера от центральной точки на плоскости
                _centerToMarkerOneDirection = _targetMarkerOnePosition - _centerPointPosition;

                // если есть второй маркер и его позиция не совпадает с позицией первого маркера
                if (_targetMarkerTwo != null && _targetMarkerTwoPosition != _targetMarkerOnePosition)
                {
                    // определяем вектор направления до второго маркера от переднего борта на плоскости
                    _frontToMarkerTwoDirection = _targetMarkerTwoPosition - _frontEdgePointPosition;

                    // определяем вектор направления до второго маркера от заднего борта на плоскости
                    _rearToMarkerTwoDirection = _targetMarkerTwoPosition - _rearEdgePointPosition;

                    // определяем вектор направления до второго маркера от левого борта на плоскости
                    _leftToMarkerTwoDirection = _targetMarkerTwoPosition - _leftEdgePointPosition;

                    // определяем вектор направления до второго маркера от правого борта на плоскости
                    _rightToMarkerTwoDirection = _targetMarkerTwoPosition - _rightEdgePointPosition;

                    // определяем вектор направления до второго маркера от центральной точки на плоскости
                    _centerToMarkerTwoDirection = _targetMarkerTwoPosition - _centerPointPosition;


                    _markerToMarkerDirection = _targetMarkerTwoPosition - _targetMarkerOnePosition;
                }
                else
                {
                    _frontToMarkerTwoDirection = Vector3.zero;
                    _rearToMarkerTwoDirection = Vector3.zero;
                    _leftToMarkerTwoDirection = Vector3.zero;
                    _rightToMarkerTwoDirection = Vector3.zero;
                    _centerToMarkerTwoDirection = Vector3.zero;

                    _markerToMarkerDirection = Vector3.zero;
                }
            }
            else
            {
                _frontToMarkerOneDirection = Vector3.zero;
                _rearToMarkerOneDirection = Vector3.zero;
                _leftToMarkerOneDirection = Vector3.zero;
                _rightToMarkerOneDirection = Vector3.zero;
                _centerToMarkerOneDirection = Vector3.zero;

                _frontToMarkerTwoDirection = Vector3.zero;
                _rearToMarkerTwoDirection = Vector3.zero;
                _leftToMarkerTwoDirection = Vector3.zero;
                _rightToMarkerTwoDirection = Vector3.zero;
                _centerToMarkerTwoDirection = Vector3.zero;

                _markerToMarkerDirection = Vector3.zero;
            }
        }
    }

    // вычисление скалярный произведених направлений
    void UpdateToTargetDotProduct()
    {
        if (_Enable)
        {
            if (_targetMarkerOne != null)
            {
                _frontMarkerOneDotProduct = Vector3.Dot(_tankFrontDirection.normalized, _frontToMarkerOneDirection.normalized);
                _rearMarkerOneDotProduct = Vector3.Dot(_tankRearDirection.normalized, _rearToMarkerOneDirection.normalized);
                _leftMarkerOneDotProduct = Vector3.Dot(_tankLeftDirection.normalized, _leftToMarkerOneDirection.normalized);
                _rightMarkerOneDotProduct = Vector3.Dot(_tankRightDirection.normalized, _rightToMarkerOneDirection.normalized);
                _centerMarkerOneDotProduct = Vector3.Dot(_tankСenterDirection.normalized, _centerToMarkerOneDirection.normalized);

                if (_targetMarkerTwo != null && _targetMarkerTwoPosition != _targetMarkerOnePosition)
                {
                    _frontMarkerTwoDotProduct = Vector3.Dot(_tankFrontDirection.normalized, _frontToMarkerTwoDirection.normalized);
                    _rearMarkerTwoDotProduct = Vector3.Dot(_tankRearDirection.normalized, _rearToMarkerTwoDirection.normalized);
                    _leftMarkerTwoDotProduct = Vector3.Dot(_tankLeftDirection.normalized, _leftToMarkerTwoDirection.normalized);
                    _rightMarkerTwoDotProduct = Vector3.Dot(_tankRightDirection.normalized, _rightToMarkerTwoDirection.normalized);
                    _centerMarkerTwoDotProduct = Vector3.Dot(_tankСenterDirection.normalized, _centerToMarkerTwoDirection.normalized);

                    _markerDotProduct = Vector3.Dot(_frontToMarkerOneDirection.normalized, _markerToMarkerDirection.normalized);
                }
                else
                {
                    _frontMarkerTwoDotProduct = 0; _rearMarkerTwoDotProduct = 0;
                    _leftMarkerTwoDotProduct = 0; _rightMarkerTwoDotProduct = 0;
                    _centerMarkerTwoDotProduct = 0;

                    _markerDotProduct = 0;
                }
                
            }
            else
            {
                _frontMarkerOneDotProduct = 0; _rearMarkerOneDotProduct = 0;
                _leftMarkerOneDotProduct = 0; _rightMarkerOneDotProduct = 0;
                _centerMarkerOneDotProduct = 0;

                _frontMarkerTwoDotProduct = 0; _rearMarkerTwoDotProduct = 0;
                _leftMarkerTwoDotProduct = 0; _rightMarkerTwoDotProduct = 0;
                _centerMarkerTwoDotProduct = 0;

                _markerDotProduct = 0;
            }

            DEBUG_FRONT_MARKER_ONE_DOT_PRODUCT = _frontMarkerOneDotProduct;
            DEBUG_REAR_MARKER_ONE_DOT_PRODUCT = _rearMarkerOneDotProduct;
            DEBUG_LEFT_MARKER_ONE_DOT_PRODUCT = _leftMarkerOneDotProduct;
            DEBUG_RIGHT_MARKER_ONE_DOT_PRODUCT = _rightMarkerOneDotProduct;
            DEBUG_CENTER_MARKER_ONE_DOT_PRODUCT = _centerMarkerOneDotProduct;

            DEBUG_FRONT_MARKER_TWO_DOT_PRODUCT = _frontMarkerTwoDotProduct;
            DEBUG_REAR_MARKER_TWO_DOT_PRODUCT = _rearMarkerTwoDotProduct;
            DEBUG_LEFT_MARKER_TWO_DOT_PRODUCT = _leftMarkerTwoDotProduct;
            DEBUG_RIGHT_MARKER_TWO_DOT_PRODUCT = _rightMarkerTwoDotProduct;
            DEBUG_CENTER_MARKER_TWO_DOT_PRODUCT = _centerMarkerTwoDotProduct;

            DEBUG_MARKER_DOT_PRODUCT = _markerDotProduct;
        }
    }
    // обновление дистанции до цели, если она задана
    void UpdateToTargetDistance()
    {
        if (_Enable)
        {
            // если скрипт включен и имеется заданная цель
            if (_targetMarkerOne != null)
            {
                // записываем проекционную дистанцию от центральной точки до цели на плоскости 
                _centerToMarkerOneDistance = Vector3.Distance(_targetMarkerOnePosition, _centerPointPosition);

                // записываем проекционную дистанцию от передней кромки до цели на плоскости 
                _frontToMarkerOneDistance = Vector3.Distance(_targetMarkerOnePosition, _frontEdgePointPosition);

                // записываем проекционную дистанцию от задней кромки до цели на плоскости 
                _rearToMarkerOneDistance = Vector3.Distance(_targetMarkerOnePosition, _rearEdgePointPosition);

                // записываем проекционную дистанцию от левой кромки до цели на плоскости 
                _leftToMarkerOneDistance = Vector3.Distance(_targetMarkerOnePosition, _leftEdgePointPosition);

                // записываем проекционную дистанцию от правой кромки до цели на плоскости 
                _rightToMarkerOneDistance = Vector3.Distance(_targetMarkerOnePosition, _rightEdgePointPosition);


                // записываем данные о позиции цели в пространстве
                DEBUG_MARKER_ONE_POSITION = _targetMarkerOnePosition;

                if (_targetMarkerTwo != null && _targetMarkerOnePosition != _targetMarkerTwoPosition)
                {
                    // записываем проекционную дистанцию от центральной точки до цели на плоскости 
                    _centerToMarkerTwoDistance = Vector3.Distance(_targetMarkerTwoPosition, _centerPointPosition);

                    // записываем проекционную дистанцию от передней кромки до цели на плоскости 
                    _frontToMarkerTwoDistance = Vector3.Distance(_targetMarkerTwoPosition, _frontEdgePointPosition);

                    // записываем проекционную дистанцию от задней кромки до цели на плоскости 
                    _rearToMarkerTwoDistance = Vector3.Distance(_targetMarkerTwoPosition, _rearEdgePointPosition);

                    // записываем проекционную дистанцию от левой кромки до цели на плоскости 
                    _leftToMarkerTwoDistance = Vector3.Distance(_targetMarkerTwoPosition, _leftEdgePointPosition);

                    // записываем проекционную дистанцию от правой кромки до цели на плоскости 
                    _rightToMarkerTwoDistance = Vector3.Distance(_targetMarkerTwoPosition, _rightEdgePointPosition);

                    // записываем данные о позиции цели в пространстве
                    DEBUG_MARKER_TWO_POSITION = _targetMarkerTwoPosition;

                    _markerToMarkerDistance = Vector3.Distance(_targetMarkerTwoPosition, _targetMarkerOnePosition);
                }
                else
                {
                    _markerToMarkerDistance = 0; _centerToMarkerTwoDistance = 0;
                    _frontToMarkerTwoDistance = 0; _rearToMarkerTwoDistance = 0;
                    _leftToMarkerTwoDistance = 0; _rightToMarkerTwoDistance = 0;
                    

                    DEBUG_MARKER_TWO_POSITION = Vector3.zero;
                }
            }
            // если цели нет
            else
            {
                // обнуляем данные о глобальной позиции цели, глобальные дистанции до цели
                _centerToMarkerOneDistance = 0;
                _frontToMarkerOneDistance = 0; _rearToMarkerOneDistance = 0;
                _leftToMarkerOneDistance = 0; _rightToMarkerOneDistance = 0;

                _markerToMarkerDistance = 0; _centerToMarkerTwoDistance = 0;
                _frontToMarkerTwoDistance = 0; _rearToMarkerTwoDistance = 0;
                _leftToMarkerTwoDistance = 0; _rightToMarkerTwoDistance = 0;


                DEBUG_MARKER_ONE_POSITION = Vector3.zero;
                DEBUG_MARKER_TWO_POSITION = Vector3.zero;
            }

            // записываем деббаговые поля согласно полученным данным в условии выше
            DEBUG_TANK_POSITION = _frontEdgePointPosition;

            DEBUG_CENTER_TO_MARKER_ONE_DISTANCE = _centerToMarkerOneDistance;
            DEBUG_FRONT_TO_MARKER_ONE_DISTANCE = _frontToMarkerOneDistance;
            DEBUG_REAR_TO_MARKER_ONE_DISTANCE = _rearToMarkerOneDistance;
            DEBUG_LEFT_TO_MARKER_ONE_DISTANCE = _leftToMarkerOneDistance;
            DEBUG_RIGHT_TO_MARKER_ONE_DISTANCE = _rightToMarkerOneDistance;

            DEBUG_CENTER_TO_MARKER_TWO_DISTANCE = _centerToMarkerTwoDistance;
            DEBUG_FRONT_TO_MARKER_TWO_DISTANCE = _frontToMarkerTwoDistance;
            DEBUG_REAR_TO_MARKER_TWO_DISTANCE = _rearToMarkerTwoDistance;
            DEBUG_LEFT_TO_MARKER_TWO_DISTANCE = _leftToMarkerTwoDistance;
            DEBUG_RIGHT_TO_MARKER_TWO_DISTANCE = _rightToMarkerTwoDistance;

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
                    + new Vector3(0, TO_TARGET_RAY_ELEVATION, 0), _frontToMarkerOneDirection, Color.red);
            }

            if (SHOW_REAR_TO_TARGET_RAY)
            {
                Debug.DrawRay(_rearEdgePointPosition 
                    + new Vector3(0, TO_TARGET_RAY_ELEVATION, 0), _rearToMarkerOneDirection, Color.red);
            }

            if (SHOW_LEFT_TO_TARGET_RAY)
            {
                Debug.DrawRay(_leftEdgePointPosition 
                    + new Vector3(0, TO_TARGET_RAY_ELEVATION, 0), _leftToMarkerOneDirection, Color.red);
            }

            if (SHOW_RIGHT_TO_TARGET_RAY)
            {
                Debug.DrawRay(_rightEdgePointPosition 
                    + new Vector3(0, TO_TARGET_RAY_ELEVATION, 0), _rightToMarkerOneDirection, Color.red);
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
                // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление на плоскости
                Debug.DrawRay(_frontEdgePointPosition
                    + new Vector3(0, DIRECTION_RAY_ELEVATION, 0), _tankFrontDirection, Color.green);
            }

            if (SHOW_REAR_DIRECTION_RAY)
            {
                // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление на плоскости
                Debug.DrawRay(_rearEdgePointPosition
                    + new Vector3(0, DIRECTION_RAY_ELEVATION, 0), _tankRearDirection, Color.green);
            }

            if (SHOW_LEFT_DIRECTION_RAY)
            {
                // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление на плоскости
                Debug.DrawRay(_leftEdgePointPosition
                    + new Vector3(0, DIRECTION_RAY_ELEVATION, 0), _tankLeftDirection, Color.green);
            }

            if (SHOW_RIGHT_DIRECTION_RAY)
            {
                // если включен флаг, то в дебаге будет рисоваться зеленая линия показывающая направление на плоскости
                Debug.DrawRay(_rightEdgePointPosition
                    + new Vector3(0, DIRECTION_RAY_ELEVATION, 0), _tankRightDirection, Color.green);
            }
        }
    }
}