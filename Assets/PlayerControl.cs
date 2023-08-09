using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public static float ACCELERATION = 10.0f;//가속도
    public static float SPEED_MIN = 4.0f;//속도 최솟값
    public static float SPEED_MAX = 8.0f;//속도 최댓값
    public static float JUMP_HEIGHT_MAX = 3.0f;//점프 높이
    public static float JUMP_KEY_RELEASE_REDUCE = 0.5f;//점프 후의 감속도

    public static float LOW_HEIGHT = -0.0f;//문턱값보다 낮은 장소에 플레이어가 존재 시, 즉 구멍에 떨어질 시

    public float current_speed = 0.0f;//현재 속도
    public LevelControl level_control = null;//LevelControl이 저장됨 --->LevelControl과 연계하기 위한 변수 2개 추가

    private float click_timer = -1.0f;//버튼이 눌린 후의 시간
    private float CLICK_GRACE_TIME = 0.5f;//점프하고싶은 의사를 받아들일 시간

    //GUI를 위한 변수 추가
    private Text JumpScore;
    private int getCount = 0;

    public enum STEP//Player의 각종 상태를 나타내는 자료형
    {
        NONE = -1,//상태정보 없음
        RUN = 0,//달린다
        JUMP,//점프
        MISS,//실패
        NUM,//상태가 몇 종류 있는지 보여준다.(=3)

    };

    public STEP step = STEP.NONE;//플레이어 현재 상태
    public STEP next_step = STEP.NONE;//플레이어 다음 상태

    public float step_timer = 0.0f;//경과 시간
    private bool is_landed = false;//착지 유무
    private bool is_colided = false;//무언가와 충돌했는지 유무
    private bool is_key_released = false;//버튼이 떨어졌는지 유무

    void Start()
    {
        this.next_step = STEP.RUN;//게임을 시작하자마자 달릴 수 있도록

        JumpScore = GameObject.Find("Score").GetComponent<Text>();
    }

    private void check_landed()
    {
        this.is_landed = false;
        do
        {
            Vector3 s = this.transform.position;//플레이어 현재위치
            Vector3 e = s + Vector3.down * 1.0f;//s부터 아래로 1.0f 이동한 위치
            RaycastHit hit;
            if (!Physics.Linecast(s, e, out hit))//s부터 e 사이에 아무것도 없을 때
            {
                break;//아무것도 하지 않고 do while 루프 탈출
            }
            if (this.step == STEP.JUMP) //s부터 e 사이에 뭔가 있을 때 아래의 처리가 실행
            {
                if (this.step_timer < Time.deltaTime * 3.0f)//경과 시간이 3.0f 미만일 때
                {
                    break;
                }
            }
            this.is_landed = true;//s부터 e 사이에 뭔가 있고 jump 직후가 아닐 때 실행
        }
        while (false);//루프 탈출구
    }

    void Update()
    {
        
        Vector3 velocity = this.GetComponent<Rigidbody>().velocity;//속도 설정
        this.current_speed = this.level_control.getPlayerSpeed();
        this.check_landed();//착지 상태인지 체크

        switch (this.step) //현재 위치가 문턱값 아래이면 실패
        {
            case STEP.RUN:
            case STEP.JUMP:
                if(this.transform.position.y < LOW_HEIGHT)
                {
                    this.next_step = STEP.MISS;
                }
                break;
        }

        this.step_timer += Time.deltaTime;//경과시간 체크

        if (Input.GetMouseButtonDown(0))
        {
            this.click_timer = 0.0f;//버튼이 눌렸으면 타이머 리셋
        }
        else
        {
            if(this.click_timer >= 0.0f)
            {
                this.click_timer += Time.deltaTime;//버튼이 안눌렸다면 경과시간 더하기
            }
        }


        //다음 상태가 정해져있지 않으면 상태의 변화를 조사
        if (this.next_step == STEP.NONE)
        {
            switch (this.step)//플레이어의 현재상태로 분기
            {
                case STEP.RUN://달리는 중일 때
                    if(0.0f <=this.click_timer && this.click_timer <= CLICK_GRACE_TIME)//클릭 타이머가 0 이상, 클릭 그레이스 타임 이하일 때
                    {
                        if (this.is_landed)
                        {//착지했다면 버튼이 눌리지 않은 상태인 -1.0f 점프상태.
                            this.click_timer = -1.0f;
                            this.next_step = STEP.JUMP;
                        }
                    }
                    /*if (!this.is_landed)
                    {
                        //달리는중, 착지하지 않은 경우 행동 x
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            this.next_step = STEP.JUMP;//달리는 중, 착지, 왼쪽 버튼 눌림 시 다음 상태를 점프로 변경
                        }
                    }*/
                    break;
                case STEP.JUMP://점프중일 때
                    if (this.is_landed)
                    {
                        this.next_step = STEP.RUN;//점프 중 착지 시 다음 상태를 주행으로.
                        getCount++;
                    }
                    break;
            }
        }
        while (this.next_step != STEP.NONE) // 다음 정보가 '상태정보 없음'이 아닌 동안. 즉 상태가 변할 때
        {
            this.step = this.next_step; // 현재 상태 = 다음 상태
            this.next_step = STEP.NONE; // 다음 상태 = 상태 없음

            switch (this.step) // 갱신된 현재 상태 스위치
            {
                case STEP.JUMP: // 최고 도달점 높이까지 점프할 수 있는 속도 계산. sqrt는 제곱근을 구하는 메서드.
                    velocity.y = Mathf.Sqrt(2.0f * 9.8f * PlayerControl.JUMP_HEIGHT_MAX); // 버튼이 떨어졌음을 나타내는 플래그 클리어
                    this.is_key_released = false;
                    break;
            }
            this.step_timer = 0.0f; // 상태가 변했으므로 경과시간 리셋 
        }


        //상태 별 매 프레임 갱신 처리
        switch (this.step)
        {
            case STEP.RUN://달리는 중일 때
                velocity.x += PlayerControl.ACCELERATION * Time.deltaTime;

               /* if (Mathf.Abs(velocity.x) > PlayerControl.SPEED_MAX)//속도가 최고속도 제한을 넘을 때
                {
                    velocity.x *= PlayerControl.SPEED_MAX / Mathf.Abs(this.GetComponent<Rigidbody>().velocity.x);
                }*/
               if(Mathf.Abs(velocity.x) > this.current_speed)//계산으로 구한 속도가 설정해야 할 속도를 넘었다면 넘지 않게 조정
                {
                    velocity.x*=this.current_speed / Mathf.Abs(velocity.x);
                }
                break;
            case STEP.JUMP://점프중일 때
                
                    if (!Input.GetMouseButtonUp(0))//버튼이 떨어진 순간이 아닐 때, 즉 버튼이 눌리고 있을 때.
                    {
                        break;
                    }
                    if (this.is_key_released)//이미 감속된 상태일 때(두 번이상 감속하지 않도록)
                    {
                        break;
                    }
                    if (velocity.y <= 0.0f)//상하방향 속도가 0 이하일 때, 즉 하강 중일 때
                    {
                        break;
                    }
                    velocity.y *= JUMP_KEY_RELEASE_REDUCE;//버튼이 떨어져있고 상승 중일 때 -->감속 시작. 점프 상승 종료
                
                

                this.is_key_released = true;
                break;

            case STEP.MISS:
                velocity.x -= PlayerControl.ACCELERATION * Time.deltaTime;//가속도를 빼서 플레이어의 속도를 감소
                if(velocity.x < 0.0f)//플레이어 속도가 마이너스일 때->0으로
                {
                    velocity.x = 0.0f;
                }
                break;

                }
                 this.GetComponent<Rigidbody>().velocity = velocity;//Rigidbody의 속도를 위에서 구한 속도로 갱신

        SetCountText();
        
    }
       
    public bool isPlayEnd()//게임이 끝났는지 판정
    {
        bool ret = false;
        switch(this.step)
        {
            case STEP.MISS:
                ret = true;
                break;
        }
        return (ret);
    }

    void SetCountText()
    {
        JumpScore.text = "점프한 횟수 : " + getCount;
    }

    }
   

