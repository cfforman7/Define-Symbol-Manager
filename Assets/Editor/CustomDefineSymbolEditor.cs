using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Unity.Plastic.Newtonsoft.Json;
using System.Linq;
/// <summary>
/// ������Ʈ ���� DefineSymbol�� ã�� On/ Off�� �ϰ� �����ϴ� Tool �Դϴ�.
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

    /// <summary>������ �����ϴ°� ����</summary>
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
    /// ������ ���� �ϴ°� �о���δ�.
    /// </summary>
    private void LoadDefineSymbolFile()
    {
        if (applyDefineSymbolList == null)
        {
            applyDefineSymbolList = new List<string>();
        }
        if (isExistFile == false) return;
        //������ �о� �ͼ� Dictionary�� ����
        string jsonString = File.ReadAllText(assetsPath + directoryPath + fileName);
        if (string.IsNullOrEmpty(jsonString) == false)
        {
            defineData = JsonUtility.FromJson<DefineSymbolDataList>(jsonString);
        }
    }

    /// <summary>
    /// DefineSymbol .Json ������ ���� �� �����Ѵ�.
    /// </summary>
    private void SaveDefineSymbolJsonFile()
    {
        string jsonString = JsonUtility.ToJson(defineData);
        File.WriteAllText(assetsPath + directoryPath + fileName, jsonString);
    }

    private void OnEnable()
    {
        //���ÿ� ����� ��ΰ� ���� �ϴ��� üũ
        string directoryValue = PlayerPrefs.GetString(DIRECTORY_KEY);
        if (string.IsNullOrEmpty(directoryValue) == false)
        {
            directoryPath = directoryValue;
        }
        //���ÿ� ����� ���� �̸��� ���� �ϴ��� üũ
        string fileValue = PlayerPrefs.GetString(FILE_KEY);
        if (string.IsNullOrEmpty(fileValue) == false)
        {
            fileName = fileValue;
        }
        //������ ���� �ϴ��� üũ
        isExistFile = File.Exists(assetsPath + directoryPath + fileName);
    }

    private void OnGUI()
    {
        //���� ���� �����ֱ�
        DrawPathInfo();
        //Define ����Ʈ �����ֱ�
        DrawDefineSymbolListView();
        DrawLine(5);
        //���ο� Define �߰� �����ֱ�
        DrawAddSymbolContent();
        DrawLine(10);
        //������Ʈ�� �����ϱ�
        ApplyAllScripts();
    }

    /// <summary>
    /// �н� ������ �����ݴϴ�.
    /// </summary>
    private void DrawPathInfo()
    {
        DrawLine(5);
        GUILayout.BeginVertical();
        GUILayout.Label("���� �Ϸ��� ���� ��θ� ���� �ϼ���.");
        GUILayout.Label("�ش� ��δ� ������ �ε� �� ���� ����մϴ�. ������ ������ �����մϴ�.");
        GUILayout.EndVertical();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        directoryPath = EditorGUILayout.TextField("��� : Assets/", directoryPath);        
        fileName = EditorGUILayout.TextField("���� �̸� : ", fileName);
        GUILayout.EndVertical();
        string infoText = isExistFile ? "Open" : "Create";
        if (GUILayout.Button(infoText, GUILayout.Width(60), GUILayout.Height(37)))
        {
            //���� ��θ� üũ�Ͽ� ���� ����
            CreateDirectory();
            if (isExistFile == false)
            {
                if (this.defineData == null)
                {
                    this.defineData = new DefineSymbolDataList();
                }

                //������ ������ ���� ����
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
    /// Define Symbol �����ֱ�
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
                //����Ʈ�� ����
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
                
                if (GUILayout.Button(new GUIContent("Delete", "�Ǽ��� ����Ͽ� ������ ������ �ʴ� �� ������� �ʽ��ϴ�."), GUILayout.ExpandWidth(true)))
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
    /// ���� �����
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
    /// ���� �Ǿ���� DefineList 
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
    /// ���ο� Define �߰� �ϱ�
    /// </summary>
    private void DrawAddSymbolContent()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("���ο� DefineSymbol �� �߰� �մϴ�.", style);
        GUILayout.Label("���Ͽ��� ��������� ��ũ��Ʈ���� ������� �ʽ��ϴ�. �� ������ ��������", style);
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
    /// ��ũ��Ʈ�� �����ϱ�
    /// </summary>
    private void ApplyAllScripts()
    {
        if (GUILayout.Button("���� ���� �� ��ũ��Ʈ�� �����ϱ�"))
        {
            SaveDefineSymbolJsonFile();
            string define = string.Join(";", applyDefineSymbolList.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, define);
            //EditorUtility.DisplayDialog("���", "���� �� ���� �Ǿ����ϴ�.", "Ȯ��");
        }
        GUILayout.Space(10);
    }

    /// <summary>���� �׸���</summary>
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
