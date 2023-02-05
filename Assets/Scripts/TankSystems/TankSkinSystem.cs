using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TankSkinSystem : MonoBehaviour
{
    public enum TankSkin
    {
        None, Green, Red
    }

    [Tooltip("Выбранный цвет танчика, требует заполнения массивов по объектам и материалам")]
    public TankSkin _usedSkin = TankSkin.None;
    private TankSkin _archiveSkin = TankSkin.None;

    [Tooltip("Список объектов MeshRenderer к которым будет примененено изменение цвета")]
    public List<MeshRenderer> _Objects;
    [Tooltip("Список имеющихся материалов")]
    public List<Material> _Materials;

    private void Update()
    {
        ColorChecker();
    }

    private void ColorChecker()
    {
        switch (_usedSkin)
        {
            case TankSkin.None:
                ColorUpdater(0);
                break;

            case TankSkin.Green:
                ColorUpdater(1);
                break;

            case TankSkin.Red:
                ColorUpdater(2);
                break;
        }

        _archiveSkin = _usedSkin;
    }

    private void ColorUpdater(int index)
    {
        if (_usedSkin != _archiveSkin)
        {
            foreach (var obj in _Objects)
            {
                obj.material = _Materials[index];
            }
        }
    }
}