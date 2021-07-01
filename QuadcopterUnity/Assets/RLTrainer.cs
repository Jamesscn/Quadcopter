using UnityEngine;

/**
The RLTrainer class creates multiple agents in parallel to increase the speed of training.
*/
public class RLTrainer : MonoBehaviour {

    public GameObject TrainingArea;
    public int Instances;
    public int Rows;
    public float Separation;

    void Start() {
        for(int i = 0; i < Instances; i++) {
            int x = i % Rows;
            int z = (i / Rows) % Rows;
            int y = (i / (Rows * Rows));
            Vector3 AreaPosition = new Vector3(x - Rows / 2.0F, y, z - Rows / 2.0F) * Separation;
            Instantiate(TrainingArea, AreaPosition, Quaternion.identity);
        }
    }

}
