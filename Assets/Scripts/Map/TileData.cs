using UnityEngine;

/// <summary> A tile on some map. </summary>
[System.Serializable]
public class TileData {
  [field: SerializeField] public string name { get; set; }
  [field: SerializeField] public bool isExplored { get; set; }
  [field: SerializeField] public bool isVisible { get; set; }

  public TileData(string name, bool isExplored, bool isVisible) {
    this.name = name;
    this.isExplored = isExplored;
    this.isVisible = isVisible;
  }
}