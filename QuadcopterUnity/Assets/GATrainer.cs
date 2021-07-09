using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class GATrainer : MonoBehaviour {

    public GameObject TrainingArea;
    public int Instances;
    public int Rows;
    public float Separation;
    public double SurvivalProportion;
    public double MutationProbability;
    public bool QuickTimescale;

    int EpisodesCompleted;
    float BestEpisodeFitness;
    float[] BestEpisodeData;

    List<float[]> LoggedFittestData;
    List<float> LoggedEpisodeEnds;

    GeneticAlgorithm PIDTrainer;
    GameObject[] TrainingAreas;

    void Start() {
        if(QuickTimescale) {
            Time.timeScale = 100;
        }
        PIDTrainer = new GeneticAlgorithm(Instances, 9, 0.0D, 0.1D, SurvivalProportion, MutationProbability);
        EpisodesCompleted = 0;
        BestEpisodeFitness = 0.0F;
        LoggedFittestData = new List<float[]>();
        LoggedEpisodeEnds = new List<float>();
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

    void EpisodeEnded(Tuple<int, float[]> endData) {
        float[] endValues = endData.Item2;
        float fitness = endValues[0];
        if(fitness > BestEpisodeFitness) {
            BestEpisodeFitness = fitness;
            BestEpisodeData = endValues;
        }
        PIDTrainer.SetFitness(endData.Item1, fitness);
        EpisodesCompleted++;
    }

    void FixedUpdate() {
        if(EpisodesCompleted == Instances) {
            Debug.Log("Fitness: " + BestEpisodeFitness);
            LoggedFittestData.Add(BestEpisodeData);
            LoggedEpisodeEnds.Add(Time.timeSinceLevelLoad);
            PIDTrainer.Evolve();    
            BestEpisodeFitness = 0.0F;
            EpisodesCompleted = 0;
            for(int i = 0; i < Instances; i++) {
                Tuple<int, Genome> initialisationData = new Tuple<int, Genome>(i, PIDTrainer.GetGenome(i));
                TrainingAreas[i].transform.GetChild(0).SendMessage("InitialisePID", initialisationData);
            }
        }
    }

    public void OnDestroy() {
        string CSVOutput = "Iteration, Time, Fitness, Final Distance, Final Speed, Final Angular Speed, Final Yaw, Final Pitch, Final Roll\n";
        for(int i = 0; i < LoggedFittestData.Count; i++) {
            CSVOutput += i.ToString() + ", " + LoggedEpisodeEnds[i].ToString();
            for(int j = 0; j <= 6; j++) {
                CSVOutput += ", " + LoggedFittestData[i][j].ToString();
            } 
            CSVOutput += "\n";
        }
        StreamWriter Writer = new StreamWriter("results/HoverPID/TrainingData.csv", false);
        Writer.Write(CSVOutput);
        Writer.Close();
        string TXTOutput = "";
        double[] BestValues = PIDTrainer.GetFittest().GetChromosome();
        for(int i = 0; i < BestValues.Length; i++) {
            TXTOutput += String.Format("{0:0.00000}", BestValues[i]);
            if(i != BestValues.Length - 1) {
                TXTOutput += ", ";
            }
        }
        Writer = new StreamWriter("results/HoverPID/Parameters.txt", false);
        Writer.Write(TXTOutput);
        Writer.Close();
        Debug.Log("Wrote PID data to file");
    }

}
