using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCyclicMoving : MonoBehaviour
{
    public bool _Enable = false;

    public enum TargetChange
    {
        Linear, Random
    }

    public TargetChange Mode = TargetChange.Linear;

    public GameObject _Tank;                                       // танк который будет участвовать в тестировании
    public GameObject[] _Targets;                                  // цели танка для поворота на месте


    private TankAutoPilotSystem _autoPilot;                        // танковый автопилот
    private int _targetArraySize = 0;                              // количество записанных в массив целей

    private void Awake()
    {
        if (_Tank != null)
            _autoPilot = _Tank.GetComponent<TankAutoPilotSystem>();

        if (_Targets.Length != 0 && _autoPilot != null && _Enable)
        {
            _autoPilot.SetTargetObject(_Targets[0]);
            _targetArraySize = _Targets.Length;
            ConstructTargetsLine();
        }
    }

    void ConstructLinearMode()
    {
        for (int i = 0; i != _targetArraySize; i++)
        {
            if (i < _targetArraySize - 1)
            {
                _Targets[i].GetComponent<TargetBoxSystem>().SetNextTarget(_Targets[i + 1]);
            }
            else
            {
                _Targets[i].GetComponent<TargetBoxSystem>().SetNextTarget(_Targets[0]);
            }
        }
    }

    void ConstructRandomMode()
    {

    }

    void ConstructTargetsLine()
    {
        switch (Mode)
        {
            case TargetChange.Linear:
                ConstructLinearMode();
                break;
            case TargetChange.Random:
                ConstructRandomMode();
                break;
        }
    }
}