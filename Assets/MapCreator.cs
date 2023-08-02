using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{ 
    public enum TYPE //블록 종류
    {
        None = -1, //없음
        FLOOR = 0,//바닥
        HOLE,//구멍
        NUM,//블록 종류 몇 종류?(2)
    };

};


public class MapCreator : MonoBehaviour
    { private GameRoot game_root = null;

    public TextAsset Level_Data_text = null;

    public static float BLOCK_WIDTH = 1.0f;//폭
    public static float BLOCK_HEIGHT = 0.2f;//높이
    public static int BLOCK_NUM_IN_SCREEN = 24;//화면 내 들어가는 블록 개수

    private LevelControl level_control = null; //레벨 컨트롤과 연계되는 변수

    private struct FloorBlock//블록에 관한 정보를 모아서 관리하는 구조체(변수의 폴더와 같음)
    {
        public bool is_created;//블록생성유무
        public Vector3 position;//블록위치
    };

    private FloorBlock last_block;//마지막에 생성한 블록
    private PlayerControl player = null;//scene 상의 player를 보관
    private BlockCreator block_creator;//BlockCreator 를 보관

    void Start()
    {
        this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        this.last_block.is_created = false;
        this.block_creator = this.gameObject.GetComponent<BlockCreator>();

        this.level_control = new LevelControl();
        this.level_control.initialize();

        this.level_control.loadLevelData(this.Level_Data_text);//LevelControl.cs에 있는 loadLevelData()메서드를 호출.

    this.game_root = this.gameObject.GetComponent<GameRoot>();//게임루트 스크립트 할당
        this.player.level_control = this.level_control;
    }

    
    void Update()
    {
        float block_generate_x = this.player.transform.position.x;//플레이어의 x위치
        block_generate_x += BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN + 1) / 2.0f;//대략 반 화면만큼 오른쪽으로 이동, 이 위치가 블록 생성 문턱값

        //마지막에 만든 블록 위치가 문턱값 이하일 때-->블록을 만들도록
        while (this.last_block.position.x < block_generate_x)
        {
            this.create_floor_block();
        }
    }

    private void create_floor_block()
    {
        Vector3 block_position;//이제부터 만들 블록의 위치
        if(!this.last_block.is_created)//last_block이 생성되지 않은 경우
        {
            block_position = this.player.transform.position;//블록의 위치를 player와 같게
            block_position.x -= BLOCK_WIDTH * ((float)BLOCK_NUM_IN_SCREEN / 2.0f);//블록의 x 위치를 화면 절반만큼 왼쪽으로 이동
            block_position.y = 0.0f;//블록의 y 위치는 0
        }
        else//last_block이 생성된 경우
        {
            block_position = this.last_block.position;//이번에 만들 블록의 위치를 직전에 만든 블록과 같게
        }
        block_position.x += BLOCK_WIDTH;//블록을 1블록만큼 오른쪽으로 이동

    //BlockCreator 스크립트의 crateBlock()에 생성를 지시 --> 이제까지의 코드에서 설정한 block_position을 건네준다
    //this.block_creator.createBlock(block_position);

    //this.level_control.update();//레벨컨트롤 갱신
        this.level_control.update(this.game_root.getPlayTime());
        block_position.y = level_control.current_block.height * BLOCK_HEIGHT;//지금 만들 블록 정보의 높이를 scene상의 좌표로 변환
        LevelControl.CreationInfo current = this.level_control.current_block;//지금 만들 블록에 관한 정보를 커런트 변수에 넣는다
        if(current.block_type == Block.TYPE.FLOOR)
        {
            this.block_creator.createBlock(block_position);//지금 만들 블록이 바닥일 경우실제 위치에 블록 생성
        }

        this.last_block.position = block_position;//last_block의 위치를 이번 위치로 갱신
        this.last_block.is_created = true;//블록이 생성되었으므로 true로 설정
    }
    
}
