﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Spawner : MonoBehaviour {
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

	// Use this for initialization
	void Start () {

        globalStats = GetComponentInChildren<Text>();

        creatureParent = new GameObject("Creatures");
        foodParent = new GameObject("Foods");

        population = numCreatures;
        foodCount = numFoods;

        globalStats.text = "Pop: " + population;

        for (int i = 0; i < numCreatures; i++)
        {
            var creature = SpawnCreature();
            creature.GetComponent<CreatureController>().Create();
        }

        for (int i = 0; i < numFoods; i++)
        {
            SpawnFood();
        }
	}

    private GameObject SpawnCreature()
    {
        var creature = Instantiate(Creature, new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0), Creature.transform.rotation);        
        ((GameObject)creature).GetComponent<CreatureController>().creatureBorn += Spawner_creatureBorn;
        ((GameObject)creature).GetComponent<CreatureController>().creatureDeath += Spawner_creatureDeath;
        ((GameObject)creature).GetComponent<CreatureController>().foodConsumed += Spawner_foodConsumed;
        ((GameObject)creature).transform.parent = creatureParent.transform;

        return (GameObject)creature;
    }

    private void SpawnFood()
    {
        var food = Instantiate(Food, new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0), new Quaternion());
        ((GameObject)food).transform.parent = foodParent.transform;
    }

    void Spawner_foodConsumed()
    {
        foodCount--;
        SpawnFood();
    }

    private void SetLabel()
    {
        globalStats.text = "Pop: " + population;
	globalStats.text += "\nHighest Gen: " + highestGen;
    }
    void Spawner_creatureDeath()
    {
        population--;

        SetLabel();
	
	if(population < numCreatures){
		var creature = SpawnCreature();
        	creature.GetComponent<CreatureController>().Create();
        	population++;

        	SetLabel();
	}
    }

    void Spawner_creatureBorn(float[] genome, int generation)
    {
	if(generation+1 > highestGen){
		highestGen = generation+1;
	}
        var creature = SpawnCreature();
        creature.GetComponent<CreatureController>().Create(genome, generation);
        population++;

        SetLabel();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
