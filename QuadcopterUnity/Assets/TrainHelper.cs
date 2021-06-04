using UnityEngine;

public class TrainHelper : MonoBehaviour {

    public GameObject TrainingArea;
    public int instances;
    public int rows;
    public float separation;

    void Start() {
        for(int i = 0; i < instances; i++) {
            int x = i % rows;
            int z = (i / rows) % rows;
            int y = (i / (rows * rows));
            Vector3 AreaPosition = new Vector3(x - rows / 2.0F, y, z - rows / 2.0F) * separation;
            Instantiate(TrainingArea, AreaPosition, Quaternion.identity);
        }
    }

}
