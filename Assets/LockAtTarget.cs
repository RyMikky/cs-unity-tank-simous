using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockAtTarget : MonoBehaviour
{
    public bool Enable = false;

    public Transform LockAt_Target;                 // ������ ����� �������� ����� ���������� �������
    public Transform Tower_Axix;                    // ������ - ��� �������� ����� �����

    void Update()
    {
        if (Enable)
        {
            // ����� ������� �� ������� �������� �������
            Vector3 forward = (LockAt_Target.position - Tower_Axix.position).normalized;

            forward.y = 0; // �������� ��� y, ����� ����� ������ ��������� ������ ������ ��

            // ������������ ����� �� �������� �������������
            Tower_Axix.rotation = Quaternion.LookRotation(forward);
        }
    }
}