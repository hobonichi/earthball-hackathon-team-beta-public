// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadSceneButton : MonoBehaviour {

  public void OnClick() {
    SceneManager.LoadScene("App");
  }
}
