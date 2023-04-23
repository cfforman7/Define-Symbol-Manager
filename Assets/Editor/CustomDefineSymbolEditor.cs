using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Unity.Plastic.Newtonsoft.Json;
using System.Linq;
/// <summary>
/// 프로젝트 내의 DefineSymbol을 찾아 On/ Off를 일괄 관리하는 Tool 입니다.
/// </summary>
public class CustomDefineSymbolEditor : EditorWindow
{
    [System.Serializable]
    public class DefineSymbolDataList
    {
        public List<DefineSymbolData> defineSymbolList = new List<DefineSymbolData>();
    }

    [System.Serializable]
    public class DefineSymbolData
    {
        public string key;
        public bool value;
        public string desc;
    }

    public const string DIRECTORY_KEY = "DIRECTORY_KEY";
    public const string FILE_KEY = "FILE_KEY";

    private DefineSymbolDataList defineData = new DefineSymbolDataList();
    private List<string> applyDefineSymbolList = new List<string>();
    private string assetsPath = "Assets/";
    private string directoryPath = "DefineSymbol/";
    private string fileName = "defineSymbols.json";

    private string editKey = "";
    private bool editValue = false;
    private string editDesc = "";

    /// <summary>파일이 존재하는가 여부</summary>
    private bool isExistFile = false;

    private Vector2 scrollPosition;
    [MenuItem("Tools/Custom Define Symbol")]
    public static void OpenWindow()
    {
        CustomDefineSymbolEditor window = GetWindow<CustomDefineSymbolEditor>();
        window.titleContent = new GUIContent("Define Symbols Manager");
        window.minSize = new Vector2(700, 500);
        window.maxSize = new Vector2(800, 700);
        window.LoadDefineSymbolFile();
        window.Show();
    }

    /// <summary>
    /// 파일이 존재 하는가 읽어들인다.
    /// </summary>
    private void LoadDefineSymbolFile()
    {
        if (applyDefineSymbolList == null)
        {
            applyDefineSymbolList = new List<string>();
        }
        if (isExistFile == false) return;
        //파일을 읽어 와서 Dictionary에 대입
        string jsonString = File.ReadAllText(assetsPath + directoryPath + fileName);
        if (string.IsNullOrEmpty(jsonString) == false)
        {
            defineData = JsonUtility.FromJson<DefineSymbolDataList>(jsonString);
        }
    }

    /// <summary>
    /// DefineSymbol .Json 파일을 적용 및 저장한다.
    /// </summary>
    private void SaveDefineSymbolJsonFile()
    {
        string jsonString = JsonUtility.ToJson(defineData);
        File.WriteAllText(assetsPath + directoryPath + fileName, jsonString);
    }

    private void OnEnable()
    {
        //로컬에 저장된 경로가 존재 하는지 체크
        string directoryValue = PlayerPrefs.GetString(DIRECTORY_KEY);
        if (string.IsNullOrEmpty(directoryValue) == false)
        {
            directoryPath = directoryValue;
        }
        //로컬에 저장된 파일 이름이 존재 하는지 체크
        string fileValue = PlayerPrefs.GetString(FILE_KEY);
        if (string.IsNullOrEmpty(fileValue) == false)
        {
            fileName = fileValue;
        }
        //파일이 존재 하는지 체크
        isExistFile = File.Exists(assetsPath + directoryPath + fileName);
    }

    private void OnGUI()
    {
        //파일 정보 보여주기
        DrawPathInfo();
        //Define 리스트 보여주기
        DrawDefineSymbolListView();
        DrawLine(5);
        //새로운 Define 추가 보여주기
        DrawAddSymbolContent();
        DrawLine(10);
        //프로젝트에 적용하기
        ApplyAllScripts();
    }

    /// <summary>
    /// 패스 정보를 보여줍니다.
    /// </summary>
    private void DrawPathInfo()
    {
        DrawLine(5);
        GUILayout.BeginVertical();
        GUILayout.Label("저장 하려는 파일 경로를 설정 하세요.");
        GUILayout.Label("해당 경로는 파일을 로드 할 때도 사용합니다. 파일이 없으면 생성합니다.");
        GUILayout.EndVertical();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        directoryPath = EditorGUILayout.TextField("경로 : Assets/", directoryPath);        
        fileName = EditorGUILayout.TextField("파일 이름 : ", fileName);
        GUILayout.EndVertical();
        string infoText = isExistFile ? "Open" : "Create";
        if (GUILayout.Button(infoText, GUILayout.Width(60), GUILayout.Height(37)))
        {
            //폴더 경로를 체크하여 폴더 생성
            CreateDirectory();
            if (isExistFile == false)
            {
                if (this.defineData == null)
                {
                    this.defineData = new DefineSymbolDataList();
                }

                //파일이 없으면 파일 생성
                string defineData = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                string[] defineSymbols = defineData.Split(';');
                for (int i = 0; i < defineSymbols.Length; i++)
                {
                    if (string.IsNullOrEmpty(defineSymbols[i]) == false)
                    {
                        DefineSymbolData data = new DefineSymbolData();
                        data.key = defineSymbols[i];
                        data.value = true;
                        data.desc = string.Empty;
                        this.defineData.defineSymbolList.Add(data);
                    }
                }
                string convert = JsonUtility.ToJson(this.defineData);
                File.WriteAllText(assetsPath + directoryPath + fileName, convert);
                isExistFile = File.Exists(assetsPath + directoryPath + fileName);
            }
            string openPath = Application.dataPath + "/" + directoryPath;
            Process.Start(openPath);
        }
        GUILayout.EndHorizontal();
        DrawLine(10);
    }

