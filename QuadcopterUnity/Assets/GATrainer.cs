using System;
using UnityEngine;

public class GATrainer : MonoBehaviour {

    public GameObject TrainingArea;
    public int Instances;
    public int Rows;
    public float Separation;
    public double SurvivalProportion;
    public double MutationProbability;

    int EpisodesCompleted;

    GeneticAlgorithm PIDTrainer;
    GameObject[] TrainingAreas;

    void Start() {
        PIDTrainer = new GeneticAlgorithm(Instances, 9, 0.0D, 7.0D, SurvivalProportion, MutationProbability);
        EpisodesCompleted = 0;
        TrainingAreas = new GameObject[Instances];
        for(int i = 0; i < Instances; i++) {
            int x = i % Rows;
            int z = (i / Rows) % Rows;
            int y = (i / (Rows * Rows));
            Vector3 AreaPosition = new Vector3(x - Rows / 2.0F, y, z - Rows / 2.0F) * Separation;
            TrainingAreas[i] = Instantiate(TrainingArea, AreaPosition, Quaternion.identity);
            TrainingAreas[i].transform.SetParent(transform);
            Tuple<int, Genome> initialisationData = new Tuple<int, Genome>(i, PIDTrainer.GetGenome(i));
            TrainingAreas[i].transform.GetChild(0).SendMessage("InitialisePID", initialisationData);
        }
    }

    void EpisodeEnded(Tuple<int, double> rewardData) {
        PIDTrainer.SetFitness(rewardData.Item1, rewardData.Item2);
        EpisodesCompleted++;
    }

    void FixedUpdate() {
        if(EpisodesCompleted == Instances) {
            PIDTrainer.Evolve();

            double[] BestValues = PIDTrainer.GetFittest().GetChromosome();
            String FittestString = "";
            for(int i = 0; i < BestValues.Length; i++) {
                FittestString += BestValues[i];
                if(i != BestValues.Length - 1) {
                    FittestString += ", ";
                }
            }
            Debug.Log(FittestString);
            
            EpisodesCompleted = 0;
            for(int i = 0; i < Instances; i++) {
                Tuple<int, Genome> initialisationData = new Tuple<int, Genome>(i, PIDTrainer.GetGenome(i));
                TrainingAreas[i].transform.GetChild(0).SendMessage("InitialisePID", initialisationData);
            }
        }
    }

}
