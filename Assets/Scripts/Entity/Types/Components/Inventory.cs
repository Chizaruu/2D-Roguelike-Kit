using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Inventory : MonoBehaviour {
  
  [field: SerializeField] public int Capacity { get; private set; } = 22;
  [field: SerializeField] public Consumable SelectedConsumable { get; set; } = null;
  [field: SerializeField] public List<Item> Items { get; private set; } = new List<Item>();

  public void Add(Item item) {
    Items.Add(item);
    item.transform.SetParent(transform);
    GameManager.instance.RemoveEntity(item);
  }

  public void Drop(Item item) {
    Items.Remove(item);
    item.transform.SetParent(null);
    GameManager.instance.AddOrInsertEntity(item);
    UIManager.instance.AddMessage($"You dropped the {item.name}.", "#FF0000");
  }
}