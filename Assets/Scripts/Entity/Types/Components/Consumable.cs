using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class Consumable : MonoBehaviour {
  public virtual bool Activate(Actor actor) => false;
  public virtual bool Cast(Actor actor, Actor target) => false;
  public virtual bool Cast(Actor actor, List<Actor> targets) => false;

  public void Consume(Actor consumer) {
    if (consumer.inventory.SelectedConsumable == this) {
      consumer.inventory.SelectedConsumable = null;
    }

    consumer.inventory.Items.Remove(GetComponent<Item>());
    GameManager.instance.RemoveEntity(GetComponent<Item>());
    Destroy(gameObject);
  }
}