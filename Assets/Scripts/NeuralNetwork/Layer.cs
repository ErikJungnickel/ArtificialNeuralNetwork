using UnityEngine;
using System.Collections;

public class Layer {
    public Neuron[] neurons;
    public LayerType layerType;
    private int numNeurons;

    public Layer(int numNeurons, int numInputs, LayerType layerType)
    {
        neurons = new Neuron[numNeurons];
        for (int i = 0; i < neurons.Length; i++)
        {
            neurons[i] = new Neuron(numInputs);
        }
           
        this.layerType = layerType;
        this.numNeurons = numNeurons;
    }

    public float[] Feed(float[] input)
    {
        float[] output = new float[numNeurons];

        int neuronCount = 0;
        foreach (Neuron neuron in neurons)
        {
            float neuronResult = neuron.Feed(input);
            output[neuronCount] = neuronResult;

            neuronCount++;
        }

        return output;
    }
}
