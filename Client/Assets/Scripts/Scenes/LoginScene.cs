using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        // 빌드 화면 크기 설정
        Screen.SetResolution(1920, 1080, false);

        Application.runInBackground = true;
    }

    public override void Clear()
    {

    }
}