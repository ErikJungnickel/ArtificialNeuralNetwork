using UnityEngine;
using System.Collections;

public class CreatureController : MonoBehaviour
{
    private NeuralNetwork network;

    public float feedLevel; 

    // Use this for initialization
    void Start()
    {
        network = new NeuralNetwork(6, 4, 1, 5);
        feedLevel = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        feedLevel -= 0.02f * Time.deltaTime;

       // if (feedLevel > 0)
        {
            //creating random inputs
            //TODO create real inputs
            float[] inputs = new float[6];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = Random.Range(-1, 1);
            }
            inputs[5] = feedLevel;

            float[] output = network.GetOutput(inputs);

            transform.Rotate(new Vector3(0, (output[0] - output[1]) * Time.deltaTime * 100, 0));
            transform.Translate(new Vector3(0, 0, (output[2] - output[3]) * Time.deltaTime * 10));
        }
    }

    //Input
    //2 Fühler
    //2 inputs pro Fühler
    //Kein Kontakt -> 0,0
    //Kontakt Essen -> 1,1
}
