using UnityEngine;

/**
The GATest class is used to test the GeneticAlgorithm class by observing the genetic algorithm given a task that must maximise the values of the alleles of a chromosome.
*/
public class GATest : MonoBehaviour {

    GeneticAlgorithm GA;

    void Start() {
        GA = new GeneticAlgorithm(10, 6, -0.0D, 2.0D, 0.4D, 0.15D);
    }

    void FixedUpdate() {
        for(int i = 0; i < 10; i++) {
            Genome currentGenome = GA.GetGenome(i);
            double[] chromosome = currentGenome.GetChromosome();
            double fitness = 0.0D;
            for(int j = 0; j < 6; j++) {
                fitness += chromosome[j];
            }
            string chromosomeString = "";
            for(int j = 0; j < 6; j++) {
                chromosomeString += string.Format("{0:0.00}", chromosome[j]);
                if(j != 5) {
                    chromosomeString += ", ";
                }
            }
            Debug.Log(chromosomeString + " - FITNESS: " + string.Format("{0:0.00}", fitness));
            //GA.SetFitness(i, fitness);
        }
        Debug.Log("Evolving");
        GA.Evolve();
        for(int i = 0; i < 10; i++) {
            Genome currentGenome = GA.GetGenome(i);
            double[] chromosome = currentGenome.GetChromosome();
            double fitness = 0.0D;
            if(i < 4) {
                fitness = currentGenome.GetFitness();
            }
            string chromosomeString = "";
            for(int j = 0; j < 6; j++) {
                chromosomeString += string.Format("{0:0.00}", chromosome[j]);
                if(j != 5) {
                    chromosomeString += ", ";
                }
            }
            Debug.Log(chromosomeString + " - FITNESS: " + string.Format("{0:0.00}", fitness));
            //GA.SetFitness(i, fitness);
        }
        Debug.Log("Evolution has occurred");
    }
}
