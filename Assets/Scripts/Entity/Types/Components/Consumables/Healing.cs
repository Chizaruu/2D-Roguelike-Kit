using UnityEngine;

namespace TheSleepyKoala.Entities
{
  [RequireComponent(typeof(Item))]
  public class Healing : Consumable
  {
    [field: SerializeField] public int amount { get; private set; } = 4;

    public override bool Activate(Actor consumer)
    {
      int amountRecovered = consumer.Fighter.Heal(amount);

      if (amountRecovered > 0)
      {
        UIManager.instance.AddMessage($"You consume the {name}, and recover {amountRecovered} HP!", "#00FF00");
        Consume(consumer);
        return true;
      }
      else
      {
        UIManager.instance.AddMessage("Your health is already full.", "#808080");
        return false;
      }
    }
  }
}