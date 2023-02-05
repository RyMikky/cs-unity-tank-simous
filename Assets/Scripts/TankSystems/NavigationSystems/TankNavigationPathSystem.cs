using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TankNavigationPathSystem : MonoBehaviour
{

    public bool _Enable = false;
    public GameObject _targetObject;

    [Header("Отладочная информация построения пути")]
    [Tooltip("Количество ребер построенного пути")]
    public bool NAVMESH_PATH_RENDERING = false;
    [Tooltip("Количество ребер построенного пути")]
    public int NAVMESH_CORNERS_COUNT = 0;

    private NavMeshPath _navMeshPath;                            // путь от встроенной системы навигации Unity

    // включение скрипта
    public void SetSubSystemEnable(bool enable) { _Enable = enable; }
    // задать текущую конечную цель перемещения
    public void SetTargetObject(GameObject target) { _targetObject = target; }
    // возвращает количество вершин пути, они же маркеры пути
    public int GetTargetMarkersCount() { return _navMeshPath.corners.Length; }
    // возвращает первый после текущей позиции маркер
    public Vector3 GetTargetMarkerOne() 
    { 
        if (_navMeshPath.corners.Length > 1)
        {
            return _navMeshPath.corners[1];
        }
        else
        {
            return transform.position;
        }
    }
    // возвращает второй после текущей позиции маркер, для эвристики пути
    public Vector3 GetTargetMarkerTwo()
    {
        if (_navMeshPath.corners.Length > 2)
        {
            return _navMeshPath.corners[2];
        }
        else
        {
            // если всего две вершины или меньше, то маркер один и два будут одинаковы
            return GetTargetMarkerOne();
        }
    }

    private void Awake()
    {
        _navMeshPath = new NavMeshPath();
    }

    private void FixedUpdate()
    {
        UpdateNavMeshUnitySystem();                              // построение пути в NavMeshUnity
        UpdateDebugInformation();                                // обновление отладочной информации
        UpdatePathRendering();                                   // отрисовка построенного пути
    }

    // построение пути в NavMeshUnity
    void UpdateNavMeshUnitySystem()
    {
        if (_Enable)
        {
            if (_targetObject != null)
            {
                NavMesh.CalculatePath(transform.position, _targetObject.transform.position, NavMesh.AllAreas, _navMeshPath);
            }
        }
    }
    // обновление отладочной информации
    void UpdateDebugInformation()
    {
        NAVMESH_CORNERS_COUNT = _navMeshPath.corners.Length;
    }
    // отрисовка построенного пути
    void UpdatePathRendering()
    {
        if (_Enable)
        {
            if (NAVMESH_PATH_RENDERING)
            {
                for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
                    Debug.DrawLine(_navMeshPath.corners[i], _navMeshPath.corners[i + 1], Color.magenta);
            }
        }
    }
}