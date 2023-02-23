using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MultiplayerBuildAndRun 
{
    // Mac
    [MenuItem("Tools/Run Multiplayer for Mac/2 Players")]
    static void PerformMacBuild2()
    {
        PerformMacBuild(2);
    }

    [MenuItem("Tools/Run Multiplayer for Mac/3 Players")]
    static void PerformMacBuild3()
    {
        PerformMacBuild(3);
    }

    [MenuItem("Tools/Run Multiplayer for Mac/4 Players")]
    static void PerformMacBuild4()
    {
        PerformMacBuild(4);
    }

    // Windows64
    [MenuItem("Tools/Run Multiplayer for Win64/2 Players")]
    static void PerformWin64Build2()
    {
        PerformWin64Build(2);
    }

    [MenuItem("Tools/Run Multiplayer for Win64/3 Players")]
    static void PerformWin64Build3()
    {
        PerformWin64Build(3);
    }

    [MenuItem("Tools/Run Multiplayer for Win64/4 Players")]
    static void PerformWin64Build4()
    {
        PerformWin64Build(4);
    }



    static void PerformMacBuild(int playerCount)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);

        // 프로젝트 이름, 플레이어 번호를 이용하여 각기 다른 폴더에다가 exe 생성
        for(int i=1;i<=playerCount; i++)
        {
            BuildPipeline.BuildPlayer(GetScenePaths(),
                "Builds/MacOS/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".app",
                BuildTarget.StandaloneOSX, BuildOptions.AutoRunPlayer);
                
        }
    }

    static void PerformWin64Build(int playerCount)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        // 프로젝트 이름, 플레이어 번호를 이용하여 각기 다른 폴더에다가 exe 생성
        for (int i = 1; i <= playerCount; i++)
        {
            BuildPipeline.BuildPlayer(GetScenePaths(),
                "Builds/Win64/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".exe",
                BuildTarget.StandaloneWindows64, BuildOptions.AutoRunPlayer);

        }
    }

    static string GetProjectName()
    {
        // 정해진 양식
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    // 모든 Scene에 대한 경로 받아
    static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];

        for(int i=0;i< scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }

        return scenes;
    }
}
