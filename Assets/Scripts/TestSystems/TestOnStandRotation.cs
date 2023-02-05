using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOnStandRotation : MonoBehaviour
{
    // базовый скрипт автоматической смены цели

    public bool _Enable = false;

    public enum TargetChange
    {
        Timer, Random, Extreme
    }

    public TargetChange Mode = TargetChange.Timer;

    public GameObject _Tank;                                       // танк который будет участвовать в тестировании
    public GameObject[] _Targets;                                  // цели танка для поворота на месте
    [Range(2, 30)]
    public int _targetChangeTime = 10;                             // время изменения цели
    public int _currentTime = 0;                                   // счётчик времени
    public int _currentTargetNumber = 0;                           // номер текущей вызванной цели

    private TankAutoPilotSystem _autoPilot;                        // танковый автопилот
    private int _activationTime = 0;                               // время актиавции смены цели
    private int _targetArraySize = 0;                              // количество записанных в массив целей

    private bool _timeIsChanged = true;                            // флаг необходимости нового опорного времени
    private bool _arrayNeedNewIndex = true;                        // флаг необходимости нового индекса
    private bool _tankNeedNewTarget = true;                        // флаг необходимости новой цели

    private void Awake()
    {
        if (_Tank != null) 
            _autoPilot = _Tank.GetComponent<TankAutoPilotSystem>();

        if (_Targets.Length != 0) 
            _targetArraySize = _Targets.Length;
    }

    private void FixedUpdate()
    {
        SystemTimer();
        TargetSelect();
        OnStandRotation();
        RandomChangeTime();
    }
    void SystemTimer()
    {
        if (_Enable)
        {
            // берем текущую секунду
            _currentTime = ((int)Time.timeAsDouble % 60) % 60;
            // записываем активационную секунду
            _activationTime = _currentTime % _targetChangeTime;
        }
    }

    void RandomChangeTime()
    {
        if (_Enable && Mode == TargetChange.Random)
        {
            if (_timeIsChanged && (_currentTime % 15 == 0))
            {
                _targetChangeTime = Random.Range(2, 11);
                _timeIsChanged = false;
            }
            else if (_currentTime % 15 == 1)
            {
                _timeIsChanged = true;
            }
        }
    }

    void TargetSelect()
    {
        if (_Enable)
        {
            if (_activationTime == 0 && _arrayNeedNewIndex)
            {
                _currentTargetNumber = Random.Range(0, _targetArraySize);
                _arrayNeedNewIndex = false;
            }
            else if (_activationTime == 1)
            {
                _arrayNeedNewIndex = true;
            }
        }
    }

    void OnStandRotation()
    {
        if (_Enable)
        {
            if (_activationTime == 0 && _tankNeedNewTarget)
            {
                _autoPilot.SetTargetObject(_Targets[_currentTargetNumber]);
                _tankNeedNewTarget = false;
            }
            else if (_activationTime == 1)
            {
                _tankNeedNewTarget = true;
            }
        }
    }
}