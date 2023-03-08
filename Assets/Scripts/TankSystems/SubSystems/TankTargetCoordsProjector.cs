using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// НЕ ИСПОЛЬЗУЕТСЯ В ПРОЕКТЕ!

public class TankTargetCoordsProjector : MonoBehaviour
{
    [Header("Трансформы персональной системы координат объекта")]
    public Transform _unitGizmoZero;                                 // точка начала координат вызвавшего юнита
    public Transform _unitGizmoTargetMarker;                         // точка начала координат вызвавшего юнита

    public Transform _unitGizmoOZFront;                              // точка положения переднего края оси oZ
    public Transform _unitGizmoOZRear;                               // точка положения обратного края оси oZ

    public Transform _unitGizmoOYTop;                                // точка положения верхнего края оси oY
    public Transform _unitGizmoOYDown;                               // точка положения нижнего края оси oY

    public Transform _unitGizmoOXRight;                              // точка положения правого края оси oX
    public Transform _unitGizmoOXLeft;                               // точка положения левого края оси oX


    public float _unitGizmoScaler = 2.0f;                            // отступ от начала координат в обе стороны
    public bool _showUnitGizmoSystem = false;                        // включить отображение собственной системы координат юнита
    public float _globalGizmoScaler = 2.0f;                          // отступ от начала координат в обе стороны
    public bool _showGlobalGizmoSystem = false;                      // включить отображение глобальной системы координат юнита

    public Vector3 CURRENT_UNIT_GLOBAL_ROTATION;                     // показатель текущего вращения относительно глобальной системы координат

    private Transform _targetGlobalTransform;                        // получаемая поцизия цели в глобальной системе координат
    public void SetTargetGlobalTransform(ref Transform transform) { _targetGlobalTransform = transform; }
    public void SetTargetNullTransform() { _targetGlobalTransform = null; }
    public Transform GetTargetGlobalTransform() { return _targetGlobalTransform; }

    private bool _SelfTest = false;



    // --------------------------------- блок параметров углов поворота по осям ---------------------------------------

    private float oZ_axis_angle = 0.0f;                              // поворт объекта относительно глобальной оси oZ
    private float oY_axis_angle = 0.0f;                              // поворт объекта относительно глобальной оси oY
    private float oX_axis_angle = 0.0f;                              // поворт объекта относительно глобальной оси oX

    // --------------------------------- блок координат отрисовки глобальных осей -------------------------------------

    [Space]
    public Vector3 global_oZ_front = Vector3.zero;                  // координата начальной точки отрисовки луча оси oZ
    public Vector3 global_oZ_rear = Vector3.zero;                   // координата конечной точки отрисовки луча оси oZ

    public Vector3 global_oY_top = Vector3.zero;                    // координата начальной точки отрисовки луча оси oY
    public Vector3 global_oY_down = Vector3.zero;                   // координата конечной точки отрисовки луча оси oY

    public Vector3 global_oX_right = Vector3.zero;                  // координата начальной точки отрисовки луча оси oX
    public Vector3 global_oX_left = Vector3.zero;                   // координата конечной точки отрисовки луча оси oX


    // --------------------------------- блок координат отрисовки собственных осей ------------------------------------

    [Space]
    public Vector3 unit_oZ_front = Vector3.zero;                    // координата начальной точки отрисовки луча оси oZ
    public Vector3 unit_oZ_rear = Vector3.zero;                     // координата конечной точки отрисовки луча оси oZ

    public Vector3 unit_oY_top = Vector3.zero;                      // координата начальной точки отрисовки луча оси oY
    public Vector3 unit_oY_down = Vector3.zero;                     // координата конечной точки отрисовки луча оси oY

    public Vector3 unit_oX_right = Vector3.zero;                    // координата начальной точки отрисовки луча оси oX
    public Vector3 unit_oX_left = Vector3.zero;                     // координата конечной точки отрисовки луча оси oX


    private bool SubSystemSelfTest()
    {
        if (_unitGizmoZero == null) return false;

        return true;
    }

    private void Awake()
    {
        _SelfTest = SubSystemSelfTest();
    }

    private void FixedUpdate()
    {
        UpdateGlobalGizmoRotation();                         // определение текущих углов поворота объекта 
        UpdateGlobalGizmoSystem();                           // обновление и отрисовка глобальной системы координат
        UpdateStaticGizmoSystem();                           // обновление и отрисовка собственной системы координат
        UpdateUnitGizmoTargetMarker();
    }


    // определение текущих углов поворота объекта относительно глобальной системы координат
    private void UpdateGlobalGizmoRotation()
    {
        oZ_axis_angle = _unitGizmoZero.eulerAngles.z;
        oY_axis_angle = _unitGizmoZero.eulerAngles.y;
        oX_axis_angle = _unitGizmoZero.eulerAngles.x;

        // углы поворота необходимы для проецирования координат с помощью косинусов и синусов

        CURRENT_UNIT_GLOBAL_ROTATION = _unitGizmoZero.eulerAngles;
    }
    