    /// <summary>
    /// Define Symbol 보여주기
    /// </summary>
    private void DrawDefineSymbolListView()
    {
        if (defineData == null) return;
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (defineData.defineSymbolList.Count > 0)
        {
            for (int i = 0; i < defineData.defineSymbolList.Count; i++)
            {
                string key = defineData.defineSymbolList[i].key;
                bool value = defineData.defineSymbolList[i].value;
                string desc = defineData.defineSymbolList[i].desc;
                //리스트에 저장
                ApplyDefineSymbolList(key, value);
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal();
                bool newValue = EditorGUILayout.ToggleLeft("On/Off", value, GUILayout.Width(65));
                GUILayout.Space(15);
                GUILayout.Label("Define Name: ", GUILayout.Width(80));
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.TextField(key, GUILayout.Width(150));
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(15);
                GUILayout.Label("Desc: ", GUILayout.Width(40));
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.TextField(desc, GUILayout.Width(250));
                EditorGUI.EndDisabledGroup();
                
                if (GUILayout.Button(new GUIContent("Delete", "실수를 대비하여 저장을 누르지 않는 한 적용되지 않습니다."), GUILayout.ExpandWidth(true)))
                {
                    ApplyDefineSymbolList(key, false);
                    defineData.defineSymbolList.RemoveAt(i);
                    //SaveDefineSymbols();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    break;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                if (value != newValue)
                {
                    defineData.defineSymbolList[i].value = newValue;
                    ApplyDefineSymbolList(key, newValue);
                    //SaveDefineSymbols();
                    break;
                }
            }
        }
        EditorGUILayout.EndScrollView();        
    }

    /// <summary>
    /// 폴더 만들기
    /// </summary>
    private void CreateDirectory()
    {
        string[] folders = directoryPath.Split('/');
        string path = folders[0];
        for (int i = 1; i < folders.Length; i++)
        {
            if (string.IsNullOrEmpty(folders[i])) continue;
            path = Path.Combine(path, folders[i]);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    /// <summary>
    /// 적용 되어야할 DefineList 
    /// </summary>
    private void ApplyDefineSymbolList(string key, bool value)
    {
        if (value)
        {
            if (applyDefineSymbolList.Contains(key) == false)
            {
                applyDefineSymbolList.Add(key);
            }
        }
        else
        {
            if (applyDefineSymbolList.Contains(key) == true)
            {
                applyDefineSymbolList.Remove(key);
            }
        }
    }

    /// <summary>
    /// 새로운 Define 추가 하기
    /// </summary>
    private void DrawAddSymbolContent()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("새로운 DefineSymbol 을 추가 합니다.", style);
        GUILayout.Label("파일에는 저장되지만 스크립트에는 적용되지 않습니다. 꼭 저장을 누르세요", style);
        GUILayout.Space(5);
        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        editValue = EditorGUILayout.ToggleLeft("On/Off", editValue, GUILayout.Width(65));
        GUILayout.Space(15);
        GUILayout.Label("Define Name: ", GUILayout.Width(80));
        editKey = EditorGUILayout.TextField(editKey, GUILayout.Width(150));
        GUILayout.Space(15);
        GUILayout.Label("Desc: ", GUILayout.Width(40));
        editDesc = EditorGUILayout.TextField(editDesc, GUILayout.Width(250));
        if (GUILayout.Button("ADD", GUILayout.ExpandWidth(true)))
        {
            if (string.IsNullOrEmpty(editKey)) return;
            DefineSymbolData data = new DefineSymbolData();
            data.key = editKey;
            data.value = editValue;
            data.desc = editDesc;
            defineData.defineSymbolList.Add(data);
            ApplyDefineSymbolList(editKey, editValue);
            SaveDefineSymbolJsonFile();
            editKey = "";
            editValue = false;
            editDesc = "";
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 스크립트에 적용하기
    /// </summary>
    private void ApplyAllScripts()
    {
        if (GUILayout.Button("상태 저장 및 스크립트에 적용하기"))
        {
            SaveDefineSymbolJsonFile();
            string define = string.Join(";", applyDefineSymbolList.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, define);
            //EditorUtility.DisplayDialog("결과", "저장 및 적용 되었습니다.", "확인");
        }
        GUILayout.Space(10);
    }

    /// <summary>라인 그리기</summary>
    private void DrawLine(int aSpace)
    {
        GUILayout.Space(aSpace);
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(aSpace);
    }
}
