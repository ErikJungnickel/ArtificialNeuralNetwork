using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using System;

public class CreatureController : MonoBehaviour
{
    [HideInInspector]
    public NeuralNetwork network;

    [HideInInspector]
    public float feedLevel;

    [HideInInspector]
    public bool isDead;

    //private float health = 1.0f;
    //private float breedThreshold = 120;
    [HideInInspector]
    public float fitness = 0;

    private float proximityRadius = 2;

    private int numInputs = 11;
    private int numOutputs = 5;
    private int numHiddenLayers = 1;
    private int numNeurons = 8;

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
    private Vector3 closestCreature;

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
        isDead = false;
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
                fitness += 1 * Time.deltaTime;
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
        }
        else
        {
            isDead = true;
            material.color = Color.black;
        }
        //else
        //{
        //    GameObject.Destroy(this.gameObject);
        //}


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

        //Closest Food
        var closestFoodGo = GetClosest("Food");

        closestFood = (closestFoodGo.transform.position - transform.position).normalized;

        inputs[2] = closestFood.x;
        inputs[3] = closestFood.y;

        //Closest Creature
        var closestCreatureGo = GetClosest("Creature");
        if (closestCreatureGo != null)
        {
            closestCreature = (closestCreatureGo.transform.position - transform.position).normalized;

            inputs[8] = closestCreature.x;
            inputs[9] = closestCreature.y;


            float distToCreature = Vector3.Distance(transform.position.normalized, closestCreatureGo.transform.position.normalized);
            inputs[10] = distToCreature;
        }

        float posx = -1 + (2 * (transform.position.x + 25) / 250);
        float posy = -1 + (2 * (transform.position.y + 25) / 250);

        inputs[4] = posx;
        inputs[5] = posy;

        inputs[6] = feedLevel;

        float distToFood = Vector3.Distance(transform.position.normalized, closestFoodGo.transform.position.normalized);

        inputs[7] = distToFood;

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
        if (output[4] >= 0.5f)
        {
            Attack();
        }

        Eat();


        //Breed();
    }

    private GameObject GetClosest(string tag)
    {
        var objects = GameObject.FindGameObjectsWithTag(tag);

        if (objects == null || objects.Length == 0)
        {
            Debug.LogError("No object with tag " + tag + " found");
            return null;
        }

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in objects)
        {
            if (go != this.gameObject)
            {
                if (!tag.Equals("Creature") || !go.GetComponent<CreatureController>().isDead)
                {
                    Vector3 diff = go.transform.position - position;
                    float curDistance = diff.sqrMagnitude;
                    if (curDistance < distance)
                    {
                        closest = go;
                        distance = curDistance;
                    }
                }
            }
        }

        return closest;
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
        //Gizmos.DrawWireSphere(this.transform.position, proximityRadius);

        Gizmos.DrawLine(transform.position, (transform.position + lookAt));
        Gizmos.DrawLine(transform.position, (transform.position + closestFood * 4));
        Gizmos.DrawLine(transform.position, (transform.position + closestCreature * 4));

    }

    private void Attack()
    {
        var colliders = Physics.OverlapSphere(this.transform.position, proximityRadius);

        var creature = colliders.FirstOrDefault(c => c.tag.Equals("Creature") && c.gameObject != this.gameObject
            && !c.gameObject.GetComponent<CreatureController>().isDead );
        if (creature != null)
        {
            CreatureController other = creature.gameObject.GetComponent<CreatureController>();
            other.feedLevel -= 0.1f;
            feedLevel += 0.1f;

            other.feedLevel = Mathf.Clamp(other.feedLevel, -1, 1);
            feedLevel = Mathf.Clamp(feedLevel, -1, 1);
        }               
    }

    private void Eat()
    {
        var colliders = Physics.OverlapSphere(this.transform.position, proximityRadius);

        var food = colliders.FirstOrDefault(c => c.tag.Equals("Food"));
        if (food != null)
        {
            GameObject.Destroy(food.gameObject);

            feedLevel += 0.3f;

            foodConsumed();
        }
        //else
        //    feedLevel -= 0.05f;

        feedLevel = Mathf.Clamp(feedLevel, -1, 1);
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