    // обновление координат и отрисовка глобальной системы координат по заданной точке начала
    private void UpdateGlobalGizmoSystem()
    {
        // получаем позицию точки начала координат
        Vector3 zero_position = _unitGizmoZero.position;

        // ось Z - вперед-назад
        global_oZ_front = zero_position; global_oZ_front.z += _globalGizmoScaler;
        global_oZ_rear = zero_position; global_oZ_rear.z -= _globalGizmoScaler;

        // ось Y - вверх-вниз
        global_oY_top = zero_position; global_oY_top.y += _globalGizmoScaler;
        global_oY_down = zero_position; global_oY_down.y -= _globalGizmoScaler;

        // ось X - влево-вправо
        global_oX_right = zero_position; global_oX_right.x += _globalGizmoScaler;
        global_oX_left = zero_position; global_oX_left.x -= _globalGizmoScaler;

        if (_showGlobalGizmoSystem)
        {
            Debug.DrawLine(global_oZ_front, global_oZ_rear, Color.blue);
            Debug.DrawLine(global_oY_top, global_oY_down, Color.green);
            Debug.DrawLine(global_oX_right, global_oX_left, Color.red);
        }
    }

    private enum CoordsCalcMode
    {
        oZ, oY, oX
    }

    private void CoordsCalculator(ref Vector3 global_coord, ref Vector3 static_coord, ref float axis_angle, CoordsCalcMode mode)
    {
        switch (mode)
        {
            case CoordsCalcMode.oZ:
                break;
                
            case CoordsCalcMode.oY:

                static_coord.z = (global_coord.z * Mathf.Cos(axis_angle)) - (global_coord.z * Mathf.Sin(axis_angle));
                static_coord.y = global_coord.y;
                static_coord.x = (global_coord.x * Mathf.Sin(axis_angle)) + (global_coord.x * Mathf.Cos(axis_angle));

                //static_coord.z = (global_coord.z * Mathf.Cos(axis_angle)) + (global_coord.z * Mathf.Sin(axis_angle));
                //static_coord.y = global_coord.y;
                //static_coord.x = (-global_coord.x * Mathf.Sin(axis_angle)) + (global_coord.x * Mathf.Cos(axis_angle));

                break; 

            case CoordsCalcMode.oX:
                break;
        }

    }

    private void StaticGizmoOYProjection()
    {
        float angle = (oY_axis_angle * Mathf.PI) / 180;

        CoordsCalculator(ref global_oZ_front, ref unit_oZ_front, ref angle, CoordsCalcMode.oY);
        CoordsCalculator(ref global_oZ_rear, ref unit_oZ_rear, ref angle, CoordsCalcMode.oY);

        CoordsCalculator(ref global_oY_top, ref unit_oY_top, ref angle, CoordsCalcMode.oY);
        CoordsCalculator(ref global_oY_down, ref unit_oY_down, ref angle, CoordsCalcMode.oY);

        CoordsCalculator(ref global_oX_right, ref unit_oX_right, ref angle, CoordsCalcMode.oY);
        CoordsCalculator(ref global_oX_left, ref unit_oX_left, ref angle, CoordsCalcMode.oY);

    }

    private void UpdateUnitGizmoScale() 
    {
        Vector3 coords = _unitGizmoOZFront.localPosition;
        coords.z = _unitGizmoScaler; _unitGizmoOZFront.localPosition = coords;
        unit_oZ_front = _unitGizmoOZFront.position;

        coords = _unitGizmoOZRear.localPosition;
        coords.z = -_unitGizmoScaler; _unitGizmoOZRear.localPosition = coords;
        unit_oZ_rear = _unitGizmoOZRear.position;

        coords = _unitGizmoOYTop.localPosition;
        coords.y = _unitGizmoScaler; _unitGizmoOYTop.localPosition = coords;
        unit_oY_top = _unitGizmoOYTop.position;

        coords = _unitGizmoOYDown.localPosition;
        coords.y = -_unitGizmoScaler; _unitGizmoOYDown.localPosition = coords;
        unit_oY_down = _unitGizmoOYDown.position;

        coords = _unitGizmoOXRight.localPosition;
        coords.x = _unitGizmoScaler; _unitGizmoOXRight.localPosition = coords;
        unit_oX_right = _unitGizmoOXRight.position;

        coords = _unitGizmoOXLeft.localPosition;
        coords.x = -_unitGizmoScaler; _unitGizmoOXLeft.localPosition = coords;
        unit_oX_left = _unitGizmoOXLeft.position;
    }

    // обновление координат и отрисовка собственной системы координат по заданной точке начала
    private void UpdateStaticGizmoSystem()
    {
        UpdateUnitGizmoScale();

        if (_showUnitGizmoSystem)
        {
            Debug.DrawLine(unit_oZ_front, unit_oZ_rear, Color.blue);
            Debug.DrawLine(unit_oY_top, unit_oY_down, Color.green);
            Debug.DrawLine(unit_oX_right, unit_oX_left, Color.red);
        }
    }

    private void UpdateUnitGizmoTargetMarker()
    {
        if (_targetGlobalTransform != null)
        {
            _unitGizmoTargetMarker.position = _targetGlobalTransform.position;
        }
        else
        {
            _unitGizmoTargetMarker.localPosition = Vector3.zero;
        }
    }
}