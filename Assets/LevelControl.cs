using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelData
{
    public struct Range //범위
    {
        public int min;
        public int max;
    };

    public float end_time;//종료시간
    public float player_speed;//플레이어 속도
    public Range floor_count;//발판 블록 수 범위
    public Range hole_count;//구멍 개수 범위
    public Range height_diff;//발판 높이 범위

    public LevelData()
    {
        this.end_time = 15.0f;
        this.player_speed = 6.0f;
        this.floor_count.min = 10;
        this.floor_count.max = 10;
        this.hole_count.min = 2;
        this.hole_count.max = 6;
        this.height_diff.min = 0;
        this.height_diff.max = 0;
            
    }
}

public class LevelControl : MonoBehaviour
{
    //본 스크립트는 타이밍 별 블록 생성 유무를 결정하는 스크립트로, 게임 오브젝트에 전달하지 않음. 따라서 자동 호출 메서드는 사용 X

    public float getPlayerSpeed()
    {
        return (this.level_datas[this.level].player_speed);
    }

    private List<LevelData> level_datas = new List<LevelData>();
    public int HEIGHT_MAX = 20;
    public int HEIGHT_MIN = -4;

    public struct CreationInfo //만들어야 하는 블록 정보. 이전, 현재, 다음
    {
        public Block.TYPE block_type; //블록의 종류
        public int max_count; //블록 최대 개수
        public int height; //블록 배치 높이
        public int current_count; //작성한 블록의 개수
    };

    public CreationInfo previous_block; //이전에 어떤 블록을 만들었는가?
    public CreationInfo current_block; // 지금 어떤 블록을 만들어야 하는가?
    public CreationInfo next_block; // 다음에 어떤 블록을 만들어야 하는가?

    public int block_count = 0;//생성한 블록 총 수
    public int level = 0;//난이도
  
    private void clear_next_block(ref CreationInfo block) //프로필 노트에 초깃값을 넣는다. 외부에서 호출될 일이 없기에 프라이빗으로 작성
    {
        block.block_type = Block.TYPE.FLOOR;
        block.max_count = 15;
        block.height = 0;
        block.current_count = 0;
    }

    public void initialize() //프로필 노트 초기화
    {
        this.block_count = 0;
        this.clear_next_block(ref this.previous_block);//이전
        this.clear_next_block(ref this.current_block);//현재
        this.clear_next_block(ref this.next_block);//다음
    }

    private void update_level(ref CreationInfo current, CreationInfo previous, float passage_time)//새 인수 passage_time으로 플레이 경과 시간을 받도록 추가.
    {   //일정 범위 내에서 값을 순환시키고자 할 때 Mathf.Repeat(value, max) 사용
        float local_time = Mathf.Repeat(passage_time, this.level_datas[this.level_datas.Count - 1].end_time);//레벨1~5 반복.  레벨 5가 될 경우 다시 레벨 1로 루프

        int i;
        for (i = 0; i < this.level_datas.Count - 1; i++)//현재 레벨 구하기
        {
            if(local_time <= this.level_datas[i].end_time)
            {
                break;
            }
        }
        this.level = i;

        current.block_type = Block.TYPE.FLOOR;
        current.max_count = 1;

        if(this.block_count >= 10)
        {
            //현재 레벨용 데이터를 가져온다
            LevelData level_data;
            level_data = this.level_datas[this.level];

            switch (previous.block_type) 
            {   
                case Block.TYPE.FLOOR:
                    current.block_type = Block.TYPE.HOLE;//이전 블록이 바닥일 때->구멍 생성
                    current.max_count = Random.Range(level_data.hole_count.min, level_data.hole_count.max);//구멍 크기 최솟값~최댓값 사이의 랜덤된 값
                    current.height = previous.height;break;
                case Block.TYPE.HOLE:
                    current.block_type = Block.TYPE.FLOOR;//이전 블록이 구멍이 때->바닥 생성
                    current.max_count = Random.Range(level_data.floor_count.min, level_data.floor_count.max);
                    //바닥 높이 최솟값~최댓값
                    int height_min = previous.height + level_data.height_diff.min;
                    int height_max = previous.height + level_data.height_diff.max;
                    height_min = Mathf.Clamp(height_min, HEIGHT_MIN, HEIGHT_MAX);
                    height_max = Mathf.Clamp(height_max, HEIGHT_MIN, HEIGHT_MAX);//Mathf.Clamp(value, min, max) = 값을 최솟값~최댓값 범위 내에 강제로 넣기 위해 사용

                    current.height =Random.Range(height_min, height_max);break;
            }

        }
    }

    
    public void update(float passage_time)//반복 실행이 필요한 갱신 처리
    {
        this.current_block.current_count++;//이번에 만든 블록 개수 증가
        if(this.current_block.current_count >= this.current_block.max_count)
        {
            this.previous_block = this.current_block;//이번에 만든 블록 개수 >= 맥스 카운트
            this.current_block = this.next_block;      
            this.clear_next_block(ref this.next_block);//다음에 만들 블록 내용 초기화
            this.update_level(ref this.next_block, this.current_block, passage_time);//다음에 만들 블록 설정
        }
        this.block_count++;//블록 총 수 증가
    }

