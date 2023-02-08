using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Equipment : MonoBehaviour {
  [field: SerializeField] public Equippable Weapon { get; set; }
  [field: SerializeField] public Equippable Armor { get; set; }

  public int DefenseBonus() {
    int bonus = 0;

    if (Weapon is not null && Weapon.DefenseBonus > 0) {
      bonus += Weapon.DefenseBonus;
    }

    if (Armor is not null && Armor.DefenseBonus > 0) {
      bonus += Armor.DefenseBonus;
    }
    return bonus;
  }

  public int PowerBonus() {
    int bonus = 0;

    if (Weapon is not null && Weapon.PowerBonus > 0) {
      bonus += Weapon.PowerBonus;
    }

    if (Armor is not null && Armor.PowerBonus > 0) {
      bonus += Armor.PowerBonus;
    }

    return bonus;
  }

  public bool ItemIsEquipped(Item item) {
    if (item.Equippable is null) {
      return false;
    }

    return item.Equippable == Weapon || item.Equippable == Armor;
  }

  public void UnequipMessage(string name) {
    UIManager.instance.AddMessage($"You remove the {name}.", "#da8ee7");
  }

  public void EquipMessage(string name) {
    UIManager.instance.AddMessage($"You equip the {name}.", "#a000c8");
  }

  public void EquipToSlot(string slot, Item item, bool addMessage) {
    Equippable currentItem = slot == "Weapon" ? Weapon : Armor;

    if (currentItem is not null) {
      UnequipFromSlot(slot, addMessage);
    }

    if (slot == "Weapon") {
      Weapon = item.Equippable;
    } else {
      Armor = item.Equippable;
    }

    if (addMessage) {
      EquipMessage(item.name);
    }

    item.name = $"{item.name} (E)";
  }

  public void UnequipFromSlot(string slot, bool addMessage) {
    Equippable currentItem = slot == "Weapon" ? Weapon : Armor;
    currentItem.name = currentItem.name.Replace(" (E)", "");

    if (addMessage) {
      UnequipMessage(currentItem.name);
    }

    if (slot == "Weapon") {
      Weapon = null;
    } else {
      Armor = null;
    }
  }

  public void ToggleEquip(Item equippableItem, bool addMessage = true) {
    string slot = equippableItem.Equippable.EquipmentType == EquipmentType.Weapon ? "Weapon" : "Armor";

    if (ItemIsEquipped(equippableItem)) {
      UnequipFromSlot(slot, addMessage);
    } else {
      EquipToSlot(slot, equippableItem, addMessage);
    }
  }
}
