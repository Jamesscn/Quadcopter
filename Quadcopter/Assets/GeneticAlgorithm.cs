using System;
using static StaticFunctions;

public class Genome : IComparable {

    static Random RandomGenerator;
    static double MinValue, MaxValue;
    static int Genes;

    double[] Chromosome;
    float[] Data;
    float Fitness;

    public Genome(int genes, double min, double max, Random randomGenerator) {
        RandomGenerator = randomGenerator;
        Genes = genes;
        MinValue = min;
        MaxValue = max;
        Fitness = 0.0F;
        Data = new float[7];
        Chromosome = new double[Genes];
        for(int i = 0; i < genes; i++) {
            Chromosome[i] = GetRandomAlelle();
        }
    }

    public Genome() {
        Fitness = 0.0F;
        Data = new float[7];
        Chromosome = new double[Genes];
    }

    public int GetGenes() {
        return Chromosome.Length;
    }

    public double[] GetChromosome() {
        return Chromosome;
    }

    public void SetAlelle(int gene, double alelle) {
        Chromosome[gene] = alelle;
    }

    public void AddFitness(float fitness) {
        Fitness += fitness;
    }

    public void ResetFitness() {
        Fitness = 0.0F;
    }

    public float GetFitness() {
        return Fitness;
    }

    public void AggregateData(float[] data) {
        for(int i = 0; i < 7; i++) {
            Data[i] += data[i];
        }
    }

    public void ResetData() {
        for(int i = 0; i < 7; i++) {
            Data[i] = 0.0F;
        }
    }

    public float[] GetData() {
        return Data;
    }

    public int CompareTo(object obj) {
        Genome other = obj as Genome;
        if(Fitness > other.GetFitness()) {
            return -1;
        }
        return 1;
    }

    public static double GetRandomAlelle() {
        return (MaxValue - MinValue) * RandomGenerator.NextDouble() + MinValue;
    }

    public static Tuple<Genome, Genome> Crossover(Genome firstParent, Genome secondParent) {
        Genome firstChild = new Genome();
        Genome secondChild = new Genome();
        for(int i = 0; i < firstParent.GetGenes(); i++) {
            if(RandomGenerator.NextDouble() < 0.5D) {
                firstChild.SetAlelle(i, firstParent.GetChromosome()[i]);
                secondChild.SetAlelle(i, secondParent.GetChromosome()[i]);
            } else {
                firstChild.SetAlelle(i, secondParent.GetChromosome()[i]);
                secondChild.SetAlelle(i, firstParent.GetChromosome()[i]);
            }
        }
        return new Tuple<Genome, Genome>(firstChild, secondChild);
    }

    public static Genome MutateAdd(Genome original, double mutationProbability) {
        Genome mutated = new Genome();
        for(int i = 0; i < original.GetGenes(); i++) {
            if(RandomGenerator.NextDouble() < mutationProbability) {
                double shiftAmount = (MaxValue - MinValue) * 0.1D * (RandomGenerator.NextDouble() - 0.5D);
                double newAlelle = original.GetChromosome()[i] + shiftAmount;
                newAlelle = Clamp(newAlelle, MinValue, MaxValue);
                mutated.SetAlelle(i, newAlelle);
            } else {
                mutated.SetAlelle(i, original.GetChromosome()[i]);
            }
        }
        return mutated;
    }

    public static Genome MutateReplace(Genome original, double mutationProbability) {
        Genome mutated = new Genome();
        for(int i = 0; i < original.GetGenes(); i++) {
            if(RandomGenerator.NextDouble() < mutationProbability) {
                mutated.SetAlelle(i, GetRandomAlelle());
            } else {
                mutated.SetAlelle(i, original.GetChromosome()[i]);
            }
        }
        return mutated;
    }

}

public class GeneticAlgorithm {

    static Random RandomGenerator;
    int PopulationSize, GenesPerChromosome;
    double SurvivalProportion, MutationProbability;
    Genome[] Population;

