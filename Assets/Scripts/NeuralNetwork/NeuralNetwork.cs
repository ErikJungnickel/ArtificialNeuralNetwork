﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NeuralNetwork
{
    private int numHiddenLayers;

    private int numNeurons; //Neurons in hidden layer(s)
    private int numInputs;
    private int numOutputs;

    private float mutationRate = 0.05f;

    private List<Layer> layers;

    public NeuralNetwork(int numInputs, int numOutputs, int numHiddenLayers, int numNeurons)
    {
        this.numInputs = numInputs;
        this.numOutputs = numOutputs;
        this.numHiddenLayers = numHiddenLayers;
        this.numNeurons = numNeurons;

        layers = new List<Layer>();        

        for (int i = 0; i < numHiddenLayers; i++)
        {
            layers.Add(new Layer(numNeurons, numInputs, LayerType.HIDDEN));
        }

        layers.Add(new Layer(numOutputs, numInputs, LayerType.OUTPUT));
    }

    /// <summary>
    /// Initialize the network with the passed genome
    /// </summary>
    /// <param name="numInputs"></param>
    /// <param name="numOutputs"></param>
    /// <param name="numHiddenLayers"></param>
    /// <param name="numNeurons"></param>
    /// <param name="genome"></param>
    public NeuralNetwork(int numInputs, int numOutputs, int numHiddenLayers, int numNeurons, float[] genome)
        : this(numInputs, numOutputs, numHiddenLayers, numNeurons)
    {
        int genomeCounter = 0;

        foreach (Layer layer in layers)
        {
            foreach (Neuron neuron in layer.neurons)
            {
                for (int i = 0; i < neuron.inputWeights.Length; i++)
                {                    
                    float mutation = Random.Range(-mutationRate, mutationRate);
                    neuron.inputWeights[i] = genome[genomeCounter] + mutation;

                    if (neuron.inputWeights[i] > 1)
                        neuron.inputWeights[i] = 1;
                    if(neuron.inputWeights[i] < -1)
                        neuron.inputWeights[i] = -1;

                    genomeCounter++;
                }
            }
        }
    }

    public float[] GetGenome()
    {
        List<float> genome = new List<float>();

        foreach (Layer layer in layers)
        {
            foreach (Neuron neuron in layer.neurons)
            {
                for (int i = 0; i < neuron.inputWeights.Length; i++)
                {
                    genome.Add(neuron.inputWeights[i]);
                }
            }
        }

        return genome.ToArray();
    }

    public float[] GetOutput(float[] input)
    {
        float[] output = new float[numOutputs];

        List<float> inputs = new List<float>(input);

        foreach (Layer layer in layers)
        {
            inputs = layer.Feed(inputs.ToArray()).ToList();
        }

        return inputs.ToArray();
    }
}