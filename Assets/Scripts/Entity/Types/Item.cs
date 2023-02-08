using TheSleepyKoala.Map;
using UnityEngine;

namespace TheSleepyKoala.Entities
{
  public class Item : Entity
  {
    [field: SerializeField] public Consumable Consumable { get; private set; }
    [field: SerializeField] public Equippable Equippable { get; private set; }

    private void OnValidate()
    {
      if (GetComponent<Consumable>())
      {
        Consumable = GetComponent<Consumable>();
      }
    }

    private void Start() => AddToGameManager();

    public override EntityState SaveState() => new ItemState(
        name: name,
        blocksMovement: BlocksMovement,
        isVisible: MapManager.instance.VisibleTiles.Contains(MapManager.instance.FloorMap.WorldToCell(transform.position)),
        position: transform.position,
        parent: transform.parent != null ? transform.parent.gameObject.name : ""
      );

    public void LoadState(ItemState state)
    {
      if (!state.IsVisible)
      {
        SpriteRenderer.enabled = false;
      }

      if (state.Parent is not "")
      {
        GameObject parent = GameObject.Find(state.Parent);
        parent.GetComponent<Inventory>().Add(this);

        if (Equippable is not null && state.Name.Contains("(E)"))
        {
          parent.GetComponent<Equipment>().EquipToSlot(Equippable.EquipmentType.ToString(), this, false);
        }
      }

      transform.position = state.Position;
    }
  }

  [System.Serializable]
  public class ItemState : EntityState
  {
    [field: SerializeField] public string Parent { get; set; }

    public ItemState(EntityType type = EntityType.Item, string name = "", bool blocksMovement = false, bool isVisible = false, Vector3 position = new Vector3(),
     string parent = "") : base(type, name, blocksMovement, isVisible, position)
    {
      this.Parent = parent;
    }
  }
}