    public GeneticAlgorithm(int populationSize, int genesPerChromosome, double minValue, double maxValue, double survivalProportion, double mutationProbability) {
        RandomGenerator = new Random();
        PopulationSize = populationSize;
        GenesPerChromosome = genesPerChromosome;
        SurvivalProportion = survivalProportion;
        MutationProbability = mutationProbability;

        Population = new Genome[populationSize];
        for(int i = 0; i < PopulationSize; i++) {
            Population[i] = new Genome(genesPerChromosome, minValue, maxValue, RandomGenerator);
        }
    }

    public Genome GetGenome(int index) {
        return Population[index];
    }

    public void AddFitness(int index, float fitness) {
        Population[index].AddFitness(fitness);
    }

    public void AggregateData(int index, float[] data) {
        Population[index].AggregateData(data);
    }

    Tuple<Genome, Genome> SelectRandomParents(double[] selectionProbabilities) {
        double firstParentRandom = RandomGenerator.NextDouble();
        double secondParentRandom = RandomGenerator.NextDouble();
        Genome firstParent = Population[PopulationSize - 1];
        Genome secondParent = Population[PopulationSize - 1];
        for(int i = 0; i < PopulationSize; i++) {
            if(firstParentRandom <= selectionProbabilities[i]) {
                firstParent = Population[i];
                break;
            }
        }
        for(int i = 0; i < PopulationSize; i++) {
            if(secondParentRandom <= selectionProbabilities[i]) {
                secondParent = Population[i];
                break;
            }
        }
        return new Tuple<Genome, Genome>(firstParent, secondParent);
    }

    public Tuple<float, float[]> Evolve() {
        Array.Sort(Population);
        float bestFitness = Population[0].GetFitness();
        float[] bestData = new float[7];
        for(int i = 0; i < 7; i++) {
            bestData[i] = Population[0].GetData()[i];
        }
        Tuple<float, float[]> returnData = new Tuple<float, float[]>(bestFitness, bestData);
        Genome[] NextGeneration = new Genome[PopulationSize];
        int currentPopulationSize = 0;
        for(int i = 0; i < PopulationSize * SurvivalProportion; i++) {
            NextGeneration[currentPopulationSize] = Population[i];
            currentPopulationSize++;
        }
        double[] cumulativeFitnesses = new double[PopulationSize];
        //This may work incorrectly for negative fitnesses
        double sumOfFitnesses = 0.0D;
        for(int i = 0; i < PopulationSize; i++) {
            sumOfFitnesses += Population[i].GetFitness();
        }
        double lastCumulativeFitness = 0.0D;
        for(int i = 0; i < PopulationSize; i++) {
            cumulativeFitnesses[i] = lastCumulativeFitness + Population[i].GetFitness() / sumOfFitnesses;
            lastCumulativeFitness = cumulativeFitnesses[i];
        }
        while(currentPopulationSize != PopulationSize) {
            Tuple<Genome, Genome> parents = SelectRandomParents(cumulativeFitnesses);
            Tuple<Genome, Genome> children = Genome.Crossover(parents.Item1, parents.Item2);
            Genome firstChild = Genome.MutateAdd(children.Item1, MutationProbability);
            Genome secondChild = Genome.MutateAdd(children.Item2, MutationProbability);
            firstChild = Genome.MutateReplace(firstChild, MutationProbability);
            secondChild = Genome.MutateReplace(secondChild, MutationProbability);
            NextGeneration[currentPopulationSize] = firstChild;
            currentPopulationSize++;
            if(currentPopulationSize == PopulationSize) {
                break;
            }
            NextGeneration[currentPopulationSize] = secondChild;
            currentPopulationSize++;
        }
        Population = NextGeneration;
        for(int i = 0; i < PopulationSize; i++) {
            Population[i].ResetFitness();
            Population[i].ResetData();
        }
        return returnData;
    }

    public Genome GetFittest() {
        return Population[0];
    }

}
