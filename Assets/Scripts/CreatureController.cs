using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using System;

public class CreatureController : BaseController
{
    [HideInInspector]
    public bool isDead;

    private float proximityRadius = 2;

    private Material material;

    private Text infoText;
    private float labelUpdate = 1;
    private float labelTimer = 1;

    private float tickTimer;
    private float tick = 1;

    private Vector3 lookAt;
    private Vector3 closestFood;
    private Vector3 closestCreature;
    
    void Awake()
    {        
        numInputs = 14;
        numOutputs = 5;
        numHiddenLayers = 1;
        numNeurons = 10;

        feedLevel = 0;

        attacked = false;
        isDead = false;
        material = this.GetComponent<Renderer>().material;        
    }

    // Update is called once per frame
    void Update()
    {
        if (feedLevel > -1.0f && feedLevel < 1.0f)
        {
            //Increase fitness
            tickTimer += Time.deltaTime;
            if (tickTimer >= tick)
            {
                tickTimer = 0;
                fitness += 1 * Time.deltaTime;
            }

            //Decrease feed level
            feedLevel -= 0.05f * Time.deltaTime;

            ProcessInput();

            labelTimer += Time.deltaTime;

            if (labelTimer >= labelUpdate)
            {              
                labelTimer = 0;
            }

            //Kill creature on the edges
            if (transform.position.x >= 225)
            {
                feedLevel = -1;
            }
            if (transform.position.x <= -25)
            {
                feedLevel = -1;
            }
            if (transform.position.z >= 225)
            {
                feedLevel = -1;
            }
            if (transform.position.z <= -25)
            {
                feedLevel = -1;
            }

            //Coloring based on feed level
            if (feedLevel >= -0.2f && feedLevel <= 0.2)
                material.color = Color.green;
            else if (feedLevel >= -0.5f && feedLevel <= 0.5)
                material.color = Color.yellow;
            else if (feedLevel > -1 && feedLevel < 1)
                material.color = Color.red;

            var colliders = Physics.OverlapSphere(this.transform.position, proximityRadius);

            var deathZone = colliders.FirstOrDefault(c => c.tag.Equals("DeathZone"));
            if (deathZone != null)
            {
                feedLevel = -1;
            }
        }
        else
        {
            isDead = true;
            material.color = Color.black;
        }
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
        //TODO sort input params

        float[] inputs = new float[numInputs];

        lookAt = (transform.forward).normalized;

        inputs[0] = lookAt.x;
        inputs[1] = lookAt.y;

        //Closest Death Zone
        var closestDeathZoneGo = GetClosest("DeathZone");
        Vector3 closestDeathZone = (closestDeathZoneGo.transform.position - transform.position).normalized;

        float distToDeathZone = Vector3.Distance(transform.position.normalized, closestDeathZoneGo.transform.position.normalized);    

        inputs[11] = closestDeathZone.x;
        inputs[12] = closestDeathZone.y;
        inputs[13] = distToDeathZone;

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

        output[0] = (2 * output[0]) - 1;
        output[1] = (2 * output[1]) - 1;

        transform.LookAt(transform.position + new Vector3(output[0], 0, output[1]) * Time.deltaTime);
        transform.Translate(new Vector3(0, 0, (output[2] - output[3])) * Time.deltaTime * 10);

        if (output[4] >= 0.5f)
        {
            Attack();
        }

        Eat();
    }

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

            attacked = true;
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

            FoodConsumed();
        }
    }

    protected GameObject GetClosest(string tag)
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

}
