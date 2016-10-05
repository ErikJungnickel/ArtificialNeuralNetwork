using UnityEngine;
using System.Collections;

public class CreatureController : MonoBehaviour
{
    private NeuralNetwork network;

    private float feedLevel = 1.0f;
    private float age = 0;
    private float health = 1.0f;
    private float breedThreshold = 15;
    private bool isDead = false;

    private int numInputs = 2;
    private int numOutputs = 5;
    private int numHiddenLayers = 1;
    private int numNeurons = 3;

    private float breedTimer;

    private GameObject creatureParent;

    public event OnCreatureDeath creatureBorn;
    public delegate void OnCreatureDeath();

    public event OnCreatureBorn creatureDeath;
    public delegate void OnCreatureBorn();

    public void Create()
    {
        network = new NeuralNetwork(numInputs, numOutputs, numHiddenLayers, numNeurons);
    }

    public void Create(float[] genome)
    {
        network = new NeuralNetwork(numInputs, numOutputs, numHiddenLayers, numNeurons, genome);
    }

    // Use this for initialization
    void Start()
    {
        creatureParent = GameObject.Find("Creatures");
    }

    // Update is called once per frame
    void Update()
    {
        if (health > 0)
        {
            breedTimer += Time.deltaTime;

            if (feedLevel > 0)
                feedLevel -= 0.02f * Time.deltaTime;
            else
                feedLevel = 0;

            if (feedLevel == 0)
            {
                if (health > 0)
                    health -= 0.1f * Time.deltaTime;
                else
                {
                    health = 0;
                    //creatureDeath();
                }
            }

            if (feedLevel >= 0.7f)
            {
                if (health < 1)
                    health += 0.05f * Time.deltaTime;
                else
                    health = 1;
            }

            ProcessInput();
        } 
    }

    /// <summary>
    /// Output:
    /// 0&1 - rotation
    /// 2&3 - movement
    /// 4 - breed
    /// 
    /// Input:    
    /// 0 = feedlevel
    /// 1 = health
    /// </summary>
    private void ProcessInput()
    {
        float[] inputs = new float[2];
        inputs[0] = feedLevel;
        inputs[1] = health;

        float[] output = network.GetOutput(inputs);

        transform.Rotate(new Vector3(0, (output[0] - output[1]) * Time.deltaTime * 100, 0));
        transform.Translate(new Vector3(0, 0, (output[2] - output[3]) * Time.deltaTime * 10));

        if (output[4] >= 0.5f)
        {
            Breed();
        }
    }

    private void Breed()
    {
        if (breedTimer >= breedThreshold)
        {
            var genome = network.GetGenome();

            var sibling = Instantiate(this);
            sibling.Create(genome);
            sibling.transform.parent = creatureParent.transform;

            breedTimer = 0;
            feedLevel -= 0.2f;

            //creatureBorn();
        }
    }
}