    public void loadLevelData(TextAsset Level_Data_text)//텍스트파일 로드 메서드
    {
        string level_texts = Level_Data_text.text;//텍스트 데이터를 문자열로 가져온다
        string[] lines = level_texts.Split('\n');//개행문자마다 분할하여 문자열 배열에 넣는다.
        foreach(var line in lines)//lines 내의 각 행에 대하여 차례로 처리해 가는 루프
        {
            if (line == "")
            {
                continue;//행이 빈 줄이면 아래 처리를 하지 않고 루프 처음으로.
            };
            Debug.Log(line);//행의 내용을 디버그 출력
            string[]words = line.Split();//행 내의 워드를 배열에 저장
            int n = 0;

            LevelData level_data = new LevelData();//현재 처리하는 행의 데이터를 넣어간다.

            foreach(var word in words)
            {
                //words 내의 각 워드에 대하여 순서대로 처리
                if (word.StartsWith("#"))
                {
                    break;//워드 시작문자가 #이면 루프 탈출
                }
                if (word == "")
                {
                    continue;
                }

                switch (n)//n값을 0~7로 변화시켜가며 8항목처리. 각 워드를 플롯값으로 변환 후 레벨데이터에 저장
                {
                    case 0:level_data.end_time = float.Parse(word); break;
                    case 1:level_data.player_speed = float.Parse(word); break;
                    case 2:level_data.floor_count.min = int.Parse(word);break;
                    case 3:level_data.floor_count.max = int.Parse(word);break;
                    case 4:level_data.hole_count.min = int.Parse(word);break;
                    case 5:level_data.hole_count.max = int.Parse(word);break;
                    case 6:level_data.height_diff.min = int.Parse(word);break;
                    case 7:level_data.height_diff.max = int.Parse(word);break;
                }
                n++;
            }
            if (n >= 8)
            {
                this.level_datas.Add(level_data);//8항목 정상 처리 시 리스트구조의 level_datas에 level_data를 추가
            }
            else
            {
                if (n == 0)
                {
                    //1 워드도 처리하지 않은 경우=주석-->문제x
                }
                else
                {//그 이외-->데이터 개수가 맞지 않으므로 오류메세지 출력
                    Debug.LogError("[LevelData] Out of parameter.\n");
                }
            }
        }
        if (this.level_datas.Count == 0)
        {
            Debug.LogError("[LevelData] Has no data\n");//level_datas에 데이터가 하나도 없으면 오류메세지 표시
            this.level_datas.Add(new LevelData());//기본 레벨데이터를 하나 추가
        }
    }
}
