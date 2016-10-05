using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NeuralNetwork
{

    private int numHiddenLayers;

    private int numNeurons; //Neurons in hidden layer(s)
    private int numInputs;
    private int numOutputs;

    private List<Layer> layers;

    public NeuralNetwork(int numInputs, int numOutputs, int numHiddenLayers, int numNeurons)
    {
        this.numInputs = numInputs;
        this.numOutputs = numOutputs;
        this.numHiddenLayers = numHiddenLayers;
        this.numNeurons = numNeurons;

        layers = new List<Layer>();

        //layers.Add(new Layer(numInputs, LayerType.INPUT));

        for (int i = 0; i < numHiddenLayers; i++)
        {
            layers.Add(new Layer(numNeurons, numInputs, LayerType.HIDDEN));
        }

        layers.Add(new Layer(numOutputs, numInputs, LayerType.OUTPUT));
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
