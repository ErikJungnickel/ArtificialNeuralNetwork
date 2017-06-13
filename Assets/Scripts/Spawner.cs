using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

public class Spawner : MonoBehaviour
{
    public int numCreatures;
    public int numFoods;
    public int numDeathZones;

    public GameObject Creature;
    public GameObject Food;
    public GameObject Death;

    private int population;
    private int foodCount;

    private GameObject creatureParent;
    private GameObject foodParent;
    private int highestGen = 1;

    private Text globalStats;

    [HideInInspector]
    public List<BaseController> creatures;

    private float genTimer;
    private float genThreshold = 180;

    private float currentGeneration = 1;
    private List<float> avgFitnesses;

    // Use this for initialization
    void Start()
    {
        avgFitnesses = new List<float>();

        creatures = new List<BaseController>();

        globalStats = GetComponentInChildren<Text>();

        creatureParent = new GameObject("Creatures");
        foodParent = new GameObject("Foods");

        population = numCreatures;
        foodCount = numFoods;

        for (int i = 0; i < numCreatures; i++)
        {
            var creature = SpawnCreature();
            creature.GetComponent<BaseController>().Create();
        }

        for (int i = 0; i < numFoods; i++)
        {
            SpawnFood();
        }

        for (int i = 0; i < numDeathZones; i++)
        {
            SpawnDeathZone();
        }

        SetLabel();

        //creatures[0].network.DrawNetwork();
    }

    public void SaveCurrentGeneration()
    {
        string content = currentGeneration + ";";

        foreach (BaseController c in creatures)
        {
            content += string.Join(",", c.GetGenome().Select(f => f.ToString()).ToArray());
            content += ";";
        }

        //System.IO.File.WriteAllText("save " + DateTime.Now.ToString("dd_MM_yyyy HH_mm") + ".ann", content);
        System.IO.File.WriteAllText("save.ann", content);
    }

    public void LoadGeneration()
    {
        var stream = System.IO.File.Open("save.ann", System.IO.FileMode.Open);
        var streamReader = new StreamReader(stream);

        var content = streamReader.ReadToEnd();
        var splitContent = content.Split(';');

        currentGeneration = float.Parse(splitContent[0]);

        var foods = GameObject.FindGameObjectsWithTag("Food");
        for (int i = 0; i < foods.Length; i++)
        {
            Destroy(foods[i]);
        }
        for (int i = 0; i < numFoods; i++)
        {
            SpawnFood();
        }

        var deathZones = GameObject.FindGameObjectsWithTag("DeathZone");
        for (int i = 0; i < deathZones.Length; i++)
        {
            Destroy(deathZones[i]);
        }
        for (int i = 0; i < numDeathZones; i++)
        {
            SpawnDeathZone();
        }

        creatures.ForEach(c => Destroy(c.gameObject));
        creatures.Clear();

        for (int i = 1; i < splitContent.Length; i++)
        {
            if (string.IsNullOrEmpty(splitContent[i]))
            {
                continue;
            }
            var genome_s = splitContent[i].ToString().Split(',');
            var genome = genome_s.Select(s => float.Parse(s)).ToArray();

            var creatureGo = SpawnCreature();

            creatureGo.GetComponent<BaseController>().Create(genome);
        }

        genTimer = 0;
    }

    private GameObject SpawnCreature()
    {
        var creature = Instantiate(Creature, new Vector3(UnityEngine.Random.Range(0, 200), 0, UnityEngine.Random.Range(0, 200)), Creature.transform.rotation);
        ((GameObject)creature).GetComponent<BaseController>().foodConsumed += Spawner_foodConsumed;
        ((GameObject)creature).transform.parent = creatureParent.transform;

        creatures.Add(((GameObject)creature).GetComponent<BaseController>());

        return (GameObject)creature;
    }

