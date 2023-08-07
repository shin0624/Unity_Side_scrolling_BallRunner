using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeScript : MonoBehaviour
{
    public GameObject treePrefabs;
    private int tree_count = 0; 
   // public int  obj = GetComponent<MapCreator>
    void Start()
    {
        for(int i=0; i < 10; i++) 
        {
            Instantiate(treePrefabs, new Vector3(transform.position.x + 2f * i, transform.position.y, transform.position.z), Quaternion.identity);
        }
    }

    
    void Update()
    {
       
    }

   
}
