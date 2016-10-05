using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
    public int numCreatures;
    public int numFoods;

    public GameObject Creature;
    public GameObject Food;

	// Use this for initialization
	void Start () {

        for (int i = 0; i < numCreatures; i++)
        {
            Instantiate(Creature);
        }

        for (int i = 0; i < numFoods; i++)
        {
            Instantiate(Food, new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0), new Quaternion());
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
