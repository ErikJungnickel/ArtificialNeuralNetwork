using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using System;

public class CreatureController : MonoBehaviour
{
    private NeuralNetwork network;

    [HideInInspector]
    public float feedLevel;

    //private float health = 1.0f;
    //private float breedThreshold = 120;
    [HideInInspector]
    public int fitness = 0;

    private float proximityRadius = 2;

    private int numInputs = 7;
    private int numOutputs = 4;
    private int numHiddenLayers = 1;
    private int numNeurons = 6;

    //private float breedTimer;

    private GameObject creatureParent;

    //public event OnCreatureBorn creatureBorn;
    //public delegate void OnCreatureBorn(float[] genome, int generation);

    //public event OnCreatureDeath creatureDeath;
    //public delegate void OnCreatureDeath(GameObject go);

    public event OnFoodConsumed foodConsumed;
    public delegate void OnFoodConsumed();

    private Material material;

    private Text infoText;
    private float labelUpdate = 1;
    private float labelTimer = 1;

    private float tickTimer;
    private float tick = 1;

    private Vector3 lookAt;
    private Vector3 closestFood;

    public void Create()
    {
        network = new NeuralNetwork(numInputs, numOutputs, numHiddenLayers, numNeurons);
        SetLabel();
    }

    public void Create(float[] genomeFather, float[] genomeMother)
    {
        network = new NeuralNetwork(numInputs, numOutputs, numHiddenLayers, numNeurons, genomeFather, genomeMother);
        SetLabel();
    }

    private void SetLabel()
    {
        //if (infoText == null)
        //    infoText = this.GetComponentInChildren<Text>();

        //infoText.text = "Fitness " + this.fitness;
        //infoText.text += "\nFeed " + Math.Round(this.feedLevel, 2);
    }

    // Use this for initialization
    void Start()
    {
        feedLevel = 0;
        creatureParent = GameObject.Find("Creatures");
        material = this.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {        
        if (feedLevel > -1.0f && feedLevel < 1.0f)
        {
            
            tickTimer += Time.deltaTime;
            if (tickTimer >= tick)
            {
                tickTimer = 0;
                fitness++;
            }

            feedLevel -= 0.05f * Time.deltaTime;

            feedLevel = Mathf.Clamp(feedLevel, -1, 1);

            ProcessInput();

            labelTimer += Time.deltaTime;

            if (labelTimer >= labelUpdate)
            {
                SetLabel();
                labelTimer = 0;
            }

            if (transform.position.x >= 225)
            {
                //transform.position = new Vector3(-49, transform.position.y, transform.position.z);
                feedLevel = -1;
            }
            if (transform.position.x <= -25)
            {
                //transform.position = new Vector3(249, transform.position.y, transform.position.z);
                feedLevel = -1;
            }
            if (transform.position.z >= 225)
            {
                //transform.position = new Vector3(transform.position.x, transform.position.y, -49);
                feedLevel = -1;
            }
            if (transform.position.z <= -25)
            {
                feedLevel = -1;
                //transform.position = new Vector3(transform.position.x, transform.position.y, 249);
            }

            if (feedLevel >= -0.2f && feedLevel <= 0.2)
                material.color = Color.green;
            else if (feedLevel >= -0.5f && feedLevel <= 0.5)
                material.color = Color.yellow;
            else if (feedLevel > -1 && feedLevel < 1)
                material.color = Color.red;
            else
                material.color = Color.black;
        }


        //}
        //else
        //{
        //    creatureDeath(this.gameObject);
        //}
    }

    /// <summary>
    /// Output:
    /// 0&1 - lookat
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
        //inputs[0] = feedLevel;
        //inputs[1] = health;

        lookAt = (transform.forward).normalized;

        inputs[0] = lookAt.x;
        inputs[1] = lookAt.y;

        //inputs[2] = GetProximity();
        var foods = GameObject.FindGameObjectsWithTag("Food");

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in foods)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        closestFood = (closest.transform.position - position).normalized;

        inputs[2] = closestFood.x;
        inputs[3] = closestFood.y;

        //float dist = Vector3.Distance(transform.position, closest.transform.position);
        //float posx = (transform.position.x - (-1)) / (300 - (-1));
        //float posy = (transform.position.y - (-1)) / (300 - (-1));

        float posx = -1 + (2 * (transform.position.x + 25) / 250);
        float posy = -1 + (2 * (transform.position.y + 25) / 250);

        inputs[4] = posx;
        inputs[5] = posy;

        inputs[6] = feedLevel;

        float[] output = network.GetOutput(inputs);

        //transform.Rotate(new Vector3(0, (output[0] - output[1]), 0) * Time.deltaTime * 10);

        output[0] = (2 * output[0]) - 1;
        output[1] = (2 * output[1]) - 1;

        transform.LookAt(transform.position + new Vector3(output[0], 0, output[1]) * Time.deltaTime);
        transform.Translate(new Vector3(0, 0, (output[2] - output[3])) * Time.deltaTime * 10);

        //if (output[4] >= 0.5f)
        //{
        //    Breed();
        //}
        //if (output[5] >= 0.5f)
        //{
        //    Eat();
        //}
        Eat();
        //Breed();
    }


    //private int GetProximity()
    //{
    //    var colliders = Physics.OverlapSphere(this.transform.position, proximityRadius);
    //    if (colliders.Any(c => c.tag.Equals("Food")))
    //        return 1;
    //    else if (colliders.Any(c => c.tag.Equals("Creature") && c.gameObject != this.gameObject))
    //    {
    //        return -1;
    //    }
    //    else
    //        return 0;
    //}

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, proximityRadius);

        Gizmos.DrawLine(transform.position, (transform.position + lookAt));
        Gizmos.DrawLine(transform.position, (transform.position + closestFood));
    }

    private void Eat()
    {
        var colliders = Physics.OverlapSphere(this.transform.position, proximityRadius);

        var food = colliders.FirstOrDefault(c => c.tag.Equals("Food"));
        if (food != null)
        {
            GameObject.Destroy(food.gameObject);
            //fitness++;
            feedLevel += 0.3f;
            //health += 0.1f;

            feedLevel = Mathf.Clamp(feedLevel, -1, 1);
            //health = Mathf.Clamp(health, 0, 1);

            foodConsumed();
        }
        //else
        //    health -= 0.05f;
    }

    public float[] GetGenome()
    {
        return network.GetGenome();
    }

    //private void Breed()
    //{
    //    if (breedTimer >= breedThreshold)
    //    {
    //        var genome = network.GetGenome();

    //        breedTimer = 0;
    //        //feedLevel -= 0.2f;

    //        //feedLevel = Mathf.Clamp(feedLevel, 0, 1);

    //        creatureBorn(genome, generation);
    //    }
    //}
}