    private void SpawnFood()
    {
        var food = Instantiate(Food, new Vector3(UnityEngine.Random.Range(0, 200), 0, UnityEngine.Random.Range(0, 200)), new Quaternion());
        ((GameObject)food).transform.parent = foodParent.transform;
    }

    private void SpawnDeathZone()
    {
        var deathZone = Instantiate(Death, new Vector3(UnityEngine.Random.Range(0, 200), 0, UnityEngine.Random.Range(0, 200)), new Quaternion());
        ((GameObject)deathZone).transform.parent = foodParent.transform;
    }

    void Spawner_foodConsumed()
    {
        foodCount--;
        SpawnFood();
    }

    private void SetLabel()
    {
        globalStats.text = "Gen: " + currentGeneration;
        globalStats.text += "\nTime: " + Math.Round(genThreshold - genTimer, 2);
        globalStats.text += "\nTotal Avg Fitness: " + Math.Round((avgFitnesses.Any() ? Math.Round(avgFitnesses.Average(), 2) : 0), 2);
        globalStats.text += "\nHighest Fitness: " + Math.Round(creatures.Select(c => c.fitness).ToList().Max(), 2);
        globalStats.text += "\nAvg Fitness: " + Math.Round(creatures.Select(c => c.fitness).ToList().Average(), 2);
        globalStats.text += "\nSpeed: " + Time.timeScale;
    }

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
        if (Input.GetKeyUp(KeyCode.N))
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

        var deathZones = GameObject.FindGameObjectsWithTag("DeathZone");
        for (int i = 0; i < deathZones.Length; i++)
        {
            Destroy(deathZones[i]);
        }
        for (int i = 0; i < numDeathZones; i++)
        {
            SpawnDeathZone();
        }

        //get fittest  two creatures
        var orderedCreatures = creatures.OrderByDescending(c => c.fitness).ToList();
        //Get the numCreatures/4 fittest 
        var fittest = new List<BaseController>();
        for (int i = 0; i < numCreatures / 4; i++)
        {
            var creatureToAdd = orderedCreatures[i];
            fittest.Add(orderedCreatures[i]);
        }

        //var father = orderedCreatures[0];
        //var mother = orderedCreatures[1];

        //List<BaseController> fittest = new List<BaseController>();
        //for (int i = 0; i < 2; i++)
        //{
        //    fittest.Add(orderedCreatures[i]);
        //}

        Debug.Log("Gen " + currentGeneration + " : Highest: " + orderedCreatures[0].fitness + " Median: " + Math.Round(creatures.Select(c => c.fitness).ToList().Average(), 2) +
            " Aggro Rate: " + ((float)creatures.Where(c => c.attacked == true).Count() / (float)creatures.Count) * 100.0f + "%");
        avgFitnesses.Add((float)creatures.Select(c => c.fitness).ToList().Average());

        creatures.ForEach(c => Destroy(c.gameObject));
        creatures.Clear();

        int creatureCount = 0;

        do
        {
            var creatureGo = SpawnCreature();

            var father = fittest[UnityEngine.Random.Range(0, fittest.Count)];
            var mother = fittest[UnityEngine.Random.Range(0, fittest.Count)];

            creatureGo.GetComponent<BaseController>().Create(father.GetGenome(), mother.GetGenome());

            creatureCount++;
        } while (creatureCount < numCreatures);

        //    } while (creatureCount < numCreatures);
        //if (!orderedCreatures.Any(c => c.fitness > 0))
        //{
        //    for (int i = 0; i < numCreatures; i++)
        //    {
        //        var creature = SpawnCreature();
        //        creature.GetComponent<BaseController>().Create();
        //    }
        //}
        //else
        //{

        //    do
        //    {
        //        var creatureGo = SpawnCreature();
        //        creatureGo.GetComponent<BaseController>().Create(father.GetGenome(), mother.GetGenome());
        //        creatureCount++;
        //    } while (creatureCount < numCreatures);
        //}

        this.currentGeneration++;
    }
}
