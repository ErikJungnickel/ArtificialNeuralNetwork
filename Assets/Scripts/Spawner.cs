using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
    public int numCreatures;    
    public int numFoods;

    public GameObject Creature;
    public GameObject Food;

    private int population;

    private GameObject creatureParent;

	// Use this for initialization
	void Start () {

        creatureParent = new GameObject("Creatures");
        var foods = new GameObject("Foods");
        population = numCreatures;

        for (int i = 0; i < numCreatures; i++)
        {
            SpawnCreature();
        }

        //for (int i = 0; i < numFoods; i++)
        //{
        //    var food = Instantiate(Food, new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0), new Quaternion());
        //    ((GameObject)food).transform.parent = foods.transform;
        //}
	}

    private void SpawnCreature()
    {
        var creature = Instantiate(Creature, new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0), Creature.transform.rotation);
        ((GameObject)creature).GetComponent<CreatureController>().Create();
        ((GameObject)creature).GetComponent<CreatureController>().creatureBorn += Spawner_creatureBorn;
        ((GameObject)creature).GetComponent<CreatureController>().creatureDeath += Spawner_creatureDeath;
        ((GameObject)creature).transform.parent = creatureParent.transform;
    }

    void Spawner_creatureDeath()
    {
        population--;
        Debug.Log("Population: " + population);
    }

    void Spawner_creatureBorn()
    {
        population++;
        Debug.Log("Population: " + population);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
