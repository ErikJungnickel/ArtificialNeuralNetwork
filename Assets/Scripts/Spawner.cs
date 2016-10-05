using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
    public int numCreatures;    
    public int numFoods;

    public GameObject Creature;
    public GameObject Food;

	// Use this for initialization
	void Start () {

        var creatures = new GameObject("Creatures");
        var foods = new GameObject("Foods");

        for (int i = 0; i < numCreatures; i++)
        {
            var creature = Instantiate(Creature);
            creature.GetComponent<CreatureController>().Create();
            creature.transform.parent = creatures.transform;
        }

        for (int i = 0; i < numFoods; i++)
        {
            var food = Instantiate(Food, new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0), new Quaternion());
            ((GameObject)food).transform.parent = foods.transform;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
