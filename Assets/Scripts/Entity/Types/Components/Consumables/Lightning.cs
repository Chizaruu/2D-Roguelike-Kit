using UnityEngine;

public class Lightning : Consumable {
  [field: SerializeField] public int damage { get; private set; } = 20;
  [field: SerializeField] public int maximumRange { get; private set; } = 5;

  public override bool Activate(Actor consumer) {
    consumer.inventory.SelectedConsumable = this;
    consumer.GetComponent<Player>().ToggleTargetMode();
    UIManager.instance.AddMessage("Select a target to strike.", "#63FFFF");
    return false;
  }

  public override bool Cast(Actor consumer, Actor target) {
    UIManager.instance.AddMessage($"A lighting bolt strikes the {target.name} with a loud thunder, for {damage} damage!", "#FFFFFF");
    target.fighter.Hp -= damage;
    Consume(consumer);
    consumer.GetComponent<Player>().ToggleTargetMode();
    return true;
  }
}
