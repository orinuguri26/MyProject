using UnityEngine;


[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName; // 무기 이름 (예: Shotgun)
    // 나중에 여기에 기본 공격력, 쿨타임 등을 넣어서 관리해도 좋습니다.
}