using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public float step_timer = 0.0f;//경과시간 유지
    private PlayerControl player = null;

    private AudioSource audio;
    public AudioClip jumpSound;

    public static string mes_text = "점프 : 마우스 왼쪽 버튼\n 구멍에 빠지면 게임 오버\n 게임이 진행될 수록 속도가 빨라지며 구멍의 크기도 커진다.";

    void Start()
    {
        this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

        this.audio = this.gameObject.AddComponent<AudioSource>();
        this.audio.clip = this.jumpSound;
        this.audio.loop = false;
    }

    void Update()
    {
        this.step_timer += Time.deltaTime;//경과시간 더해가기

        if (this.player.isPlayEnd())
        {
            Application.LoadLevel("TitleScene");
        }

        this.audio.Play();
    }

    public float getPlayTime()//-->맵크리에이터의 create_floor_block()에서 사용
    {
        float time;
        time = this.step_timer;
        return (time);//호출한 곳에 경과시간을 알려줌
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2, 128, 1000, 1000), mes_text);
    }


}
