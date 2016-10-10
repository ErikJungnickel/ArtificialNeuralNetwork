using UnityEngine;
using System.Collections;
using System;

public class Neuron
{
    public float[] inputWeights;

    public Neuron(int numInputs)
    {
        inputWeights = new float[numInputs + 1]; //+1 for the bias
        CreateRandomWeights();
    }

    private void CreateRandomWeights()
    {
        for (int i = 0; i < inputWeights.Length; i++)
        {
            inputWeights[i] = UnityEngine.Random.Range(-1.0f, 1.0f);
        }
    }

    public float Feed(float[] input)
    {
        float result = 0;
        for (int i = 0; i < input.Length; i++)
        {
            result += input[i] * inputWeights[i];
        }

        //bias
        result += inputWeights[inputWeights.Length-1];

        return Sigmoid(result);
    }

    private float Sigmoid(float x)
    {
        return (float)(2 / (1 + Math.Exp(-2 * x)) - 1); //this seems to be work much better
        //return (float)(1 / (1 + Math.Exp(-x / 1)));
    }
}
