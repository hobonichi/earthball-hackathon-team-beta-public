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
using Vuforia;


namespace Hobonichi {
    public class Controller : MonoBehaviour {
        [SerializeField] Material[] materials;
        [SerializeField] GameObject[] prefabs;
        [SerializeField] string initialURL;

        Uri baseURL;

        string userState;

        Dictionary<string, GameObject> objs = new Dictionary<string, GameObject>();
        Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();

        public delegate string ResolveURLDelegate(string url);

        public bool IsTracking {
            get {
                var s = FindObjectOfType<ModelTargetBehaviour>().TargetStatus.Status;
                return s == Status.TRACKED || s == Status.EXTENDED_TRACKED;
            }
        }

        void Start() {
            foreach (var p in GameObject.FindGameObjectsWithTag("UIPanel")) {
                panels.Add(p.name, p);
                p.SetActive(false);
            }

            userState = null;
            baseURL = new Uri(initialURL);

            LoadObjects(initialURL, clear: true);
        }

        void Update() {

        }

        public Coroutine LoadObjects(string url, bool clear = false) {
            return StartCoroutine(_LoadObjects());

            IEnumerator _LoadObjects() {
                var req = UnityWebRequest.Get(ResolveURL(url));
                if (!string.IsNullOrEmpty(userState)) {
                    req.SetRequestHeader("X-User-State", userState);
                }
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success) {
                    if (req.GetResponseHeader("X-User-State") != null) {
                        UpdateState(req.GetResponseHeader("X-User-State"));
                    }
                    if (clear) {
                        ClearObjects();
                    }
                    InstantiateObjects(req.downloadHandler.text);
                }
                else {
                    Debug.LogErrorFormat("failed to get initial url : {0}", req.result);
                }
            }
        }

        void ClearObjects() {
            foreach (var obj in objs) {
                Destroy(obj.Value);
            }
            objs.Clear();
        }

        void InstantiateObjects(string json) {
            var contentRoot = GameObject.FindGameObjectWithTag("ContentRoot").transform;

            var infos = JsonHelper.FromJson<ObjectInfo>(json);
            foreach (var info in infos) {
                var prefab = prefabs.FirstOrDefault(p => p.name == info.model);
                var mat = materials.FirstOrDefault(m => m.name == info.material);
                if (prefab) {
                    if (objs.ContainsKey(info.id)) {
                        if (info.OverwriteFlag) {
                            // 同じ id があり、overwrite が true なら、一度消してから再作成
                            DeleteObject(info.id);
                        }
                        else {
                            // さもなくば同一 id は生成スキップ
                            continue;
                        }
                    }
                    var obj = CreateObject(info, prefab, mat, contentRoot);
                    objs.Add(obj.name, obj);
                }
                else {
                    Debug.LogErrorFormat("unknown model name : {0}", info.model);
                }
            }
        }

        public GameObject CreateObject(ObjectInfo info, GameObject prefab, Material mat, Transform root) {
            var obj = Instantiate(prefab, root);
            obj.gameObject.name = info.id;

            var o = obj.GetComponent<ObjectOnSphere>();
            if (o) {
                o.Setup(info, mat, ResolveURL);
            }

            var l = obj.GetComponent<LineOnSphere>();
            if (l) {
                l.Setup(info);
            }

            return obj;
        }

        public void DeleteObject(string name) {
            if (objs.ContainsKey(name)) {
                Destroy(objs[name]);
                objs.Remove(name);
            }
        }

        public void OpenWeb(string url) {
            var panel = panels["Web"];
            panel.SetActive(true);
            panel.GetComponent<WebBehaviour>().Open(ResolveURL(url), userState);
        }

        public void CloseWeb() {
            var panel = panels["Web"];
            if (panel.activeSelf) {
                panel.GetComponent<WebBehaviour>().Close();
            }
        }

        public string ResolveURL(string url)
        {
            return new Uri(baseURL, url).AbsoluteUri;
        }

        public void UpdateState(string state)
        {
            Debug.Log("UpdateState : " + state);
            userState = state;

            var panel = panels["Web"];
            if (panel.activeSelf) {
                panel.GetComponent<WebBehaviour>().UpdateState(state);
            }
        }

        public void RunAction(ActionInfo action)
        {
            switch (action.action)
            {
                case "load":
                    LoadObjects(action.param, clear: true);
                    break;

                case "add":
                    LoadObjects(action.param, clear: false);
                    break;

                case "delete":
                    var es = action.param.Split(',');
                    for (int i = 0; i < es.Length; ++i) {
                        DeleteObject(es[i]);
                    }
                    break;
                
                case "open":
                    OpenWeb(action.param);
                    break;

                case "close":
                    CloseWeb();
                    break;
                
                case "update":
                    // TODO: 簡易実装です。JavaScript経由で外部のサイトから任意のタイミングで書き換えられるリスクがあります。
                    UpdateState(action.param);
                    break;
                
                default:
                    Debug.LogWarning("Unknown action : " + action.action);
                    break;
            }
        }

        public void RunActions(ActionInfo[] actions)
        {
            foreach (ActionInfo action in actions) {
                RunAction(action);
            }
        }

#if UNITY_EDITOR
        public void OnApplicationQuit() {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
#endif
    }
}