using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TankTruckMovingSystem_B : MonoBehaviour
{
    public enum InputType
    {
        None, Player, AutoPilot
    }

    [Tooltip("����� ����� ��� ������� �����")]
    public InputType _inputType = InputType.Player;

    [Tooltip("���� ��������� �������")]
    public bool _Enable = false;                                       // ��������� �������
    [Tooltip("���� ��������� �������������. ������ ����� ��������� � ����� ��������� � ������ " +
        "����� ������������� � ����������� �� ���������� �����, ������� ������� � ������������")]
    public bool _useDufferencial = false;                              // ��������� �������������

    [Header("���� ���������� ������������ � ������������")]
    [Tooltip("���������� ������� ����� ������ �����")]
    public WheelCollider[] _leftWheelsColliders;                       // ���������� ������� ����� ������ �����
    [Tooltip("���������� ������� ����� ������� �����")]
    public WheelCollider[] _rightWheelsColliders;                      // ���������� ������� ����� ������� �����

    [Tooltip("���������� ������� ����� ������ �����")]
    public Transform[] _leftWheelsTransforms;                          // ���������� ������� ����� ������ �����
    [Tooltip("���������� ������� ����� ������� �����")]
    public Transform[] _rightWheelsTransforms;                         // ���������� ������� ����� ������� �����

    [Tooltip("���������� ��������� ����� ������ �����")]
    public Transform[] _leftDependenceWheelsTransforms;                // ���������� ��������� ����� ������ �����
    [Tooltip("���������� ��������� ����� ������� �����")]
    public Transform[] _rightDependenceWheelsTransforms;               // ���������� ��������� ����� ������� �����

    [Tooltip("���������� ������ �������� ������ �����")]
    public Transform[] _leftTruckBones;                                // ���������� ������ ������ �����
    [Tooltip("���������� ������ �������� ������� �����")]
    public Transform[] _rightTruckBones;                               // ���������� ������ ������� �����

    [Tooltip("������� ������ ����� ��������")]
    public GameObject _leftTankTruck;                                  // ������� ������ ����� ��������
    [Tooltip("������� ������ ������ ��������")]
    public GameObject _rightTankTruck;                                 // ������� ������ ������ ��������

    [Header("������� �������������� ��������� � ��������")]
    [Tooltip("�������� ���������")]
    public float _engineForce = 3400f;                // �������� ���������
    public void SetEngineForce(float force) { _engineForce = force; }
    public float GetEngineForce() { return _engineForce; }
    [Tooltip("�������� ����������")]
    public float _brakeForce = 5200f;                 // ���� ����������
    public void SetBreakForce(float force) { _brakeForce = force; }
    public float GetBreakForce() { return _brakeForce; }

    [Header("��������� ������������ �������� � �������� ���������")]
    [Tooltip("������� ��������� �������� ��������� �������")]
    public float _defaultSideFriction = 1f;           // ������� ��������� �������� ��������� �������
    [Tooltip("��������� �������� ��������� ������� ������������ �����������")]
    public float _completeMoveSideFriction = 0.6f;    // ��������� �������� ��������� ������� ������������ �����������

    [Tooltip("������������ �������� ��������")]
    public float _directMoveVelocityLimit = 10f;      // ������������ �������� ��������
    public void SetDirectVelocityLimit(float limit) { _directMoveVelocityLimit = limit; }
    public float GetDirectVelocityLimit() { return _directMoveVelocityLimit; }
    [Tooltip("��������� �������� ���������� ��� �������� �����")]
    public float _directMoveSideFriction = 0.9f;      // ��������� �������� ���������� ��� �������� �����

    [Tooltip("������������ �������� �������� �� �����")]
    public float _rotateOnStandVelocityLimit = 1f;    // ������������ �������� �������� �� �����
    public void SetRotateVelocityLimit(float limit) { _rotateOnStandVelocityLimit = limit; }
    public float GetRotateVelocityLimit() { return _rotateOnStandVelocityLimit; }
    [Tooltip("��������� �������� ���������� ��� ��������� �� �����")]
    public float _rotateOnStandSideFriction = 0.2f;   // ��������� �������� ���������� ��� ��������� �� �����

    [Header("��������� ���� ��������� ����� � ��������")]
    [Tooltip("��������� �������� ���� ���������� ��� ������ ���������")]
    public bool _useCustomData = false;               // ��������� �������� ���� ���������� ��� ������ ���������
    [Tooltip("����� ����� ����� � ����������� (������� Rigidbody)")]
    public float _unitCustomMass = 50000f;            // ����� ����� ����� � ����������� (������� Rigidbody)
    [Space]
    [Tooltip("���� ����������� ������� � ��������. ��� ����� ���������� � 10 ��� ������ �����")]
    public float _wheelCustomSpring = 500000f;            // ���� ����������� ������� � ��������. ��� ����� ���������� � 10 ��� ������ �����
    [Tooltip("������������ ������������� � ��������. ��� ����� ���������� ������ �����")]
    public float _wheelCustomDamper = 50000f;             // ������������ ������������� � ��������. ��� ����� ���������� ������ �����
    [Tooltip("������ ������� �������� � ������. ��� ����� ����� ������ ����� ��������")]
    public float _wheelCustomTarget = 1f;               // ������ ������� �������� � ������. ��� ����� ����� ������ ����� ��������

    [Header("����������� ��������� ���� ���������")]
    [Tooltip("����������� ���������� ��������� �� ���������")]
    public float _deltaVertical = 0.02f;              // ����������� ���������� ��������� �� ���������
    private float _defDeltaVertical;                  // ����������� ���������� ��������� �� ���������
    public void SetDeltaVertical(float deltaV) { _deltaVertical = deltaV; }
    public float GetDeltaVertical() { return _deltaVertical; }
    public void SetDeltaVerticalDefault() { _deltaVertical = _defDeltaVertical; }
    [Tooltip("����������� ���������� ��������� �� �����������")]
    public float _deltaHorizontal = 0.02f;            // ����������� ���������� ��������� �� �����������
    private float _defDeltaHorizontal = 0.02f;        // ����������� ���������� ��������� �� �����������
    public void SetDeltaHorizontal(float deltaV) { _deltaHorizontal = deltaV; }
    public float GetDeltaHorizontal() { return _deltaHorizontal; }
    public void SetDeltaHorizontalDefault() { _deltaHorizontal = _defDeltaHorizontal; }

    private float _forwardAcceleration;               // �������� �� �������
    private float _rotateAcceleration;                // �������� ��� �������������� ���
    private float _breakPowerScaler;                  // ��������� ������ ��������� ����������

    private float _currentSpeed;                      // �������� ������� ��������
    private float _angularSpeed;                      // �������� �������� �������� ������ ��� Y
    private Quaternion _leftCalculateRotation;        // �������� ��� ���� ����� ������ �����
    private Quaternion _rightCalculateRotation;       // �������� ��� ���� ����� ������� �����
    private Rigidbody _currentBody;                   // ���� ��� ������� �������� ������
    private float _suspentionDistance;                // ������ ��������
    private bool _needRotation = false;               // ���� ������������� �������� �������� �����

    private float _leftTrackTextureOffset = 0.0f;     // ������ �������� ������ �����
    private float _rightTrackTextureOffset = 0.0f;    // ������ �������� ������� �����

    [Header("���� ��������� ����������")]
    [Tooltip("���������� ������� ��������")]
    public float VELOCITY;
    public float GetDebugCurrentVelocity() { return VELOCITY; }
    [Tooltip("���������� ������� �������� ��������� �� ��� Y")]
    public float AXIS_VELOCITY;
    public float GetDebugCurrentAxisVelocity() { return AXIS_VELOCITY; }
    [Tooltip("���������� ������� ������� c�������")]
    public float ANGULAR_VELOCITY;
    [Tooltip("��������� ��������� ��������� �� ������������ ���")]
    public float FORWARD_ACCEL;
    public float GetDebugCurrentForwardAcceleration() { return FORWARD_ACCEL; }
    [Tooltip("��������� ��������� ��������� �� �������������� ���")]
    public float ROTATION_ACCEL;
    public float GetDebugCurrentRotationAcceleration() { return ROTATION_ACCEL; }
    [Tooltip("��������� �������� ���������� ������")]
    public float BREAK_ACCEL;
    public float GetDebugCurrentBreakAcceleration() { return BREAK_ACCEL; }

    [Tooltip("�������� �������� ������������� ������� �� ����� ������")]
    public float LEFT_TORQUE;
    public float GetDebugLeftWheelTorque() { return LEFT_TORQUE; }
    [Tooltip("�������� �������� ������������� ������� �� ������ ������")]
    public float RIGHT_TORQUE;
    public float GetDebugRightWheelTorque() { return RIGHT_TORQUE; }
    [Tooltip("�������� �������� ����������� ������� ������� �� ����� ������")]
    public float LEFT_BREAK_TORQUE;
    public float GetDebugLeftBreakForce() { return LEFT_BREAK_TORQUE; }
    [Tooltip("�������� �������� ����������� ������� ������� �� ������ ������")]
    public float RIGHT_BREAK_TORQUE;
    public float GetDebugRightBreakForce() { return RIGHT_BREAK_TORQUE; }
    [Tooltip("�������� �������� ��������� �������� ��������� �� ����� ������")]
    public float SIDEWAY_STIFFNESS;

    [Tooltip("����������� ���� �������� �����")]
    public float DELTA_ANGLE;

    private enum MovingType
    {
        direct, on_stand, complette, autobreak, out_of_hit
    }

    public void SetInputType(InputType type) { _inputType = type; }
    public InputType GetInputType() { return _inputType; }

    public void SetForwardAcceleration(float accel) { if (Mathf.Abs(accel) <= 1) _forwardAcceleration = accel; }
    public float GetForwardAcceleration() { return _forwardAcceleration; }

    public void SetRotateAcceleration(float rotate) { if (Mathf.Abs(rotate) <= 1) _rotateAcceleration = rotate; }
    public float GetRotateAcceleration() { return _rotateAcceleration; }

    public void SetBreakPowerScaler(float bPower) { if (Mathf.Abs(bPower) <= 1) _breakPowerScaler = bPower; }
    public float GetBreakPowerScaler() { return _breakPowerScaler; }

    private void Awake()
    {
        // ������ �������� ���������
        _currentBody = GetComponent<Rigidbody>();

        // ���������� ���������� �������� ����������� ���������� ���������
        _defDeltaVertical = _deltaVertical;
        _defDeltaHorizontal = _deltaHorizontal;

        UpdateCustomData();                        // ���������� �������� ��������� ��������� �������
    }

    void FixedUpdate()
    {
        UpdateCurrentSpeed();                      // ���������� ���������� ������� ��������
        UpdateInputsData();                        // ���������� ���������� �� ��������� ���� ���������

        CompletteDrive();                          // ����������� ������� ��������

        UpdateDebugData();                         // ���������� ��������� ����������
        UpdateWheelTransforms();                   // ��������� ��������� ����
    }

    // ----------------------------------------------- ���� �������� �������� �� ���� �������� ������� ------------------------------

    // ���������� ��������� ���������� �������
    void UpdateCustomData()
    {
        if (_useCustomData)
        {
            // ���� ���� ���������� ����, �� ����������� ��� ������ �� ���� �����
            if (_currentBody != null) _currentBody.mass = _unitCustomMass;

            if (_leftWheelsColliders.Length != 0)
            {
                // ���� ���� ���������� �����, �� ��������� �������� � ���
                for (int i = 0; i != _leftWheelsColliders.Length; ++i)
                {
                    JointSpring sp = _leftWheelsColliders[i].suspensionSpring;
                    sp.spring = _wheelCustomSpring;
                    sp.damper = _wheelCustomDamper;
                    sp.targetPosition = _wheelCustomTarget;

                    _leftWheelsColliders[i].suspensionSpring = sp;
                }
            }

            if (_rightWheelsColliders.Length != 0)
            {
                // ���� ���� ���������� �����, �� ��������� �������� � ���
                for (int i = 0; i != _rightWheelsColliders.Length; ++i)
                {
                    JointSpring sp = _rightWheelsColliders[i].suspensionSpring;
                    sp.spring = _wheelCustomSpring;
                    sp.damper = _wheelCustomDamper;
                    sp.targetPosition = _wheelCustomTarget;

                    _rightWheelsColliders[i].suspensionSpring = sp;
                }
            }
        }
    }
    // ���������� ��������� ������
    void UpdateDebugData()
    {

        VELOCITY = _currentSpeed;
        AXIS_VELOCITY = _angularSpeed;

        //ANGULAR_VELOCITY = _angularSpeed.magnitude;
        ANGULAR_VELOCITY = _angularSpeed;

        FORWARD_ACCEL = _forwardAcceleration;
        ROTATION_ACCEL = _rotateAcceleration;
        BREAK_ACCEL = _breakPowerScaler;

        LEFT_TORQUE = _leftWheelsColliders[2].motorTorque;
        RIGHT_TORQUE = _rightWheelsColliders[2].motorTorque;
        LEFT_BREAK_TORQUE = _leftWheelsColliders[2].brakeTorque;
        RIGHT_BREAK_TORQUE = _rightWheelsColliders[2].brakeTorque;
        SIDEWAY_STIFFNESS = _leftWheelsColliders[2].sidewaysFriction.stiffness;
    }
    // ��������� ������ �� ����������� � ��������� �� �������
    void UpdateInputsData()
    {
        switch (_inputType)
        {
            case InputType.Player:
                _forwardAcceleration = Input.GetAxis("Vertical");
                _rotateAcceleration = Input.GetAxis("Horizontal");
                break;
            case InputType.AutoPilot:
                break;
        }
    }
    // ��������� ������ �� ������� ������������� � ������� ���������
    void UpdateCurrentSpeed()
    {
        _angularSpeed = _currentBody.angularVelocity.magnitude;           // ��������� ������� ��������
        _currentSpeed = _currentBody.velocity.magnitude;                  // ��������� ������� ��������

        if (_angularSpeed >= 0.1f || _currentSpeed >= 0.1f)             // ��� ���������� �������� ������������� ������
        {
            // ����������� ������� ����� �����
            _needRotation = true;
        }
        else
        {
            // �������� ������� ����� �����
            _needRotation = false;
        }
    }
    // ����� ���������� ��������� ����� �����
    void UpdateWheelTransforms()
    {
        // ����������� ������� ���������� ���������� ����� � ������ ��������, � ����� ������� ������ �� ��������
        UpdateWheelTransform(ref _leftTrackTextureOffset, ref _leftCalculateRotation, ref _leftWheelsColliders, ref _leftWheelsTransforms, ref _leftTruckBones);
        UpdateWheelTransform(ref _rightTrackTextureOffset, ref _rightCalculateRotation, ref _rightWheelsColliders, ref _rightWheelsTransforms, ref _rightTruckBones);

        if (_needRotation)
        {
            // ��������� �������� ����� ������ �����
            UpdateWheelsRotation(ref _leftCalculateRotation, ref _leftWheelsTransforms);
            UpdateWheelsRotation(ref _leftCalculateRotation, ref _leftDependenceWheelsTransforms);
            UpdateTruckTextureOffset(ref _leftTrackTextureOffset, ref _leftTankTruck);

            // ��������� �������� ����� ������� �����
            UpdateWheelsRotation(ref _rightCalculateRotation, ref _rightWheelsTransforms);
            UpdateWheelsRotation(ref _rightCalculateRotation, ref _rightDependenceWheelsTransforms);
            UpdateTruckTextureOffset(ref _rightTrackTextureOffset, ref _rightTankTruck);
        }
    }

    // ----------------------------------------------- ���� ������� �������� ����� � ���������� ����������� -------------------------

    // �������� ������� ������ ��������, ���������� ��������� ��� �������� � ����������� �� ������ ���������� �� ���� ���������� � ������� �������� �����
    void CompletteDrive()
    {
        float totalLeftAcceleration = 0;
        float totalRightAcceleration = 0;
        float breakPowerScaler = 0;

        // ���� ��� �� ������������� ��������� �� ���������������
        if (Mathf.Abs(_forwardAcceleration) <= _deltaVertical && Mathf.Abs(_rotateAcceleration) <= _deltaHorizontal)
        {
            breakPowerScaler = 1;
            // ������ �������� ���� � ��������� ����� �� ������
            SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, 0, _defaultSideFriction, MovingType.autobreak);
            SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, 0, _defaultSideFriction, MovingType.autobreak);
        }

        // ���� ��������� ������������ ���������, �� ��������������� ��� - ������������� ��������
        else if (Mathf.Abs(_forwardAcceleration) > _deltaVertical && Mathf.Abs(_rotateAcceleration) <= _deltaHorizontal)
        {
            totalLeftAcceleration = _forwardAcceleration;
            totalRightAcceleration = _forwardAcceleration;
            breakPowerScaler = _breakPowerScaler;

            // ������� ������ ��������� ���������� �� ���, ������� ����������� ��������, ����������� ���������� � ��� ��������
            SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _directMoveVelocityLimit, _directMoveSideFriction, MovingType.direct);
            SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _directMoveVelocityLimit, _directMoveSideFriction, MovingType.direct);

        }

        // ���� ��������� �������������� ���������, �� ������������� ��� - �������� �� �������
        else if (Mathf.Abs(_forwardAcceleration) <= _deltaVertical && Mathf.Abs(_rotateAcceleration) > _deltaHorizontal)
        {
            totalLeftAcceleration = _rotateAcceleration;
            totalRightAcceleration = -_rotateAcceleration;
            breakPowerScaler = _breakPowerScaler;

            if (_currentSpeed > 1f)
            {
                // ������� ������ ��������� ���������� �� ���, ������� ����������� ��������, ����������� ���������� � ��� ��������
                SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _completeMoveSideFriction, MovingType.on_stand);
                SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _completeMoveSideFriction, MovingType.on_stand);
            }
            else
            {
                SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _rotateOnStandSideFriction, MovingType.on_stand);
                SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _rotateOnStandVelocityLimit, _rotateOnStandSideFriction, MovingType.on_stand);
            }
        }
        else
        {
            if (_forwardAcceleration > 0)
            {
                // ���� ������� ������
                if (_rotateAcceleration < 0)
                {
                    totalLeftAcceleration = _forwardAcceleration + _rotateAcceleration;
                    totalRightAcceleration = _forwardAcceleration;
                }
                else if (_rotateAcceleration > 0)
                {
                    totalLeftAcceleration = _forwardAcceleration;
                    totalRightAcceleration = _forwardAcceleration - _rotateAcceleration;
                }
            }
            else if (_forwardAcceleration < 0)
            {
                // ���� ������� ������
                if (_rotateAcceleration < 0)
                {
                    totalLeftAcceleration = _forwardAcceleration - _rotateAcceleration;
                    totalRightAcceleration = _forwardAcceleration;
                }
                else if (_rotateAcceleration > 0)
                {
                    totalLeftAcceleration = _forwardAcceleration;
                    totalRightAcceleration = _forwardAcceleration + _rotateAcceleration;
                }
            }

            breakPowerScaler = _breakPowerScaler;

            // ������� ������ ��������� ���������� �� ���, ������� ����������� ��������, ����������� ���������� � ��� ��������
            SelectedSideMoving(ref _leftWheelsColliders, totalLeftAcceleration, breakPowerScaler, _directMoveVelocityLimit, _completeMoveSideFriction, MovingType.complette);
            SelectedSideMoving(ref _rightWheelsColliders, totalRightAcceleration, breakPowerScaler, _directMoveVelocityLimit, _completeMoveSideFriction, MovingType.complette);
        }

    }
    // ���������������� ��������� ��������� ����������� �����, ���������� �� ������� CompletteDrive()
    void SelectedSideMoving(ref WheelCollider[] collidersArray, float acceleration, float breakScaler, float maxVelocity, float frictionStiffnes, MovingType type)
    {
        for (int i = 0; i < collidersArray.Length; i++)
        {
            WheelFrictionCurve frictionSet = collidersArray[i].sidewaysFriction;

            if (!collidersArray[i].GetGroundHit(out WheelHit hit))
            {
                frictionSet.stiffness = _defaultSideFriction;
                collidersArray[i].sidewaysFriction = frictionSet;
                collidersArray[i].motorTorque = 0;
                collidersArray[i].brakeTorque = _brakeForce;
            }

            else
            {

                switch (type)
                {
                    case MovingType.autobreak:
                        frictionSet.stiffness = _defaultSideFriction;
                        collidersArray[i].sidewaysFriction = frictionSet;
                        collidersArray[i].motorTorque = (acceleration * _engineForce);
                        collidersArray[i].brakeTorque = (breakScaler * _brakeForce);
                        break;

                    case MovingType.complette:
                    // ������������� ���� � �������� ��� �� ��� � ������ ���
                    case MovingType.direct:
                        if (_currentSpeed > maxVelocity)
                        {
                            frictionSet.stiffness = _defaultSideFriction;
                            collidersArray[i].sidewaysFriction = frictionSet;
                            collidersArray[i].brakeTorque = _brakeForce;
                        }
                        else
                        {
                            frictionSet.stiffness = frictionStiffnes;
                            collidersArray[i].sidewaysFriction = frictionSet;
                            collidersArray[i].brakeTorque = (breakScaler * _brakeForce);
                            collidersArray[i].motorTorque = (acceleration * _engineForce);
                        }
                        break;

                    case MovingType.on_stand:
                        if (_angularSpeed > maxVelocity)
                        {
                            frictionSet.stiffness = _defaultSideFriction;
                            collidersArray[i].sidewaysFriction = frictionSet;
                            collidersArray[i].brakeTorque = _brakeForce;
                        }
                        else
                        {
                            frictionSet.stiffness = frictionStiffnes;
                            collidersArray[i].sidewaysFriction = frictionSet;
                            collidersArray[i].brakeTorque = (breakScaler * _brakeForce);
                            collidersArray[i].motorTorque = (acceleration * _engineForce);
                        }
                        break;

                    case MovingType.out_of_hit:
                        frictionSet.stiffness = _defaultSideFriction;
                        collidersArray[i].sidewaysFriction = frictionSet;
                        collidersArray[i].motorTorque = 0;
                        collidersArray[i].brakeTorque = _brakeForce;
                        break;
                }
            }
        }
    }

    // ���������� ����������� �����, ������ �������� � ����������� �����
    void UpdateWheelTransform(WheelCollider colider, Transform transform)
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        colider.GetWorldPose(out position, out rotation);
        transform.position = position;
        transform.rotation = rotation;
    }
    void UpdateWheelTransform(ref WheelCollider[] coliders, ref Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            Vector3 position = transforms[i].position;
            Quaternion rotation = transforms[i].rotation;

            coliders[i].GetWorldPose(out position, out rotation);
            transforms[i].position = (position + new Vector3(0, _suspentionDistance, 0));
            //transforms[i].col_position = col_position;
            transforms[i].rotation = rotation;
        }
    }
    void UpdateWheelTransform(ref WheelCollider[] coliders, ref Transform[] transforms, ref Transform[] bones)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            // �������� ������ �� ������� ������� � �������� ����������
            Vector3 position = transforms[i].position;
            Quaternion rotation = transforms[i].rotation;
            coliders[i].GetWorldPose(out position, out rotation);

            //transforms[i].col_position = (col_position + new Vector3(0, _suspentionDistance, 0));
            transforms[i].position = position;
            //bones[i].col_position = (col_position - new Vector3(0, coliders[i].radius + (coliders[i].suspensionDistance / 2), 0) + new Vector3(0.3f, 0, 0));
            transforms[i].rotation = rotation;

            // ��������� ���������� �������������� ����� ��������
            {
                Vector3 bone_position = bones[i].position;
                // ����� ���������� �� ����������, �������� ������ ����� � ������ ��������
                bone_position.y = (position.y - coliders[i].radius - (coliders[i].suspensionDistance / 2));
                // �������������� ������
                bones[i].position = bone_position;
            }
        }
    }

    void UpdateWheelTransform(ref Quaternion rotation, ref WheelCollider[] coliders, ref Transform[] transforms, ref Transform[] bones)
    {
        bool isFirst = true;
        float delta_angle = 0.0f;

        for (int i = 0; i < transforms.Length; i++)
        {
            // �������� ������ �� ������� ������� � �������� ����������
            Vector3 col_position = new Vector3(0, 0, 0);
            Quaternion col_rotation = new Quaternion(0, 0, 0, 0);
            coliders[i].GetWorldPose(out col_position, out col_rotation);

            // ���������� ������ ���� �������� ��� ����� ���� �����
            {
                if (isFirst)
                {
                    // ��� ������� ������ � ������� ���������� ������ ���� ��������
                    delta_angle = Quaternion.Angle(transforms[i].rotation, col_rotation);
                    // ����� ������������ �������� �������� �������� ����������
                    rotation = col_rotation;

                    if (delta_angle != 0.0f)
                    {
                        // ��������� ���� ������ � ��� ������, ���� ���� �� ������ ����
                        isFirst = false;
                        // ����� ��� ���������, ��� ������ ����� � �������
                        // �������������� �� ���� �� �������� �������� � ��� ��������
                    }
                }

                else
                {
                    // �������� ������� ���� �������� ���������� ������
                    float angle = Quaternion.Angle(transforms[i].rotation, col_rotation);
                    // ���� �������� ���������� ������� ������ ������ ������� ������, �� �������������� ������ � �������� ��������
                    if (angle != 0.0f && angle < delta_angle)
                    {
                        rotation = col_rotation; delta_angle = angle;
                    }
                }
            }

            // ���������� ���������� ���������������� ���� ������
            {
                Vector3 wheel_position = transforms[i].position;
                // ����� ���������� ���������
                wheel_position.y = col_position.y + 0.03f;
                // �������������� ������
                transforms[i].position = wheel_position;
            }

            // ��������� ���������� �������������� ����� ��������
            {
                Vector3 bone_position = bones[i].position;
                // ����� ���������� �� ����������, �������� ������ ����� � ������ ��������
                bone_position.y = (col_position.y - coliders[i].radius/* - (coliders[i].suspensionDistance / 2)*/);
                // �������������� ������
                bones[i].position = bone_position;
            }
        }
    }

    void UpdateWheelTransform(ref float texture_offset, ref Quaternion rotation, ref WheelCollider[] coliders, ref Transform[] transforms, ref Transform[] bones)
    {
        bool isFirst = true;
        float delta_angle = 0.0f;

        for (int i = 0; i < transforms.Length; i++)
        {
            // �������� ������ �� ������� ������� � �������� ����������
            Vector3 col_position = new Vector3(0, 0, 0);
            Quaternion col_rotation = new Quaternion(0, 0, 0, 0);
            coliders[i].GetWorldPose(out col_position, out col_rotation);

            // ���������� ������ ���� �������� ��� ����� ���� �����
            {
                if (isFirst)
                {
                    // ��� ������� ������ � ������� ���������� ������ ���� ��������
                    delta_angle = Quaternion.Angle(transforms[i].rotation, col_rotation);
                    // ����� ������������ �������� �������� �������� ����������
                    rotation = col_rotation;

                    if (delta_angle != 0.0f)
                    {
                        // ��������� ���� ������ � ��� ������, ���� ���� �� ������ ����
                        isFirst = false;
                        // ����� ��� ���������, ��� ������ ����� � �������
                        // �������������� �� ���� �� �������� �������� � ��� ��������
                    }
                }

                else
                {
                    // �������� ������� ���� �������� ���������� ������
                    float angle = Quaternion.Angle(transforms[i].rotation, col_rotation);
                    // ���� �������� ���������� ������� ������ ������ ������� ������, �� �������������� ������ � �������� ��������
                    if (angle != 0.0f && angle < delta_angle)
                    {
                        rotation = col_rotation; delta_angle = angle;
                    }
                }
            }

            DELTA_ANGLE = delta_angle;

            texture_offset = texture_offset + (delta_angle / 3600);

            // ���������� ���������� ���������������� ���� ������
            {
                Vector3 wheel_position = transforms[i].position;
                // ����� ���������� ���������
                wheel_position.y = col_position.y + 0.03f;
                // �������������� ������
                transforms[i].position = wheel_position;
            }

            // ��������� ���������� �������������� ����� ��������
            {
                Vector3 bone_position = bones[i].position;
                // ����� ���������� �� ����������, �������� ������ ����� � ������ ��������
                bone_position.y = (col_position.y - coliders[i].radius/* - (coliders[i].suspensionDistance / 2)*/);
                // �������������� ������
                bones[i].position = bone_position;
            }
        }
    }

    void UpdateReferenceTransform(Transform reference, ref Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            Quaternion rotation = reference.rotation;
            transforms[i].rotation = rotation;
        }
    }

    void UpdateWheelsRotation(ref Quaternion rotation, ref Transform[] transforms)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].rotation = rotation;
        }
    }

    void UpdateTruckTextureOffset(ref float offset, ref GameObject truck)
    {
        truck.GetComponent<SkinnedMeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0, offset));
    }





    // ----------------------------------------------- �������������� ������ ������ ������� -----------------------------------------

    void DirectMoving()
    {

        DirectDrive(ref _rightWheelsColliders);
        DirectDrive(ref _leftWheelsColliders);

        _forwardAcceleration = 0;
        _rotateAcceleration = 0;
    }
    // ������� ������� �������������� �������� ������ � �����
    void DirectDrive(ref WheelCollider[] collidersArray)
    {
        for (int i = 0; i < collidersArray.Length; i++)
        {
            WheelFrictionCurve frictionSet = collidersArray[i].sidewaysFriction;

            if (Mathf.Abs(_forwardAcceleration) >= 0 && Mathf.Abs(_forwardAcceleration) <= _deltaVertical)
            {
                // ���� ��� ������ �����������, �� ��������
                frictionSet.stiffness = 0.8f;
                collidersArray[i].sidewaysFriction = frictionSet;
                collidersArray[i].brakeTorque = _brakeForce;
            }
            else if (_currentSpeed > _directMoveVelocityLimit)
            {
                // ���� ������� �������� ��������� ������������, �� ��������
                frictionSet.stiffness = 0.8f;
                collidersArray[i].sidewaysFriction = frictionSet;
                collidersArray[i].brakeTorque = _brakeForce;
            }
            else
            {
                frictionSet.stiffness = 0.06f;
                collidersArray[i].sidewaysFriction = frictionSet;
                collidersArray[i].brakeTorque = 0;
                collidersArray[i].motorTorque = ((_forwardAcceleration + _rotateAcceleration) * _engineForce);
            }
        }
    }
    void OnStandRotation()
    {
        for (int i = 0; i < _leftWheelsColliders.Length; i++)
        {
            WheelFrictionCurve frictionSet = _leftWheelsColliders[i].sidewaysFriction;

            if (Mathf.Abs(_rotateAcceleration) >= 0 && Mathf.Abs(_rotateAcceleration) <= _deltaHorizontal)
            {
                frictionSet.stiffness = 0.8f;
                _leftWheelsColliders[i].sidewaysFriction = frictionSet;
                _leftWheelsColliders[i].brakeTorque = _brakeForce;
            }
            else if (_angularSpeed > _rotateOnStandVelocityLimit)
            {
                frictionSet.stiffness = 0.8f;
                _leftWheelsColliders[i].sidewaysFriction = frictionSet;
                _leftWheelsColliders[i].brakeTorque = _brakeForce;
            }
            else
            {
                frictionSet.stiffness = 0.06f;
                _leftWheelsColliders[i].sidewaysFriction = frictionSet;
                _leftWheelsColliders[i].brakeTorque = 0;
                _leftWheelsColliders[i].motorTorque = (_rotateAcceleration * _engineForce);
            }
        }

        for (int i = 0; i < _rightWheelsColliders.Length; i++)
        {
            WheelFrictionCurve frictionSet = _rightWheelsColliders[i].sidewaysFriction;

            if (Mathf.Abs(_rotateAcceleration) >= 0 && Mathf.Abs(_rotateAcceleration) <= _deltaHorizontal)
            {
                frictionSet.stiffness = 0.8f;
                _rightWheelsColliders[i].sidewaysFriction = frictionSet;
                _rightWheelsColliders[i].brakeTorque = _brakeForce;
            }
            else if (_angularSpeed > _rotateOnStandVelocityLimit)
            {
                frictionSet.stiffness = 0.8f;
                _rightWheelsColliders[i].sidewaysFriction = frictionSet;
                _rightWheelsColliders[i].brakeTorque = _brakeForce;
            }
            else
            {
                frictionSet.stiffness = 0.06f;
                _rightWheelsColliders[i].sidewaysFriction = frictionSet;
                _rightWheelsColliders[i].brakeTorque = 0;
                _rightWheelsColliders[i].motorTorque = (-_rotateAcceleration * _engineForce);
            }
        }
    }
}
