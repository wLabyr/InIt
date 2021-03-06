﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Option : MonoBehaviour {
    bool alreadyFaded = false;

    void Start() {
        // 불러오기
        PlayerData.LoadOptionData();
        // 불륨 적용
        GameObject.Find("Effects").GetComponent<AudioSource>().volume = PlayerData.Option.volumeEffects;
        GameObject.Find("BGM").GetComponent<AudioSource>().volume = PlayerData.Option.volumeBGM;
    }

    public void OnEffectsVolumeChange() {
        GameObject.Find("Effects").GetComponent<AudioSource>().volume = GameObject.Find("Effects Area").transform.FindChild("Slider").GetComponent<Slider>().value;
    }

    public void OnBGMVolumeChange() {
        GameObject.Find("BGM").GetComponent<AudioSource>().volume = GameObject.Find("BGM Area").transform.FindChild("Slider").GetComponent<Slider>().value;
    }

    public void Open() {
        /*
         *  Option.Open()
         *      초기화
         *      UI 활성화
         */
        
        /* 초기화 */
        // BGM 불륨
        GameObject.Find("BGM Area").transform.FindChild("Slider").GetComponent<Slider>().value = PlayerData.Option.volumeBGM;
        // 효과음 불륨
        GameObject.Find("Effects Area").transform.FindChild("Slider").GetComponent<Slider>().value = PlayerData.Option.volumeEffects;

        /* UI 활성화 */
        Transform faderui = GameObject.Find("Screen Fader UI").transform;
        Transform fader = GameObject.Find("Screen Fader").transform;
        if(faderui.localScale == new Vector3(0f, 0f, 0f)) {
            // Screen Fader Fade In
            faderui.localScale = new Vector3(1f, 1f, 1f);
            fader.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            fader.GetComponent<ScreenFader>().endOpacity = .6f;

            alreadyFaded = false;
        } else
            alreadyFaded = true;
        // Show Option UI
        GameObject.Find("Option UI").transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void Close() {
        /*
         *  Option.Close()
         *      불륨 적용
         *      저장
         *      UI 비활성화
         */
        
        /* 불륨 적용 */
        GameObject.Find("BGM").GetComponent<AudioSource>().volume = PlayerData.Option.volumeBGM;
        GameObject.Find("Effects").GetComponent<AudioSource>().volume = PlayerData.Option.volumeEffects;

        /* 저장 */
        PlayerData.SaveOptionData();

        /* UI 비활성화 */
        if(!alreadyFaded) {
            // Screen Fader Fade Out
            GameObject.Find("Screen Fader").GetComponent<ScreenFader>().endOpacity = 0f;
        }
        // Hide Option UI
        GameObject.Find("Option UI").transform.localScale = new Vector3(0f, 0f, 0f);
    }

    public void ClickApplyButton() {
        PlayerData.Option.volumeBGM = GameObject.Find("BGM Area").transform.FindChild("Slider").GetComponent<Slider>().value;
        PlayerData.Option.volumeEffects = GameObject.Find("Effects Area").transform.FindChild("Slider").GetComponent<Slider>().value;

        Close();
    }

    public void ClickCancelButton() {
        Close();
    }
}
