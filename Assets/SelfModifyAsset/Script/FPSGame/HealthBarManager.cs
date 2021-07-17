using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager: MonoBehaviour
{
    public GameObject healthPoint;
    public GameObject player;

    private void Start()
    {
        int health = player.GetComponent<PlayerManager>().health;
        int j = 0;
        for(int i = 0; i < health; i++)
        {
            GameObject obj = Instantiate(healthPoint,transform);
            obj.transform.position += new Vector3(j,0,0);
            j += 35;
        }

    }

    public void decreaseHealthPoint()
    {
        int totalChild = gameObject.transform.childCount;

        if(totalChild > 0)
        {
            Transform lastHealthPoint = gameObject.transform.GetChild(totalChild - 1);
            DestroyImmediate(lastHealthPoint.gameObject);
            player.GetComponent<PlayerManager>().decreaseHealth();
        }

    }
}
