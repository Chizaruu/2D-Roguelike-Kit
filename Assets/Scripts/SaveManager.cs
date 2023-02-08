using UnityEngine;
using OdinSerializer;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour {
  public static SaveManager instance;

  [field: SerializeField] public int currentFloor { get; set; } = 0;
  [SerializeField] private string saveFileName = "saveThe.koala";
  [field: SerializeField] public SaveData save { get; set; }

  private void Awake() {
    if (SaveManager.instance == null) {
      SaveManager.instance = this;
      DontDestroyOnLoad(gameObject);
    } else {
      Destroy(gameObject);
    }
  }

  public bool HasSaveAvailable() {
    string path = Path.Combine(Application.persistentDataPath, saveFileName);

    if (!File.Exists(path)) {
      return false;
    }
    return true;
  }

  public void SaveGame(bool tempSave = true) {
    save.savedFloor = currentFloor;

    bool hasScene = save.scenes.Find(x => x.floorNumber == currentFloor) is not null;
    if (hasScene) {
      UpdateScene(SaveState());
    } else {
      AddScene(SaveState());
    }

    if (tempSave) return;

    string path = Path.Combine(Application.persistentDataPath, saveFileName);
    byte[] saveJson = SerializationUtility.SerializeValue(save, DataFormat.JSON); //Serialize the state to JSON
    File.WriteAllBytes(path, saveJson); //Save the state to a file
  }

  public void LoadGame() {
    string path = Path.Combine(Application.persistentDataPath, saveFileName);
    byte[] saveJson = File.ReadAllBytes(path); //Load the state from the file
    save = SerializationUtility.DeserializeValue<SaveData>(saveJson, DataFormat.JSON); //Deserialize the state from JSON

    currentFloor = save.savedFloor;

    if (SceneManager.GetActiveScene().name is not "Dungeon") {
      SceneManager.LoadScene("Dungeon");
    } else {
      LoadScene();
    }
  }

  public void DeleteSave() {
    string path = Path.Combine(Application.persistentDataPath, saveFileName);
    File.Delete(path);
  }

  public void AddScene(SceneState sceneState) => save.scenes.Add(sceneState);

  public void UpdateScene(SceneState sceneState) => save.scenes[currentFloor - 1] = sceneState;

  public void LoadScene(bool canRemovePlayer = true) {
    SceneState sceneState = save.scenes.Find(x => x.floorNumber == currentFloor);
    if (sceneState is not null) {
      LoadState(sceneState, canRemovePlayer);
    } else {
      Debug.LogError("No save data for this floor");
    }
  }

  public SceneState SaveState() => new SceneState(
    currentFloor,
    GameManager.instance.SaveState(),
    MapManager.instance.SaveState()
  );

  public void LoadState(SceneState sceneState, bool canRemovePlayer) {
    MapManager.instance.LoadState(sceneState.mapState);
    GameManager.instance.LoadState(sceneState.gameState, canRemovePlayer);
  }
}

[System.Serializable]
public class SaveData {
  [field: SerializeField] public int savedFloor { get; set; }
  [field: SerializeField] public List<SceneState> scenes { get; set; }

  public SaveData() {
    savedFloor = 0;
    scenes = new List<SceneState>();
  }
}

[System.Serializable]
public class SceneState {
  [field: SerializeField] public int floorNumber { get; set; }
  [field: SerializeField] public GameState gameState { get; set; }
  [field: SerializeField] public MapState mapState { get; set; }

  public SceneState(int floorNumber, GameState gameState, MapState mapState) {
    this.floorNumber = floorNumber;
    this.gameState = gameState;
    this.mapState = mapState;
  }
}