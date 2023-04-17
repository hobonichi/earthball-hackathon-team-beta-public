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
    [ExecuteAlways]
    public class ObjectOnSphere : MonoBehaviour {
        [SerializeField] public Vector3 location;
        [SerializeField] public Vector3 direction;

        Vector3 lastLocation;
        Vector3 lastDirection;

        public ObjectInfo Info { get; private set; }

        private Material _material;
        private Texture _texture;
        string mp3_url = "";
        public string[] similar_countries;

        private AudioSource audioSource;
        public SoundDataManager sound_manager;

        public void Start() {
            if (sound_manager == null) {
                sound_manager = GameObject.FindWithTag("SoundManager").GetComponent<SoundDataManager>();;
            }
        }

        public void mul_scale(float p) {
            transform.localScale = new Vector3(transform.localScale.x * p, transform.localScale.y, transform.localScale.z * p) ;
        }

        public void Setup(ObjectInfo info, Material mat, Controller.ResolveURLDelegate resolveURLMethod)
        {
            location = info.Location;
            direction = info.Direction;
            transform.localScale = info.Scale;
            mp3_url = info.mp3_url;
            similar_countries = info.SimilarContries;

            if (info.actions.Length == 0) {
                foreach (var c in GetComponentsInChildren<Collider>(true)) {
                    Destroy(c);
                }
            }
            else {
                foreach (var t in GetComponentsInChildren<EventTrigger>(true)) {
                    EventTrigger.Entry e = new EventTrigger.Entry();
                    e.eventID = EventTriggerType.PointerClick;
                    e.callback.AddListener(d => OnClick(gameObject.name, mp3_url, similar_countries));
                    t.triggers.Add(e);
                }
            }

            var enabled = FindObjectOfType<Controller>().IsTracking;
            foreach (var r in GetComponentsInChildren<Renderer>(true)) {
                r.enabled = enabled;
            }
            foreach (var c in GetComponentsInChildren<Collider>(true)) {
                c.enabled = enabled;
            }

            var renderer = GetComponent<Renderer>();
            if (mat) {
                _material = Instantiate(mat);
                renderer.material = _material;
            }
            if (!string.IsNullOrEmpty(info.texture)) {
                if (!mat) {
                    _material = new Material(renderer.sharedMaterial);
                    renderer.material = _material;
                }
                renderer.enabled = false;
                var collider = GetComponent<Collider>();
                if (collider) {
                    collider.enabled = false;
                }
                //テクスチャの取得URLは相対パスではなく、絶対パスから読み込むように変更。
                LoadTextureByURL(new Uri(info.texture).AbsoluteUri, info.AutoScale, () =>
                {
                    renderer.enabled = FindObjectOfType<Controller>().IsTracking;
                    if (collider) {
                        collider.enabled = renderer.enabled;
                    }
                });
            }

            if (!string.IsNullOrEmpty(info.label)) {
                foreach (var tmp in GetComponentsInChildren<TextMeshPro>(true)) {
                    tmp.text = info.label;
                    var boxCollider = tmp.gameObject.GetComponent<BoxCollider>();
                    if (boxCollider) {
                        tmp.ForceMeshUpdate();
                        var bounds = tmp.bounds;
                        boxCollider.center = bounds.center;
                        boxCollider.size = bounds.size;
                    }
                }
            }

            Info = info;
        }

        void Update() {
            if (lastLocation != location || lastDirection != direction) {
                var p1 = SphericalGeometry.LLH2Pos(location);
                var p2 = new Vector3(0, 1, 0);
                var p3 = Vector3.ProjectOnPlane(p2, p1);
                transform.localPosition = p1;
                transform.localRotation = Quaternion.LookRotation(p3, p1) * Quaternion.Euler(direction);

                lastLocation = location;
                lastDirection = direction;
            }
        }

        public Coroutine LoadTextureByURL(string url, bool autoScale, System.Action callback) {
            return StartCoroutine(_LoadTexture(url));

            IEnumerator _LoadTexture(string url) {
                UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success) {
                    var r = GetComponent<Renderer>();
                    if (r) {
                        var t = ((DownloadHandlerTexture)req.downloadHandler).texture;
                        _texture = t;
                        r.sharedMaterial.SetTexture("_MainTex", t);
                        if (autoScale) {
                            var l = transform.localScale.x;
                            var s = l * ((t.width < t.height) ? (float)t.width / t.height : (float)t.height / t.width);
                            transform.localScale = t.width < t.height ? new Vector3(s, 1, l) : new Vector3(l, 1, s);
                        }
                    }
                }
                else {
                    Debug.LogErrorFormat("failed to load texture: {0} / {1}", gameObject.name, req.result);
                }

                callback.Invoke();
            }
        }

        void OnDestroy()
        {
            if (_material) {
                Destroy(_material); _material = null;
            }
            if (_texture) {
                Destroy(_texture); _texture = null;
            }
        }

        public void OnClick(string name, string url_mp3, string[] contries) {
            if (url_mp3 == "") {
                return;
            }
            Debug.LogFormat("click: {0}", name);
            StartCoroutine(sound_manager.GetMP3(name, url_mp3, contries));
        }
    }
}
