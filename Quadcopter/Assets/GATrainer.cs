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
    public double MaxPIDValue;
    public bool QuickTimescale;

    int GenerationLimit = 250;
    int NoisePasses = 5;
    int NoiseIterations;
    int EpisodesCompleted;
    int GenerationsCompleted;

    List<float[]> LoggedFittestData;
    List<float> LoggedEpisodeEnds;

    GeneticAlgorithm PIDTrainer;
    GameObject[] TrainingAreas;

    void Start() {
        if(QuickTimescale) {
            Time.timeScale = 100;
        }
        PIDTrainer = new GeneticAlgorithm(Instances, 9, 0.0D, MaxPIDValue, SurvivalProportion, MutationProbability);
        NoiseIterations = 0;
        EpisodesCompleted = 0;
        GenerationsCompleted = 0;
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
        float[] endValues = new float[7];
        float fitness = endData.Item2[0];
        for(int i = 0; i < 7; i++) {
            endValues[i] = (float)endData.Item2[i] / NoisePasses;
        }
        PIDTrainer.AddFitness(endData.Item1, (float)fitness / NoisePasses);
        PIDTrainer.AggregateData(endData.Item1, endValues);
        EpisodesCompleted++;
    }

    void FixedUpdate() {
        if(EpisodesCompleted == Instances) {
            EpisodesCompleted = 0;
            NoiseIterations++;
            if(NoiseIterations == NoisePasses) {
                NoiseIterations = 0;
                LoggedEpisodeEnds.Add(Time.timeSinceLevelLoad);
                Tuple<float, float[]> TrainerData = PIDTrainer.Evolve();
                float BestEpisodeFitness = TrainerData.Item1;
                float[] BestEpisodeData = TrainerData.Item2;
                Debug.Log("Fitness: " + BestEpisodeFitness);
                BestEpisodeData[0] = BestEpisodeFitness;
                LoggedFittestData.Add(BestEpisodeData);
                GenerationsCompleted++;
            }
            if(GenerationsCompleted == GenerationLimit) {
                Application.Quit(0);
                //UnityEditor.EditorApplication.isPlaying = false;
            }
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
            for(int j = 0; j < 7; j++) {
                CSVOutput += ", " + LoggedFittestData[i][j].ToString();
            } 
            CSVOutput += "\n";
        }
        StreamWriter Writer = new StreamWriter("tmp/TrainingData.csv", false);
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
        Writer = new StreamWriter("tmp/Parameters.txt", false);
        Writer.Write(TXTOutput);
        Writer.Close();
        Debug.Log("Wrote PID data to file");
    }

}
