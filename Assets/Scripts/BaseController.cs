using UnityEngine;
using System.Collections;

public class BaseController : MonoBehaviour
{
    protected NeuralNetwork network;

    protected int numInputs;
    protected int numOutputs;
    protected int numHiddenLayers;
    protected int numNeurons;

    [HideInInspector]
    public float fitness;
    [HideInInspector]
    public float feedLevel;
    [HideInInspector]
    public bool attacked;


    public event OnFoodConsumed foodConsumed;
    public delegate void OnFoodConsumed();

    public void Create()
    {        
        network = new NeuralNetwork(numInputs, numOutputs, numHiddenLayers, numNeurons);     
    }

    public void Create(float[] genomeFather, float[] genomeMother)
    {        
        network = new NeuralNetwork(numInputs, numOutputs, numHiddenLayers, numNeurons, genomeFather, genomeMother);        
    }

    public float[] GetGenome()
    {
        return network.GetGenome();
    }

    protected void FoodConsumed()
    {
        foodConsumed();
    }


}
