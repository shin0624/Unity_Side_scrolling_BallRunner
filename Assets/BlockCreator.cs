using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCreator : MonoBehaviour
{
    public GameObject[] blockPrefabs;//블록을 저장할 배열
    private int block_count = 0; //생성한 블록 개수
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    public void createBlock(Vector3 block_position)
    {
        //만들어야 할 블록의 타입(흰색, 빨간색)을 구한다
        int next_block_type = this.block_count % this.blockPrefabs.Length;

        //블록을 생성하고 go에 보관
        GameObject go = GameObject.Instantiate(this.blockPrefabs[next_block_type]) as GameObject;

        go.transform.position = block_position;//블록의 위치를 이동
        this.block_count++;//블록 개수 증가
    }
}
