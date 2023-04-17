// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using TMPro;

namespace Hobonichi {
    public class SoundDataManager : MonoBehaviour
    {
        private AudioClip audioClip;
        private AudioSource audioSource;
        [SerializeField] ObjectOnSphere flag;
        [SerializeField] bool is_play = false;
        [SerializeField] string current_play_url = "";
        [SerializeField] bool is_clicked_once = false;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        IEnumerator RequestURL(string url_mp3) {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url_mp3, AudioType.MPEG);
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log($"[Error]Response Code : {www.responseCode}");
            }
            else
            {
                audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = audioClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        void deactivateUnrelatedNations(string[] similar_countries) {
            // deactivateUnrelatedNations(name);
            GameObject[] objs = GameObject.FindGameObjectsWithTag("NationalFlag");
            Debug.LogFormat("found: {0}", objs.Length);
        
            foreach (GameObject o in objs)
            {
                if (!similar_countries.ToList().Contains(o.name)) {
                    Destroy(o);
                }
            }
        }

        public IEnumerator GetMP3(string name, string url_mp3, string[] contries)
        {
            ObjectOnSphere new_flag = GameObject.Find(name).GetComponent<ObjectOnSphere>();
            if (new_flag != null) {
                if (! is_clicked_once) {
                    deactivateUnrelatedNations(contries);
                }
                if (is_play == true) {
                    audioSource.Stop();
                    flag.mul_scale(0.2f);
                    if (current_play_url != url_mp3) {
                        StartCoroutine(RequestURL(url_mp3));
                        current_play_url = url_mp3;
                        is_play = true;
                        new_flag.mul_scale(5.0f);
                    } else {
                        is_play = false;
                    }
                } else {
                    if (current_play_url != url_mp3) {
                        StartCoroutine(RequestURL(url_mp3));
                        current_play_url = url_mp3;
                        is_play = true;
                    } else {
                        audioSource.Play();
                        is_play = true;
                    }
                    new_flag.mul_scale(5.0f);
                }
                flag = new_flag;
                is_clicked_once = true;
            }
            yield return null;
        }
        
        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
