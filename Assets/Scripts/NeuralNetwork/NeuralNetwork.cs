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
            if (i == 0)
                layers.Add(new Layer(numNeurons, numInputs, LayerType.HIDDEN));
            else
                layers.Add(new Layer(numNeurons, numNeurons, LayerType.HIDDEN));
        }

        layers.Add(new Layer(numOutputs, numNeurons, LayerType.OUTPUT));
    }

    /// <summary>
    /// Initialize the network with the passed genome
    /// </summary>
    /// <param name="numInputs"></param>
    /// <param name="numOutputs"></param>
    /// <param name="numHiddenLayers"></param>
    /// <param name="numNeurons"></param>
    /// <param name="genomeFather"></param>
    /// <param name="genomeMother"></param>
    public NeuralNetwork(int numInputs, int numOutputs, int numHiddenLayers, int numNeurons, float[] genomeFather, float[] genomeMother)
        : this(numInputs, numOutputs, numHiddenLayers, numNeurons)
    {
        int genomeCounter = 0;

        foreach (Layer layer in layers)
        {
            foreach (Neuron neuron in layer.neurons)
            {
                for (int i = 0; i < neuron.inputWeights.Length; i++)
                {
                    float genome = Random.value >= 0.5f ? genomeFather[genomeCounter] : genomeMother[genomeCounter];

                    neuron.inputWeights[i] = genome;

                    if (Random.value >= 0.5f)
                    {
                        float mutation = Random.Range(-mutationRate, mutationRate);

                        neuron.inputWeights[i] += mutation;

                        neuron.inputWeights[i] = Mathf.Clamp(neuron.inputWeights[i], -1, 1);
                    }

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

    public void DrawNetwork()
    {
        float x = 275;
        float y = 100;
        float scale = 0.5f;

        float spaceBetweenLayers = 50;

        for (int i = 0; i < numInputs; i++)
        {
            CreateNeuron(x, y + (i * 20 * scale), scale);
        }

        x += spaceBetweenLayers * scale;

        int neuronCount = 0;
        foreach (Layer layer in layers)
        {            
            foreach(Neuron neuron in layer.neurons){
                CreateNeuron(x, y + (neuronCount * 20 * scale), scale);
                neuronCount++;
            }

            x += spaceBetweenLayers * scale;
            neuronCount = 0;
        }
    }

    private void CreateNeuron(float posX, float posY, float scale)
    {
        GameObject plane = new GameObject("Plane");
        plane.transform.Rotate(new Vector3(1, 0, 0), 90);
        plane.transform.position = new Vector3(posX, 0, posY);

        MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = CreateMesh(5 * scale, 5 * scale );
        MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material.shader = Shader.Find("Particles/Additive");
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.green);
        tex.Apply();
        renderer.material.mainTexture = tex;
        renderer.material.color = Color.green;
    }

    private Mesh CreateMesh(float width, float height)
    {
        Mesh m = new Mesh();
        m.name = "ScriptedMesh";
        m.vertices = new Vector3[] {
         new Vector3(-width, -height, 0.01f),
         new Vector3(width, -height, 0.01f),
         new Vector3(width, height, 0.01f),
         new Vector3(-width, height, 0.01f)
     };
        m.uv = new Vector2[] {
         new Vector2 (0, 0),
         new Vector2 (0, 1),
         new Vector2(1, 1),
         new Vector2 (1, 0)
     };
        m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateNormals();

        return m;
    }
}
