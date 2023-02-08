using System.Collections.Generic;
using UnityEngine;

namespace TheSleepyKoala.Entities
{
  public class Fireball : Consumable
  {
    [field: SerializeField] public int damage { get; private set; } = 12;
    [field: SerializeField] public int radius { get; private set; } = 3;

    public override bool Activate(Actor consumer)
    {
      consumer.Inventory.SelectedConsumable = this;
      consumer.GetComponent<Player>().ToggleTargetMode(true, radius);
      UIManager.instance.AddMessage($"Select a location to throw a fireball.", "#63FFFF");
      return false;
    }

    public override bool Cast(Actor consumer, List<Actor> targets)
    {
      foreach (Actor target in targets)
      {
        UIManager.instance.AddMessage($"The {target.name} is engulfed in a fiery explosion, taking {damage} damage!", "#FF0000");
        target.Fighter.Hp -= damage;
      }

      Consume(consumer);
      consumer.GetComponent<Player>().ToggleTargetMode();
      return true;
    }
  }
}