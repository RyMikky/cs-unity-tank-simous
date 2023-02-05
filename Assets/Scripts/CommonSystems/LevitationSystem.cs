using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevitationSystem : MonoBehaviour
{
    public GameObject _mainObject;                                        // базовый левитирующий объект
    public Rigidbody _mainBody;                                           // базовое левитирующее тело

    [Range(0, 10)]
    [Tooltip("Время в движении в любом направлении")]
    public int _movingTime = 2;                     // время в движении в любом направлении

    [Range(-2f, 2f)]
    [Tooltip("Величина импульса по вертикальной оси.\nСкалируется от массы")]
    public float _levitationVelocity = 0.5f;        // величина импульса по вертикальной оси

    [Range(0, 10)]
    [Tooltip("Угловая скорость поворота.\nЕсли включен флаг случайного вращения является граничным значением, также может быть равно нулю")]
    public int _rotationVelocity = 5;               // угловая скорость поворота

    public bool _levitationEnable = false;          // флаг включения левитации
    public bool _rotationEnable = false;            // флаг включения вращения

    public bool _randomDirection = false;           // флаг рандомного направления при старте
    public bool _randomRotation = false;            // флаг рандомного вращения при старте

    private int _impulseTime = 0;                   // ожидаемое время придания импульса
    private bool _downDirection = false;            // флаг направления вниз
    private bool _firstMove = true;                 // флаг первого движения

    private Vector3 _objecktRotation = Vector3.zero;                      // величина поворота
    private System.Random _rnd = new System.Random();                     // базовый рандомайзер

    private void Awake()
    {
        DirectionCheck();                           // вызываем проверку начального движения
        RotationCheck();                            // определяем величину поворота
    }

    private void Update()
    {
        ObjectRotation();                           // производим ламинарное вращение

        if (!_levitationEnable)
            LevitationStop();

        if (_firstMove)
        {
            DirectionCheck();                       // перезапускаем первое движение
        }
        else
        {
            // берем текущую секунду
            int _motion_second = ((int)Time.timeAsDouble % 60) % 60;

            // если текущая секунда равна ожидаемой
            if (_motion_second == _impulseTime)
            {
                // делаем обновление движения
                LevitationMove();  // придаем обратный импульс

                // обновляем время следующего придания импульса
                _impulseTime += _movingTime;
                // каждый раз когда переходим за границу 60 - просто вычитаем 60
                if (_impulseTime > 59) _impulseTime -= 60;
            }
        }
    }

    // стартовое движение
    private void StartLevitation(float y)
    {
        if (_mainBody != null && _levitationEnable && _firstMove)
        {
            // при старте просто отправляем вверх
            _mainBody.AddForce(new Vector3(0, y * _mainBody.mass, 0), ForceMode.Impulse);
            // плюсуем текущее время
            _impulseTime += _movingTime;
            // закрываем флаг первого движения
            _firstMove = false;
        }
    }

    // обычное обновление движения
    private void LevitationMove()
    {
        if (_mainBody != null && _levitationEnable)
        {
            //// если стартовый импульс отрицательный и текущая вертикальная скорость больше нуля
            //if (_downDirection && _mainBody.velocity.y > 0
            //// или стартовый импульс положительный, а текущая вертикальная скорость ниже нуля
            //|| !_downDirection && _mainBody.velocity.y < 0)
            //{
            //    // присваиваем новый вектор с удвоенной положительной вертикальной скоростью
            //    _mainBody.AddForce(new Vector3(0, (_levitationVelocity * 2 * _mainBody.mass), 0), ForceMode.Impulse);
            //}
            //else
            //{
            //    // присваиваем новый вектор с удвоенной отрицательной вертикальной скоростью
            //    _mainBody.AddForce(new Vector3(0, -(_levitationVelocity * 2 * _mainBody.mass), 0), ForceMode.Impulse);
            //}

            // если стартовый импульс отрицательный и текущая вертикальная скорость больше нуля
            if (_downDirection && _mainBody.velocity.y < 0
            // или стартовый импульс положительный, а текущая вертикальная скорость ниже нуля
            || !_downDirection && _mainBody.velocity.y > 0)
            {
                LevitationStop();
                // присваиваем новый вектор с удвоенной положительной вертикальной скоростью
                _mainBody.AddForce(new Vector3(0, -(_levitationVelocity), 0), ForceMode.VelocityChange);
            }
            else
            {
                LevitationStop();
                // присваиваем новый вектор с удвоенной отрицательной вертикальной скоростью
                _mainBody.AddForce(new Vector3(0, (_levitationVelocity), 0), ForceMode.VelocityChange);
            }
        }
    }

    // проверка направления начального движения
    private void DirectionCheck()
    {
        // если задают отрицательное значение, значит импульс толкает вниз
        if (_levitationVelocity < 0) _downDirection = true;

        // если включён рандомный старт
        if (_randomDirection)
        {
            if (_rnd.Next(0, 30) > 15)
            {
                StartLevitation(_levitationVelocity);         // стартуем в прямом направлении
            }
            else
            {
                StartLevitation(-(_levitationVelocity));      // стартуем в обратном направлении
            }
        }
        else
        {
            StartLevitation(_levitationVelocity);             // стартуем в прямом направлении
        }
    }
    // назначение поворота левитирующей скалы
    private void RotationCheck()
    {
        if (_randomRotation)
        {
            // даём случайную величину повотора
            _objecktRotation.y = _rnd.Next(-_rotationVelocity, _rotationVelocity);
        }
        else
        {
            _objecktRotation.y = _rotationVelocity;
        }
    }

    private void ObjectRotation()
    {
        if (_rotationEnable)
        {
            Quaternion deltaRotation = Quaternion.Euler(_objecktRotation * Time.deltaTime);
            // тут ВАЖЕН порядок - необходимо довернуть дельта на текущий поворот, а не наоборот!
            transform.rotation = deltaRotation * transform.rotation;
        }
    }

    private void LevitationStop()
    {
        if (_mainBody != null && _mainBody.velocity.y != 0)
        {
            _mainBody.velocity = Vector3.zero;
        }
    }
}