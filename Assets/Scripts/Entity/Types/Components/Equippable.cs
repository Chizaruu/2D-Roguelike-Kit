using UnityEngine;

[RequireComponent(typeof(Item))]
public class Equippable : MonoBehaviour {
  [field: SerializeField] public EquipmentType EquipmentType { get; set; }
  [field: SerializeField] public int PowerBonus { get; set; } = 0;
  [field: SerializeField] public int DefenseBonus { get; set; } = 0;
}