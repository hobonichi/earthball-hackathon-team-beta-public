// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using UnityEngine;

using System;

namespace Hobonichi
{

    // このクラスは、MIT ライセンスで公開されている以下のライブラリから引用しています。
    // [UniWebViewMarginsFromRectTransform](https://github.com/baba-s/UniWebViewMarginsFromRectTransform)
    // * Copyright (c) 2019 baba_s
    // The MIT License: https://opensource.org/license/mit/
    /// <summary>
    /// WebView に関する汎用機能を管理するクラス
    /// </summary>
    public static class WebViewUtils
    {
        //==============================================================================
        // 構造体
        //==============================================================================
        /// <summary>
        /// マージンの情報を管理する構造体
        /// </summary>
        [Serializable]
        public struct Margins
        {
            [SerializeField] private int m_left;
            [SerializeField] private int m_top;
            [SerializeField] private int m_right;
            [SerializeField] private int m_bottom;

            public int Left => m_left;
            public int Top => m_top;
            public int Right => m_right;
            public int Bottom => m_bottom;

            public Margins
            (
                int left,
                int top,
                int right,
                int bottom
            )
            {
                m_left = left;
                m_top = top;
                m_right = right;
                m_bottom = bottom;
            }
        }

        //==============================================================================
        // 関数(static)
        //==============================================================================
        /// <summary>
        /// 指定された RectTransform のサイズに合わせて WebView のマージンを返します
        /// </summary>
        public static Margins ToMargins(RectTransform rectTransform)
        {
            var canvas = rectTransform.GetComponentInParent<Canvas>();
            var camera = canvas.worldCamera;
            var corners = new Vector3[4];

            rectTransform.GetWorldCorners(corners);

            var screenCorner1 = RectTransformUtility.WorldToScreenPoint(camera, corners[1]);
            var screenCorner3 = RectTransformUtility.WorldToScreenPoint(camera, corners[3]);

            var rect = new Rect();

            rect.x = screenCorner1.x;
            rect.width = screenCorner3.x - rect.x;
            rect.y = screenCorner3.y;
            rect.height = screenCorner1.y - rect.y;

            var margins = new Margins
            (
                left: (int)rect.xMin,
                top: Screen.height - (int)rect.yMax,
                right: Screen.width - (int)rect.xMax,
                bottom: (int)rect.yMin
            );

            return margins;
        }
    }

    public class WebBehaviour : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        // この関数の実装の一部に、以下の zlib ライセンスのライブラリのサンプルコードを利用しています
        // [unity-webview](https://github.com/gree/unity-webview/)
        // * Copyright (C) 2012 GREE, Inc.
        // The zlib License: https://opensource.org/license/zlib-license-php/
        public void Open(string url, string userState)
        {
            var webview = GetComponentInChildren<WebViewObject>();
            if (webview == null)
            {
                var v = transform.Find("View");
                webview = v.gameObject.AddComponent<WebViewObject>();

                webview.Init(
                    cb: (msg) =>
                    {
                        Debug.Log(string.Format("CallFromJS[{0}]", msg));
                        OnJavaScriptCallback(msg);
                    },
                    err: (msg) =>
                    {
                        Debug.Log(string.Format("CallOnError[{0}]", msg));
                    },
                    httpErr: (msg) =>
                    {
                        Debug.Log(string.Format("CallOnHttpError[{0}]", msg));
                    },
                    started: (msg) =>
                    {
                        //Debug.Log(string.Format("CallOnStarted[{0}]", msg));
                    },
                    hooked: (msg) =>
                    {
                        Debug.Log(string.Format("CallOnHooked[{0}]", msg));
                    },
                    ld: (msg) =>
                    {
                        //Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
#if UNITY_EDITOR_OSX || (!UNITY_ANDROID && !UNITY_WEBPLAYER && !UNITY_WEBGL)
                        webview.EvaluateJS(@"
                            if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                                window.Unity = {
                                    call: function(msg) {
                                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                                    }
                                }
                            } else {
                                window.Unity = {
                                    call: function(msg) {
                                        window.location = 'unity:' + msg;
                                    }
                                }
                            }
                        ");
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                        webview.EvaluateJS(
                            "window.Unity = {" +
                            "   call:function(msg) {" +
                            "       parent.unityWebView.sendMessage('WebViewObject', msg)" +
                            "   }" +
                            "};");
#endif
                        webview.EvaluateJS(@"
                            window.SendGlobeCommand = function(commands) {
                                Unity.call(JSON.stringify(commands));
                            }
                            if (window.GlobeInitialCommand) {
                                SendGlobeCommand(window.GlobeInitialCommand);
                                window.GlobeInitialCommand = null;
                            }
                        ");
                    }
                );
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                webview.bitmapRefreshCycle = 1;
#endif
                var rectTransform = v.GetComponent<RectTransform>();
                var margins = WebViewUtils.ToMargins(rectTransform);
                webview.SetMargins
                (
                    left: margins.Left,
                    top: margins.Top,
                    right: margins.Right,
                    bottom: margins.Bottom
                );

                webview.SetTextZoom(100);  // android only. cf. https://stackoverflow.com/questions/21647641/android-webview-set-font-size-system-default/47017410#47017410
                webview.SetVisibility(true);

                // TODO: 簡易実装です。すべてのリクエストに付加されてしまうので、セッションハイジャックのリスクがあることに注意。
                webview.RemoveCustomHeader("X-User-State");
                if (!string.IsNullOrEmpty(userState)) {
                    webview.AddCustomHeader("X-User-State", userState);
                }

                webview.LoadURL(url);
            }
        }

        public void Close()
        {
            Destroy(GetComponentInChildren<WebViewObject>());
            gameObject.SetActive(false);
        }

        public void UpdateState(string userState)
        {
            var webview = GetComponentInChildren<WebViewObject>();
            webview.RemoveCustomHeader("X-User-State");
            if (!string.IsNullOrEmpty(userState))
            {
                webview.AddCustomHeader("X-User-State", userState);
            }
        }

        private void OnJavaScriptCallback(string msg)
        {
            ActionInfo[] actions = JsonHelper.FromJson<ActionInfo>(msg);
            FindObjectOfType<Controller>().RunActions(actions);
        }
    }

}