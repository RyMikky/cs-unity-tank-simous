using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TankTowerSystem : MonoBehaviour
{

    // --------------------------------- основные флаги и режимы работы скрипта ----------------------------------

    public enum SystemMode
    {
        Disable, Tank, Artillery
    }

    public SystemMode _systemMode = SystemMode.Disable;   // базовый режим работы класса

    public SystemMode GetSystemMode() { return _systemMode; }
    public void SetSystemMode(SystemMode mode) { _systemMode = mode; }

    public bool _HaveTarget = false;                     // флаг наличия цели
    public bool _ShowDistanceTriangle = false;           // включает отрисовку треугольника дистанции до цели

    // --------------------------------- основные геометрические точки привязки ----------------------------------

    [Header("Базовые трансформы положений и осей башни танка")]
    public Transform _TargetPositionPoint;               // позиция цели в глобальной системе координат
    public Transform _TowerRotationAxisPoint;            // позиция оси вращения башни танка (режим танк)
    public Transform _GunLevelingAxisPoint;              // позиция оси возвышения орудия танка (оба режима)
    public Transform _GunEdgePoint;                      // позиция оконечной точки орудия танка (оба режима)
    public Transform _TankFrontPoint;                    // позиция точки фронтальной привязки танка (режим танк)

    // --------------------------------- нестройки вращения и возвышения орудия ----------------------------------

    [Header("Параметры скорости вращения башни и возвышения орудия танка")]
    public float _TowerRotationSpeed = 1f;               // базовая скорость вращения башни (режим танк)
    public float _GunLevelingSpeed = 1f;                 // базовая скорость возвышения ствола (оба режима)
    public float _GunUpAngleLimmit = 12f;                // максимальный угол возвышеня ствола (оба режима)
    public float _GunDownAngleLimmit = 4f;               // максимальный угол опускания ствола (оба режима)
    public float _GunDefaultAngle = 4f;                  // базовое угол возвышения орудия без цели

    // --------------------------------- поля настройки стрельбы -------------------------------------------------

    [Header("Параметры силы выстрела и время жизни снаряда")]
    public float _GunAttackPower = 10f;                  // базовая мощность выстрела пушки
    public float _BulletLifeTime = 2f;                   // время существования снаряда

    // --------------------------------- поля настройки осей орудия танка ----------------------------------------

    [Header("Основные трансформы локальной системы координат орудия танка")]
    public Transform _GunLocalGizmoZeroPoint;            // точка начала системы координат орудия танка
    public Transform _GunLocalGizmoTargetMarker;         // точка положения цели в системе координат орудия

    public Transform _GunLocalAxisZFrontPoint;           // точка на оси OZ системы координат орудия танка
    public Transform _GunLocalAxisZRearPoint;            // точка на оси -OZ системы координат орудия танка

    public Transform _GunLocalAxisYTopPoint;             // точка на оси OY системы координат орудия танка
    public Transform _GunLocalAxisYDownPoint;            // точка на оси -OY системы координат орудия танка

    public Transform _GunLocalAxisXRightPoint;           // точка на оси OX системы координат орудия танка
    public Transform _GunLocalAxisXLeftPoint;            // точка на оси -OX системы координат орудия танка

    public float _GunLocalGizmoRayScaler = 0.0f;         // величина удаления точек осей от начала координат орудия танка
    public bool _ShowGunLocalGizmoRays = false;          // включает отображение локальной системы координат орудия танка
    public bool _ShowGunLocalRaysInGlobal = false;       // включает оторбражение локальной системы в глобальной

    // --------------------------------- поля настройки осей башни танка -----------------------------------------

    [Header("Основные трансформы локальной системы координат башни танка")]
    public Transform _TowerLocalGizmoZeroPoint;          // точка начала системы координат башни танка
    public Transform _TowerLocalGizmoTargetMarker;       // точка положения цели в системе координат башни

    public Transform _TowerLocalAxisZFrontPoint;         // точка на оси OZ системы координат башни танка
    public Transform _TowerLocalAxisZRearPoint;          // точка на оси -OZ системы координат башни танка

    public Transform _TowerLocalAxisYTopPoint;           // точка на оси OY системы координат башни танка
    public Transform _TowerLocalAxisYDownPoint;          // точка на оси -OY системы координат башни танка

    public Transform _TowerLocalAxisXRightPoint;         // точка на оси OX системы координат башни танка
    public Transform _TowerLocalAxisXLeftPoint;          // точка на оси -OX системы координат башни танка

    public float _TowerLocalGizmoRayScaler = 0.0f;       // величина удаления точек осей от начала координат башни танка
    public bool _ShowTowerLocalGizmoRays = false;        // включает отображение локальной системы координат башни танка
    public bool _ShowTowerLocalRaysInGlobal = false;     // включает оторбражение локальной системы в глобальной


    [Header("Дополнительные вспомогательные объекты класса")]
    // --------------------------------- дополнительные объекты --------------------------------------------------

    public Camera _TankRayCastCamera;                    // основная камера которая будет "стрелять" рейкастами
    public GameObject _TankGunBullet;                    // танковый снаряд, которым будет происходить выстрел

    // --------------------------------- закрытые внутренние поля ------------------------------------------------

    private bool _SelfCheck = false;                                       // флаг прохождения самотеста
    private Vector3 _GunRotation = new Vector3(0, 0, 0);                   // закрытое поле возвышения ствола
    private float _Distance = 0;                                           // расстояние до цели
    private Transform _OnMouseOver;                                        // поле положения указателя мыши
    private Transform _CurrentTankTransform = null;                        // текущий трансформ танка

    public enum PositionRoll
    {
        None, LeftRoll, RightRoll, ForwardRoll, RearRoll,
        ForwardLeft, ForwardRight, RearLeft, RearRight
    }

    private PositionRoll _TankPositionRoll = PositionRoll.None;            // крен корпуса танка в зависимости от рельефа

    // --------------------------------- поля отладочной информации -----------------------------------------------

    [Header("Блок деббаговых данных")]
    public float CURRENT_LEVELING_ANGLE = 0.0f;
    public PositionRoll CURRENT_TANK_ROLL = PositionRoll.None;

    public Vector3 CURRENT_TARGET_POSITION = Vector3.zero;


    // --------------------------------- методы предварительной проверки запуска скрипта -------------------------

    // проверка наличия всех требуемых объектов для работы локальной системы координат орудия башни танка
    private bool GunLocalGizmoCheck()
    {
        if (_GunLocalGizmoZeroPoint == null) return false;            // точка начала координат
        if (_GunLocalGizmoTargetMarker == null) return false;         // проекция цели в системе коорднат

        if (_GunLocalAxisZFrontPoint == null) return false;           // ось OZ
        if (_GunLocalAxisZRearPoint == null) return false;            // ось -OZ

        if (_GunLocalAxisYTopPoint == null) return false;             // ось OY
        if (_GunLocalAxisYDownPoint == null) return false;            // ось -OY

        if (_GunLocalAxisXRightPoint == null) return false;           // ось OX
        if (_GunLocalAxisXLeftPoint == null) return false;            // ось -OX

        return true;
    }
    // проверка наличия всех требуемых объектов для работы локальной системы координат башни танка
    private bool TowerLocalGizmoCheck()
    {
        if (_TowerLocalGizmoZeroPoint == null) return false;          // точка начала координат
        if (_TowerLocalGizmoTargetMarker == null) return false;       // проекция цели в системе коорднат

        if (_TowerLocalAxisZFrontPoint == null) return false;         // ось OZ
        if (_TowerLocalAxisZRearPoint == null) return false;          // ось -OZ

        if (_TowerLocalAxisYTopPoint == null) return false;           // ось OY
        if (_TowerLocalAxisYDownPoint == null) return false;          // ось -OY

        if (_TowerLocalAxisXRightPoint == null) return false;         // ось OX
        if (_TowerLocalAxisXLeftPoint == null) return false;          // ось -OX

        return true;
    }

    private bool SystemArtilleryModeCheck()
    {
        if (_GunLevelingAxisPoint == null) return false;       // ось возвышения орудия обязательна
        if (_GunEdgePoint == null) return false;               // крайняя точка орудия обязательна
        if (_TankRayCastCamera == null) return false;          // камера для рейкаста обязательна
        if (_TankGunBullet == null) return false;              // снаряд для выстрела обязателен

        if (!GunLocalGizmoCheck()) return false;

        return true;
    }
    private bool SystemTankModeCheck()
    {
        if (_TowerRotationAxisPoint == null) return false;     // ось вращения башни танка обязательна
        if (_GunLevelingAxisPoint == null) return false;       // ось возвышения орудия обязательна
        if (_GunEdgePoint == null) return false;               // крайняя точка орудия обязательна
        if (_TankFrontPoint == null) return false;             // фронтальная точка танка обязательна
        if (_TankRayCastCamera == null) return false;          // камера для рейкаста обязательна
        if (_TankGunBullet == null) return false;              // снаряд для выстрела обязателен

        if (!GunLocalGizmoCheck()) return false;
        if (!TowerLocalGizmoCheck()) return false;

        return true;
    }
    public bool SystemSelfTest() 
    {
        switch (_systemMode)
        {
            // при выключенном скрипте тестирование проходить не надо
            case SystemMode.Disable:
                return false;

            case SystemMode.Tank:
                return SystemTankModeCheck();

            case SystemMode.Artillery:
                return SystemArtilleryModeCheck();
        }

        return false;
    }

    private void Awake()
    {
        _SelfCheck = SystemSelfTest();
        _CurrentTankTransform = GetComponent<Transform>();
    }


    // --------------------------------- методы для работы с осями орудия и башни --------------------------------

    // обновление координат позиций осей локальной системы координат орудия башни танка
    private void UpdateGunLocalGizmoScale()
    {
        Vector3 pos = _GunLocalAxisZFrontPoint.localPosition;

        if (pos.z != _GunLocalGizmoRayScaler)
        {
            pos.z = _GunLocalGizmoRayScaler; _GunLocalAxisZFrontPoint.localPosition = pos;

            pos = _GunLocalAxisZRearPoint.localPosition;
            pos.z = -_GunLocalGizmoRayScaler; _GunLocalAxisZRearPoint.localPosition = pos;

            pos = _GunLocalAxisYTopPoint.localPosition;
            pos.y = _GunLocalGizmoRayScaler; _GunLocalAxisYTopPoint.localPosition = pos;

            pos = _GunLocalAxisYDownPoint.localPosition;
            pos.y = -_GunLocalGizmoRayScaler; _GunLocalAxisYDownPoint.localPosition = pos;

            pos = _GunLocalAxisXRightPoint.localPosition;
            pos.x = _GunLocalGizmoRayScaler; _GunLocalAxisXRightPoint.localPosition = pos;

            pos = _GunLocalAxisXLeftPoint.localPosition;
            pos.x = -_GunLocalGizmoRayScaler; _GunLocalAxisXLeftPoint.localPosition = pos;
        }
    }
    // обновление координат маркера цели в системе координат орудия танка
    private void UpdateGunLocalTargetMarker()
    {
        if (_TargetPositionPoint != null)
        {
            if (_GunLocalGizmoTargetMarker.position != _TargetPositionPoint.position)
            {
                _GunLocalGizmoTargetMarker.position = _TargetPositionPoint.position;
            }
        }
        else
        {
            _GunLocalGizmoTargetMarker.localPosition = Vector3.zero;
        }
    }
    // отрисовка лучей локальной системы координат орудия танка
    private void DrawGunLocalGizmo()
    {
        if (_ShowGunLocalGizmoRays)
        {
            Debug.DrawLine(_GunLocalAxisZFrontPoint.position, _GunLocalAxisZRearPoint.position, Color.blue);
            Debug.DrawLine(_GunLocalAxisYTopPoint.position, _GunLocalAxisYDownPoint.position, Color.green);
            Debug.DrawLine(_GunLocalAxisXRightPoint.position, _GunLocalAxisXLeftPoint.position, Color.red);
        }
    }
    // проекция локальных координат орудия танка на глобальную ось
    private void DrawProjectGunLocalToGlobalGizmo()
    {
        if (_ShowGunLocalRaysInGlobal)
        {
            Debug.DrawLine(_GunLocalAxisZFrontPoint.localPosition, _GunLocalAxisZRearPoint.localPosition, Color.blue);
            Debug.DrawLine(_GunLocalAxisYTopPoint.localPosition, _GunLocalAxisYDownPoint.localPosition, Color.green);
            Debug.DrawLine(_GunLocalAxisXRightPoint.localPosition, _GunLocalAxisXLeftPoint.localPosition, Color.red);

            Debug.DrawLine(_GunLocalGizmoZeroPoint.localPosition, _GunLocalGizmoTargetMarker.localPosition, Color.red);
        }
    }

    // обновление координат позиций осей локальной системы координат башни танка
    private void UpdateTowerLocalGizmoScale()
    {
        Vector3 pos = _TowerLocalAxisZFrontPoint.localPosition;

        if (pos.z != _TowerLocalGizmoRayScaler)
        {
            pos.z = _TowerLocalGizmoRayScaler; _TowerLocalAxisZFrontPoint.localPosition = pos;

            pos = _TowerLocalAxisZRearPoint.localPosition;
            pos.z = -_TowerLocalGizmoRayScaler; _TowerLocalAxisZRearPoint.localPosition = pos;

            pos = _TowerLocalAxisYTopPoint.localPosition;
            pos.y = _TowerLocalGizmoRayScaler; _TowerLocalAxisYTopPoint.localPosition = pos;

            pos = _TowerLocalAxisYDownPoint.localPosition;
            pos.y = -_TowerLocalGizmoRayScaler; _TowerLocalAxisYDownPoint.localPosition = pos;

            pos = _TowerLocalAxisXRightPoint.localPosition;
            pos.x = _TowerLocalGizmoRayScaler; _TowerLocalAxisXRightPoint.localPosition = pos;

            pos = _TowerLocalAxisXLeftPoint.localPosition;
            pos.x = -_TowerLocalGizmoRayScaler; _TowerLocalAxisXLeftPoint.localPosition = pos;
        }
    }
    // обновление координат маркера цели в системе координат башни танка
    private void UpdateTowerLocalTargetMarker()
    {
        if (_TargetPositionPoint != null)
        {
            if (_TowerLocalGizmoTargetMarker.position != _TargetPositionPoint.position)
            {
                _TowerLocalGizmoTargetMarker.position = _TargetPositionPoint.position;
            }
        }
        else
        {
            _TowerLocalGizmoTargetMarker.localPosition = Vector3.zero;
        }
    }
    // отрисовка лучей локальной системы координат орудия танка
    private void DrawTowerLocalGizmo()
    {
        if (_ShowGunLocalGizmoRays)
        {
            Debug.DrawLine(_TowerLocalAxisZFrontPoint.position, _TowerLocalAxisZRearPoint.position, Color.blue);
            Debug.DrawLine(_TowerLocalAxisYTopPoint.position, _TowerLocalAxisYDownPoint.position, Color.green);
            Debug.DrawLine(_TowerLocalAxisXRightPoint.position, _TowerLocalAxisXLeftPoint.position, Color.red);
        }
    }
    // проекция локальных координат орудия танка на глобальную ось
    private void DrawProjectTowerLocalToGlobalGizmo()
    {
        if (_ShowTowerLocalRaysInGlobal)
        {
            Debug.DrawLine(_TowerLocalAxisZFrontPoint.localPosition, _TowerLocalAxisZRearPoint.localPosition, Color.blue);
            Debug.DrawLine(_TowerLocalAxisYTopPoint.localPosition, _TowerLocalAxisYDownPoint.localPosition, Color.green);
            Debug.DrawLine(_TowerLocalAxisXRightPoint.localPosition, _TowerLocalAxisXLeftPoint.localPosition, Color.red);

            Debug.DrawLine(_TowerLocalGizmoZeroPoint.localPosition, _TowerLocalGizmoTargetMarker.localPosition, Color.red);
        }
    }

    // базовая функция обновления и отрисовки локальных систем координат орудия и башни танка
    private void UpdateLocalGizmosSystem()
    {
        switch (_systemMode)
        {
            case SystemMode.Artillery:
                UpdateGunLocalGizmoScale();                     // обновляет размер осей локальной системы координат пушки
                UpdateGunLocalTargetMarker();                   // обновляет позицию маркера цели в системе координат пушки
                DrawGunLocalGizmo();                            // рисует оси локальной системы координат пушки
                DrawProjectGunLocalToGlobalGizmo();             // рисует оси и цель в глобальной системе координат
                break;

            case SystemMode.Tank:
                UpdateGunLocalGizmoScale();                     // обновляет размер осей локальной системы координат пушки
                UpdateGunLocalTargetMarker();                   // обновляет позицию маркера цели в системе координат пушки
                DrawGunLocalGizmo();                            // рисует оси локальной системы координат пушки
                DrawProjectGunLocalToGlobalGizmo();             // рисует оси и цель в глобальной системе координат

                UpdateTowerLocalGizmoScale();                   // обновляет размер осей локальной системы координат башни
                UpdateTowerLocalTargetMarker();                 // обновляет позицию маркера цели в системе координат башни
                DrawTowerLocalGizmo();                          // рисует оси локальной системы координат башни
                DrawProjectTowerLocalToGlobalGizmo();           // рисует оси и цель в глобальной системе координат
                break;
        }
    }

 
    // обновление состояния крена танка
    void UpdateTankPositionRoll()
    {
        // ВНИМАНИЕ! Поворт будет отображаться в диапазоне от 0 до 360 градусов
        // как бы его не крутило в трансформе в редакторе, таким образом:
        // диапазон влево 0-180 по оси Z, диапазон вправо 180-360 по оси Z
        // диапазон вперед 0-180 по оси X, диапазон назад 180-360 по оси X

        // проверяем комплексный крен вперед
        if (_CurrentTankTransform.localRotation.eulerAngles.x > 0.05f && _CurrentTankTransform.localRotation.eulerAngles.x < 179.95f)
        {
            if (_CurrentTankTransform.localRotation.eulerAngles.z > 0.05f && _CurrentTankTransform.localRotation.eulerAngles.z < 179.95f)
            {
                _TankPositionRoll = PositionRoll.ForwardLeft;   // имеем крен вперед-влево
            }

            else if (_CurrentTankTransform.localRotation.eulerAngles.z < 359.95f && _CurrentTankTransform.localRotation.eulerAngles.z > 180.05f)
            {
                _TankPositionRoll = PositionRoll.ForwardRight;  // имеем крен вперед-вправо
            }

            else 
            {
                _TankPositionRoll = PositionRoll.ForwardRoll;   // имеем крен вперед
            }
        }

        // проверяем комплексный крен назад
        else if (_CurrentTankTransform.localRotation.eulerAngles.x > 180.05f && _CurrentTankTransform.localRotation.eulerAngles.x < 359.95f)
        {
            if (_CurrentTankTransform.localRotation.eulerAngles.z > 0.05f && _CurrentTankTransform.localRotation.eulerAngles.z < 179.95f)
            {
                _TankPositionRoll = PositionRoll.RearLeft;      // имеем крен назад-влево
            }

            else if (_CurrentTankTransform.localRotation.eulerAngles.z < 359.95f && _CurrentTankTransform.localRotation.eulerAngles.z > 180.05f)
            {
                _TankPositionRoll = PositionRoll.RearRight;     // имеем крен назад-вправо
            }

            else
            {
                _TankPositionRoll = PositionRoll.RearRoll;      // имеем крен назад
            }
        }

        // проверяем боковые крены без фронтальных
        else
        {
            if (_CurrentTankTransform.localRotation.eulerAngles.z > 0.05f && _CurrentTankTransform.localRotation.eulerAngles.z < 179.95f)
            {
                _TankPositionRoll = PositionRoll.LeftRoll;      // имеем крен влево
            }

            else if (_CurrentTankTransform.localRotation.eulerAngles.z < 359.95f && _CurrentTankTransform.localRotation.eulerAngles.z > 180.05f)
            {
                _TankPositionRoll = PositionRoll.RightRoll;     // имеем крен вправо
            }

            else
            {
                _TankPositionRoll = PositionRoll.None;          // крен отсутствует
            }
        }

        CURRENT_TANK_ROLL = _TankPositionRoll;

        // TODO. Систему определения крена необходимо перенести в навигационный блок, там он будет уместней
        // В данном классе определение крена необходимо для корректного определения угла возвышения орудия и поворота башни
    }
    // обновление дистанции до цели с помощью локальных координат маркера цели и начала системы координат орудия
    void UpdateToTargetDistance()
    {
        if (_GunLocalGizmoTargetMarker.localPosition != Vector3.zero)
        {
            _Distance = Vector3.Distance(_GunLocalGizmoTargetMarker.localPosition, _GunLocalGizmoZeroPoint.localPosition);
        }
        else
        {
            _Distance = 0;
        }
    }

    void FixedUpdate()
    {
        // скрипт будет работать только после прохождения теста
        if (_SelfCheck)
        {
            UpdateTankPositionRoll();            // обновляем информацию по крену танка
            UpdateLocalGizmosSystem();           // обновление размеров и отрисовка локальных осей орудия и башни

            UpdateTargetOnMouseRayCast();        // обновление цели с помощью рейкаста

            if (_HaveTarget)
            {
                UpdateToTargetDistance();        // обновление дистанции до цели

                switch (_systemMode)
                {
                    case SystemMode.Tank:
                        TowerAtTargetRotation();        // разворачиваем башню на цель
                        break;
                }
            }

            UpdateGunTransform();                   // возвышаем ствол на нужную величину
            Attack();                               // производим выстрел, если нажата кнопка пробела
        }
    }

    private void TowerAtTargetRotation()
    {
        if (_systemMode == SystemMode.Tank)
        {
            Transform target = _TankFrontPoint;
            if (_TargetPositionPoint != null) target = _TargetPositionPoint;

            // Берем разницу векторов позиций башни и цели
            Vector3 towerTargetDirection = (target.position - _TowerRotationAxisPoint.position).normalized;
            // Пересчитываем скорось относительно времени кадра
            float towerSingleStep = _TowerRotationSpeed * Time.deltaTime;
            // Расчитываем доворот башни по встроенному методу
            Vector3 towerAxixRotation =
                Vector3.RotateTowards(_TowerRotationAxisPoint.forward, towerTargetDirection, towerSingleStep, 0.0f);
            // Блокируем ось y, чтобы башня не крутилась чёрти как
            towerAxixRotation.y = 0;
            // Производим вращение башни
            _TowerRotationAxisPoint.rotation = Quaternion.LookRotation(towerAxixRotation);
        }
    }

    // метод смены возвышения дула ствола
    private void UpdateGunTransform()
    {
        // проверяем есть ли назначенная цель
        if (_HaveTarget)
        {
            // производим расчёт угла требуемого возвышения ствола орудия
            CalculateGunLevelAngle(out float angle);
            // задаем параметры врашения вектору поворота ствола
            GunLevelingOnAngle(angle);
        }
        else
        {
            GunLevelingOnAngle(-_GunDefaultAngle);
        }

        // применяем обычный поворот эейлера по полученному вектору
        Quaternion gunRotation = Quaternion.Euler(_GunRotation * Time.deltaTime);
        // производим изменение угла возвышения орудия
        _GunLevelingAxisPoint.localRotation = gunRotation * _GunLevelingAxisPoint.localRotation;
    }

    // метод определения требуемого угла возвышения башни
    private void CalculateGunLevelAngle(out float angle)
    {
        // для определения угла возвышения воспользуемся соотношением углов в прямоугльном треугольнике АВС,
        // где А - точка нахождения танка, В - точка нахождения цели, С - точка перпендикулярная к А и В, обладающая углом в 90 градусов
        // sin a = a / c, где синус альфа - угол возвышения башни танка, а - катет между ВС, с - гиппотенуза АВ.

        // для большей читаемости создадим переменные с короткими названиями вершин и сторон треугольника ABC
        Vector3 A_point = _GunLocalGizmoZeroPoint.localPosition;             // "A" она же точка начала координат
        Vector3 B_point = _GunLocalGizmoTargetMarker.localPosition;          // "B" она же точка положения маркера цели
        Vector3 C_point = B_point; C_point.y = A_point.y;                    // "C" проекция точки "B" на горизонтальную плоскость
        
        bool TragetUpper = A_point.y < B_point.y;        // флаг возвышения цели относительно начала системы координат орудия танка

        float hypotAB = _Distance;                       // гиппотенуза треугольника по факту является дистанцией до цели
        float katetBC = Vector3.Distance(B_point, C_point);                  // катет расчитывается также как гиппотенуза

        if (_ShowDistanceTriangle)
        {
            Debug.DrawLine(A_point, B_point, Color.magenta); // луч от башни до цели
            Debug.DrawLine(B_point, C_point, Color.magenta); // перпендикуляр к горизонтальной плоскости
            Debug.DrawLine(C_point, A_point, Color.magenta);
        }

        angle = katetBC / hypotAB;                                                       // производим деление катета на гиппотенузу
        angle = Mathf.Asin(angle);                                                       // переврдим значение в арксинус и радианы
        angle = angle * 180 / Mathf.PI;                                                  // преобразуем из радиан в градусы

        if (TragetUpper) angle = -angle;                                                 // обращаем знак угла, при возвышении цели
    }

    // расчёт вектора поворота орудия башни
    private void GunLevelingOnAngle(float angle)
    {
        // смотрим текущий угол относительного возвышения ствола
        float currentAngle = _GunLevelingAxisPoint.localRotation.eulerAngles.x;

        // если получили угол меньше нуля - значит надо поднимать ствол - работаем в верхней полусфере
        if (angle < 0)
        {
            // есди текущий угол меньше десятка - скорее всего он до этого смотрел "вниз"
            if (currentAngle < 30)
            {
                _GunRotation.x = -_GunLevelingSpeed * 10;      // подымаем орудие
            }
            else
            {
                // если текущее возвышение равно нулю или текущий относительный угол минус требуемый больше 0,01
                if (currentAngle - (360 + angle) > 0.05f && (360 - currentAngle) <= _GunUpAngleLimmit)
                {
                    _GunRotation.x = -_GunLevelingSpeed * 10;   // подымаем орудие
                }
                // требуемое возвышение больше текущего - значит надо опустить орудие - цель ниже текущего направления
                else if ((360 + angle) - currentAngle > 0.05f && currentAngle != 0)
                {
                    _GunRotation.x = _GunLevelingSpeed * 10;    // опускаем орудие
                }
                else
                {
                    _GunRotation.x = 0;                         // останавливаем вращение
                }
            }
        }
        // если получен угол больше нуля - значит цель ниже и ствол надо опускать - работаем в нижней полусфере
        else if (angle > 0)
        {
            // если предыдущая цель была выше оси танка
            if (currentAngle > 330)
            {
                // опускаем орудие, чтобы перейти в нижнюю полусферу обзора
                _GunRotation.x = _GunLevelingSpeed * 10;
            }
            else
            {
                // если требуемый угол минус текущий больше 0,01
                if (angle - currentAngle > 0.05f && currentAngle <= _GunDownAngleLimmit)
                {
                    _GunRotation.x = _GunLevelingSpeed * 10;    // опускаем орудие
                }
                // если текущий угол больше требуемого - значит цель чуть выше но всё еще в нижней полусфере
                else if (currentAngle - angle > 0.05f && currentAngle != 0)
                {
                    _GunRotation.x = -_GunLevelingSpeed * 10;   // подымаем орудие
                }
                else
                {
                    _GunRotation.x = 0;                         // останавливаем вращение
                }
            }
        }
        else
        {
            _GunRotation.x = 0;                                 // останавливаем вращение
        }
    }

    // выбор цели рейкастом
    private void UpdateTargetOnMouseRayCast()
    {
        // при нажатии на ЛКМ
        if (Input.GetMouseButtonUp(0))
        {
            // пускаем рейкаст из камеры по положению мышки
            Ray ray = _TankRayCastCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // если рейкаст во что-то попал
            if (Physics.Raycast(ray, out hit))
            {
                _TargetPositionPoint = hit.transform;     // задаём новую цель
                if (!_HaveTarget) _HaveTarget = true;     // обновляем флаг отслеживания цели
            }
            else
            {
                // если рейкаст был в пустоту, то сбрасывавем цель и заканчиваем отслеживание
                if (_HaveTarget) _HaveTarget = false;
                _TargetPositionPoint = null;
            }
        }
    }
    // выстрел из пушки
    private void Attack()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            // берем позицию крайней точки ствола
            Vector3 start_point = _GunEdgePoint.position;
            // берем величину поворота крайней точки ствола умноженную на первичный поворот элемента
            Quaternion start_rotation = _GunEdgePoint.rotation * Quaternion.Euler(new Vector3(90,0,0));

            // создаём снаряд по полученным координатам и углу поворота
            GameObject bullet = Instantiate(_TankGunBullet, start_point, start_rotation ) as GameObject;
            // берем физическое тело пули
            Rigidbody r_bullet = bullet.GetComponent<Rigidbody>();

            // сила воздействующая на снаряд считается от дистанции до объекта
            r_bullet.AddForce((_GunEdgePoint.position - _GunLevelingAxisPoint.position).normalized * _GunAttackPower/* * _Distance*/, ForceMode.Impulse);

            Rigidbody tank = GetComponentInChildren<Rigidbody>();
            tank.AddForce((_GunLevelingAxisPoint.position - _GunEdgePoint.position).normalized * _GunAttackPower/* * _Distance*/, ForceMode.Impulse);

            Destroy(bullet, _BulletLifeTime);
        }
    }
}