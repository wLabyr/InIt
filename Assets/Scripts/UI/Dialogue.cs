﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Dialogue : MonoBehaviour {
    public float printSpeed = 0.1f;     // 메시지 출력 속도
    public float blinkSpeed = 2f;       // Blinker의 페이드 속도
    public float blinkOpacity = 1f;     // Blinker의 최대 투명도
    public int msgCount = 0;            // 메시지 출력 카운트

    Transform uiObj;                    // UI 오브젝트
    Transform dialogueObj;              // 메시지 오브젝트
    Transform posterObj;                // 포스터 오브젝트
    string contactedNPC = string.Empty; // 현재 대화중인 NPC
    string msgResult;                   // 메시지 내용
    int msgLength;                      // 메시지 길이
    int msgIndex = 0;                   // 메시지 인덱스
    bool talkToggle = false;            // 키 인식의 빠른 연속 방지
    float blinkSpeedTmp;                // Blinker의 페이드 속도
    float blinkOpacityTmp;              // Blinker의 최대 투명도

    void Start() {
        uiObj = GameObject.Find("Dialogue UI").transform;               // UI 오브젝트
        dialogueObj = uiObj.FindChild("Canvas").FindChild("Dialogue");  // 메시지 오브젝트
        posterObj = uiObj.FindChild("Canvas").FindChild("Poster");      // 포스터 오브젝트

        // 오브젝트 숨김
        uiObj.localScale = dialogueObj.localScale = posterObj.localScale = new Vector3(0f, 0f, 0f);
        
        // blinkSpeed와 blinkOpacity가 가변성을 가지므로 미리 처음 값을 백업함
        blinkSpeedTmp = blinkSpeed;
        blinkOpacityTmp = blinkOpacity;
    }

    void Update() {
        /* NPC 대화 처리 */
        // Space 혹은 Enter 키를 누른 경우
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            Talk();
        }

        /* Blinker 깜빡거리기 */
        // Dialogue UI 우측 하단의 깜빡이
        Text blinker = dialogueObj.FindChild("Blinker").GetComponent<Text>();
        // 시간에 따른 투명도
        float opacity = blinker.color.a + blinkSpeedTmp * Time.deltaTime;
        
        // Blinker 투명도 설정
        blinker.color = new Color(blinker.color.r, blinker.color.g, blinker.color.b, opacity);
        if(opacity >= blinkOpacityTmp && blinkSpeedTmp >= 0f) {
            blinkSpeedTmp = -blinkSpeedTmp;
            blinkOpacityTmp = 0f;
        } else if(opacity <= 0f && blinkSpeedTmp <= 0f)
            blinkSpeedTmp = -blinkSpeedTmp;
            blinkOpacityTmp = blinkOpacity;
    }

    public void OnClick() {
        Talk();
    }

    void ToggleTalk() {
        talkToggle = false;
    }

    public void Talk() {
        if(talkToggle)
            return;
        talkToggle = true;
        Invoke("ToggleTalk", .5f);

        // 포스터 이미지
        Transform poster = transform.FindChild("Canvas").FindChild("Poster");
        // 상대 NPC 이름
        string npcName = PlayerData.collidedNPC.Length > 0 ? PlayerData.collidedNPC : contactedNPC;
        // 상대 NPC
        NPC npc;
        
        // 충돌 유지중이거나 대화중인 NPC가 없는 경우
        if(npcName.Length < 1) {
            if(transform.localScale == new Vector3(1f, 1f, 1f))
                HideDialogue();
            Debug.Log("[Talk Debug] 상대 NPC 없음");
            return;
        } else
            npc = GameObject.Find(npcName).GetComponent<NPC>();

        // 메뉴가 활성화되어 있는 경우
        if(GameObject.Find("Menu UI").transform.localScale == new Vector3(1f, 1f, 1f)) {
            Debug.Log("[Talk Debug] 메뉴 활성화되어 있음");
        }
        // 대사가 출력중인 경우
        else if(msgCount != 0) {
            Debug.Log("[Talk Debug] 대사 출력중");
        }
        // NPC가 대화를 거부하였거나 더 할 대사가 없는 경우
        else if(msgIndex == -1 || msgIndex >= npc.Messages.Length) {
            Debug.Log("[Talk Debug] 대화 거부 OR 대사 없음");

            // 상대 NPC 없음
            contactedNPC = string.Empty;
            // 대화 UI 숨김
            HideDialogue();
            // 레벨 설정
            if(msgIndex != -1)
                PlayerData.Player.Level = npc.Stage;
            // 대사 인덱스 초기화
            msgIndex = 0;
        }
        // 대사가 있는 경우
        else if(npc.Messages.Length > 0) {
            // 상대 NPC 이름
            contactedNPC = npc.transform.name;

            // 이 NPC와 대화할 적정 단계인 경우
            if(PlayerData.Player.Level == npc.Stage-1) {
                string message = npc.Messages[msgIndex++];

                // 포스터 이미지 출력 명령인 경우
                if(message == "Poster") {
                    // 포스터 이미지 출력
                    poster.FindChild("Image").GetComponent<Image>().sprite = npc.Poster;
                    poster.localScale = new Vector3(1f, 1f, 1f);
                } else {
                    // 메시지 중 뒤집기 명령이 포함되어 있는 경우
                    if(message.Contains("{Flip}")) {
                        // 뒤집는다.
                        SpriteRenderer npcSprite = npc.GetComponent<SpriteRenderer>();
                        npcSprite.flipY = !npcSprite.flipY;
                        // 메시지에서 뒤집기 명령 제거
                        message.Replace("{Flip}", string.Empty);
                    }

                    // 대화 UI 출력
                    ShowDialogue(npc.Name, message);
                }
            }
            // 플레이어의 진행도가 낮은 경우
            else if(PlayerData.Player.Level < npc.Stage-1) {
                // 대화 거부 처리
                msgIndex = -1;
                // 대화 UI 출력
                if(npc.dontChat)
                    Talk();
                else
                    ShowDialogue(npc.Name, "나 말고 다른 사람한테 먼저 가봐.");
            }
            // 플레이어의 진행도가 높은 경우
            else if(PlayerData.Player.Level > npc.Stage-1) {
                // 대화 거부 처리
                msgIndex = -1;
                // 대화 UI 출력
                if(npc.dontChat)
                    Talk();
                else
                    ShowDialogue(npc.Name, "난 이제 해 줄 말이 없어.");
            }
        }
    }

    void PrintMessage() {
        // 메시지 오브젝트와 내용이 존재하는 경우
        if(dialogueObj && msgResult.Length > 0) {
            // 메시지 출력 방향에 따라 내용이 한 글자씩 추가되며 출력됨
            string tmp = msgResult.Substring(0, msgCount);
            dialogueObj.FindChild("Result").GetComponent<Text>().text = tmp;

            // 메시지 출력이 완료된 경우
            if(msgCount++ == msgLength) {
                // 카운트 초기화
                msgCount = 0;
                // 타이머 해제
                CancelInvoke("PrintMessage");
            }
        }
    }

    void HideDialogue() {
        // 오브젝트 숨김
        uiObj.localScale = dialogueObj.localScale = posterObj.localScale = new Vector3(0f, 0f, 0f);
        // 타이머 강제 해제
        CancelInvoke("PrintMessage");
        // 캐릭터 이동 가능
        GameObject.Find("Character").GetComponent<CharacterMove>().canmove = true;
        // 초기화
        msgResult = string.Empty;
    }

    public void ShowDialogue(string name, string result) {
        // 이름 텍스트 설정
        dialogueObj.FindChild("Name").GetComponent<Text>().text = name;
        // Dialogue UI 활성화
        uiObj.localScale = dialogueObj.localScale =  new Vector3(1f, 1f, 1f);
        
        msgResult = result.Replace("\\n", "\n");            // 줄바꿈 문자 변환
        msgLength = msgResult.Length;                       // 메시지 길이
        msgCount = 0;                                       // 메시지 출력 카운트 초기화
        InvokeRepeating("PrintMessage", 0f, printSpeed);    // PrintMessage() 타이머 설정

        // 캐릭터 이동 불가능
        GameObject.Find("Character").GetComponent<CharacterMove>().canmove = false;
    }
}
