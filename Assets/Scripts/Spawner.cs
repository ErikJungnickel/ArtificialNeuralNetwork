using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class Spawner : MonoBehaviour
{
    public int numCreatures;
    public int numFoods;

    public GameObject Creature;
    public GameObject Food;

    private int population;
    private int foodCount;

    private GameObject creatureParent;
    private GameObject foodParent;
    private int highestGen = 1;

    private Text globalStats;

    [HideInInspector]
    public List<CreatureController> creatures;

    private float genTimer;
    private float genThreshold = 180;

    private float currentGeneration = 1;
    private List<float> avgFitnesses;

    // Use this for initialization
    void Start()
    {
        avgFitnesses = new List<float>();

        creatures = new List<CreatureController>();

        globalStats = GetComponentInChildren<Text>();

        creatureParent = new GameObject("Creatures");
        foodParent = new GameObject("Foods");

        population = numCreatures;
        foodCount = numFoods;

        for (int i = 0; i < numCreatures; i++)
        {
            var creature = SpawnCreature();
            creature.GetComponent<CreatureController>().Create();
        }

        for (int i = 0; i < numFoods; i++)
        {
            SpawnFood();
        }

        SetLabel();
    }

    private GameObject SpawnCreature()
    {
        var creature = Instantiate(Creature, new Vector3(UnityEngine.Random.Range(0, 200), 0, UnityEngine.Random.Range(0, 200)), Creature.transform.rotation);
        //((GameObject)creature).GetComponent<CreatureController>().creatureBorn += Spawner_creatureBorn;
        //((GameObject)creature).GetComponent<CreatureController>().creatureDeath += Spawner_creatureDeath;
        ((GameObject)creature).GetComponent<CreatureController>().foodConsumed += Spawner_foodConsumed;
        ((GameObject)creature).transform.parent = creatureParent.transform;

        creatures.Add(((GameObject)creature).GetComponent<CreatureController>());

        return (GameObject)creature;
    }

    private void SpawnFood()
    {
        var food = Instantiate(Food, new Vector3(UnityEngine.Random.Range(0, 200), 0, UnityEngine.Random.Range(0, 200)), new Quaternion());
        ((GameObject)food).transform.parent = foodParent.transform;
    }

    void Spawner_foodConsumed()
    {
        foodCount--;
        SpawnFood();
    }

    private void SetLabel()
    {
        globalStats.text = "Gen: " + currentGeneration;
        globalStats.text += "\nTotal Avg Fitness: " + (avgFitnesses.Any() ? Math.Round(avgFitnesses.Average(), 2) : 0);
        globalStats.text += "\nHighest Fitness: " + creatures.Select(c => c.fitness).ToList().Max();
        globalStats.text += "\nAvg Fitness: " + Math.Round(creatures.Select(c => c.fitness).ToList().Average(), 2);
        globalStats.text += "\nSpeed: " + Time.timeScale;

        //globalStats.text = "Pop: " + population;
        //globalStats.text += "\nHighest Gen: " + highestGen;
        //globalStats.text += "\nHighest alive: " + creatures.Select(c => c.generation).ToList().Max();
        //globalStats.text += "\nMedian Gen: " + Math.Round(creatures.Select(c => c.generation).ToList().Average(), 2);
    }

    //void Spawner_creatureDeath(GameObject go)
    //{
    //    creatures.Remove(go.GetComponent<CreatureController>());

    //    GameObject.Destroy(go);
    //    population--;

    //    SetLabel();

    //    if (population < numCreatures)
    //    {
    //        var creature = SpawnCreature();
    //        creature.GetComponent<CreatureController>().Create();
    //        population++;

    //        SetLabel();
    //    }
    //}

    //void Spawner_creatureBorn(float[] genome, int generation)
    //{
    //    if (generation + 1 > highestGen)
    //    {
    //        highestGen = generation + 1;
    //    }
    //    var creature = SpawnCreature();
    //    creature.GetComponent<CreatureController>().Create(genome, generation);
    //    population++;

    //    SetLabel();
    //}

    // Update is called once per frame
    void Update()
    {
        SetLabel();

        genTimer += Time.deltaTime;
        if (!creatures.Any(c => c.feedLevel > -1 && c.feedLevel < 1))
        {
            CreateNewGeneration();
            genTimer = 0;
            return;
        }
        if (genTimer >= genThreshold)
        {
            CreateNewGeneration();
            genTimer = 0;
        }
    }

    private void CreateNewGeneration()
    {
        var foods = GameObject.FindGameObjectsWithTag("Food");
        for (int i = 0; i < foods.Length; i++)
        {
            Destroy(foods[i]);
        }
        for (int i = 0; i < numFoods; i++)
        {
            SpawnFood();
        }

        //get fittest  two creatures
        var orderedCreatures = creatures.OrderByDescending(c => c.fitness).ToList();
        //Get the numCreatures/4 fittest 
        var fittest = new List<CreatureController>();
        for (int i = 0; i < numCreatures / 4; i++)
        {
            var creatureToAdd = orderedCreatures[i];
            fittest.Add(orderedCreatures[i]);
        }

        //var father = orderedCreatures[0];
        //var mother = orderedCreatures[1];

        //List<CreatureController> fittest = new List<CreatureController>();
        //for (int i = 0; i < 2; i++)
        //{
        //    fittest.Add(orderedCreatures[i]);
        //}

        Debug.Log("Gen " + currentGeneration + " : Highest: " + orderedCreatures[0].fitness + " Median: " + Math.Round(creatures.Select(c => c.fitness).ToList().Average(), 2));
        avgFitnesses.Add((float)creatures.Select(c => c.fitness).ToList().Average());

        creatures.ForEach(c => Destroy(c.gameObject));
        creatures.Clear();

        int creatureCount = 0;

        do
        {
            var creatureGo = SpawnCreature();

            var father = fittest[UnityEngine.Random.Range(0, fittest.Count)];
            var mother = fittest[UnityEngine.Random.Range(0, fittest.Count)];

            creatureGo.GetComponent<CreatureController>().Create(father.GetGenome(), mother.GetGenome());

            creatureCount++;
        } while (creatureCount < numCreatures);

        //    } while (creatureCount < numCreatures);
        //if (!orderedCreatures.Any(c => c.fitness > 0))
        //{
        //    for (int i = 0; i < numCreatures; i++)
        //    {
        //        var creature = SpawnCreature();
        //        creature.GetComponent<CreatureController>().Create();
        //    }
        //}
        //else
        //{

        //    do
        //    {
        //        var creatureGo = SpawnCreature();
        //        creatureGo.GetComponent<CreatureController>().Create(father.GetGenome(), mother.GetGenome());
        //        creatureCount++;
        //    } while (creatureCount < numCreatures);
        //}

        this.currentGeneration++;
    }
}
