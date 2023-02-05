using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class LevitationRockEngine : MonoBehaviour
{
    public enum SelectedBody
    {
        none, first, second, third, fourth, fifth, sixth
    }

    public SelectedBody _usedBody;                                        // выбор используемого камня
    private SelectedBody _archiveBody;                                    // сохраненный выбор камня

    public List<GameObject> _rockObjects = new List<GameObject>();        // список используемых объектов левитирующих камней
    public List<Rigidbody> _rockBodies = new List<Rigidbody>();           // список используемых тел левитирующих камней

    public Rigidbody _mainBody;                                           // базовое левитирующее тело

    [Range(0, 10)]
    [Tooltip("Время в движении в любом направлении")]
    public int _movingTime = 2;                     // время в движении в любом направлении

    [Range(-200f, 200f)]
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

    private GameObject _rockObject = null;                                // выбранный объект
    private Rigidbody _rockBody = null;                                   // выбранное тело
    private Vector3 _objecktRotation = Vector3.zero;                      // величина поворота
    private System.Random _rnd = new System.Random();                     // базовый рандомайзер

    private void Awake()
    {
        RockSelection();                            // определяем тип объекта на старте
        DirectionCheck();                           // вызываем проверку начального движения
        RotationCheck();                            // определяем величину поворота
    }

    private void Update()
    {
        ObjectRotation();                           // производим ламинарное вращение
        RockSelection();                            // переключение объекта если требуется

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
        if (_usedBody != SelectedBody.none && _levitationEnable && _firstMove)
        {
            // при старте просто отправляем вверх
            _rockBody.AddForce(new Vector3(0, y * _rockBody.mass, 0), ForceMode.Impulse);
            // плюсуем текущее время
            _impulseTime += _movingTime;
            // закрываем флаг первого движения
            _firstMove = false;
        }
    }

    // обычное обновление движения
    private void LevitationMove()
    {
        if (_usedBody != SelectedBody.none && _levitationEnable)
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
            if (_downDirection && _rockBody.velocity.y < 0
            // или стартовый импульс положительный, а текущая вертикальная скорость ниже нуля
            || !_downDirection && _rockBody.velocity.y > 0)
            {
                // присваиваем новый вектор с удвоенной положительной вертикальной скоростью
                _rockBody.AddForce(new Vector3(0, -(_levitationVelocity * 2), 0), ForceMode.VelocityChange);
            }
            else
            {
                // присваиваем новый вектор с удвоенной отрицательной вертикальной скоростью
                _rockBody.AddForce(new Vector3(0, (_levitationVelocity * 2), 0), ForceMode.VelocityChange);
            }
        }
    }

    // активация выбранного типа скалы
    private void RockSelection()
    {
        if (_archiveBody != _usedBody)
        {
            DeactivateRock();                       // отключаем раннее активированные элементы
            _archiveBody = _usedBody;               // сохраняем текущее состояние
            int usedIndex = 0;                      // индекс выбранного типа камня

            switch (_usedBody)
            {
                case SelectedBody.none:
                    return;
                case SelectedBody.first:
                    break;
                case SelectedBody.second:
                    usedIndex = 1;
                    break;
                case SelectedBody.third:
                    usedIndex = 2;
                    break;
                case SelectedBody.fourth:
                    usedIndex = 3;
                    break;
                case SelectedBody.fifth:
                    usedIndex = 4;
                    break;
                case SelectedBody.sixth:
                    usedIndex = 5;
                    break;
            }

            _rockObject = _rockObjects[usedIndex];  // записываем выбранный объект
            _rockBody = _rockBodies[usedIndex];     // записываем выбранное тело

            _rockObject.SetActive(true);            // активируем выбранный объект
            _firstMove = true;                      // активируем необходимость первого движения
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

    // отключение текущего назначенного объекта
    private void DeactivateRock()
    {
        if (_rockObject != null)
        {
            _rockObject.transform.position = Vector3.zero;
            _rockObject.SetActive(false);
            _rockObject = null;
        }

        if (_rockBody != null) _rockBody = null;
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
        if (_rockObject != null && _mainBody.velocity.y != 0)
        {
            _mainBody.velocity = Vector3.zero;
        }
    }
}