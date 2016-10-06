using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using System;

public class CreatureController : MonoBehaviour
{
    private NeuralNetwork network;

    private float feedLevel = 1.0f;
    private float age = 0;
    private float health = 1.0f;
    private float breedThreshold = 25;
    private float proximityRadius = 2;
    private int generation = 1;

    private int numInputs = 3;
    private int numOutputs = 6;
    private int numHiddenLayers = 1;
    private int numNeurons = 4;

    private float breedTimer;

    private GameObject creatureParent;

    public event OnCreatureDeath creatureBorn;
    public delegate void OnCreatureDeath();

    public event OnCreatureBorn creatureDeath;
    public delegate void OnCreatureBorn();

    private Material material;

    private Text infoText;

    public void Create()
    {
        network = new NeuralNetwork(numInputs, numOutputs, numHiddenLayers, numNeurons);
        SetLabel();
    }

    public void Create(float[] genome, int generation)
    {
        this.generation = generation + 1;        
        network = new NeuralNetwork(numInputs, numOutputs, numHiddenLayers, numNeurons, genome);
        SetLabel();
    }

    private void SetLabel()
    {
        if (infoText == null)
            infoText = this.GetComponentInChildren<Text>();

        infoText.text = "Gen: " + this.generation;
        infoText.text += "\nFood: " + Math.Round(this.feedLevel, 2);
        infoText.text += "\nHealth: " + Math.Round(this.health, 2);
    }
    // Use this for initialization
    void Start()
    {
        creatureParent = GameObject.Find("Creatures");
        material = this.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (health > 0)
        {
            breedTimer += Time.deltaTime;

            feedLevel -= 0.05f * Time.deltaTime;

            feedLevel = Mathf.Clamp(feedLevel, 0, 10);

            if (feedLevel == 0)
            {
                health -= 0.02f * Time.deltaTime;

                //creatureDeath();
            }
            if (feedLevel > 2f)
            {
                //Vomit
                feedLevel = 0.3f;
                health -= 0.1f;
            }

            if (feedLevel >= 0.7f)
            {
                health += 0.05f * Time.deltaTime;
            }

            health = Mathf.Clamp(health, 0, 1);

            ProcessInput();

            if (health >= 0.8f)
                material.color = Color.green;
            else if (health >= 0.5f)
                material.color = Color.yellow;
            else if (health > 0)
                material.color = Color.red;
            else
                material.color = Color.black;

            SetLabel();
        }
        else
            GameObject.Destroy(this);
    }

    /// <summary>
    /// Output:
    /// 0&1 - rotation
    /// 2&3 - movement
    /// 4 - breed
    /// 5 - eat
    /// 
    /// Input:    
    /// 0 = feedlevel
    /// 1 = health
    /// 2 = proximity (0 = nothing, 1 = food, -1 = other creature)
    /// </summary>
    private void ProcessInput()
    {
        float[] inputs = new float[numInputs];
        inputs[0] = feedLevel;
        inputs[1] = health;
        inputs[2] = GetProximity();

        float[] output = network.GetOutput(inputs);

        transform.Rotate(new Vector3(0, (output[0] - output[1]) * Time.deltaTime * 100, 0));
        transform.Translate(new Vector3(0, 0, (output[2] - output[3]) * Time.deltaTime * 10));

        if (output[4] >= 0.5f)
        {
            Breed();
        }
        if (output[5] >= 0.5f)
        {
            Eat();
        }
    }

    private int GetProximity()
    {
        var colliders = Physics.OverlapSphere(this.transform.position, proximityRadius);
        if (colliders.Any(c => c.tag.Equals("Food")))
            return 1;
        else if (colliders.Any(c => c.tag.Equals("Creature") && c.gameObject != this.gameObject))
        {
            return -1;
        }
        else
            return 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, proximityRadius);        
    }

    private void Eat()
    {       
        var colliders = Physics.OverlapSphere(this.transform.position, proximityRadius);

        var food = colliders.FirstOrDefault(c => c.tag.Equals("Food"));
        if (food != null)
        {
            GameObject.Destroy(food.gameObject);
            feedLevel += 0.2f;
            health += 0.05f;
        }
    }

    private void Breed()
    {
        if (breedTimer >= breedThreshold)
        {
            var genome = network.GetGenome();

            var sibling = Instantiate(this);
            sibling.Create(genome, generation);
            sibling.transform.parent = creatureParent.transform;

            breedTimer = 0;
            feedLevel -= 0.2f;

            //creatureBorn();
        }
    }
}
