using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RootExtensions;
using RootUtils;
using RootLogging;

public class LoadConfigMenu : MonoBehaviour
{
    public Transform ContentTransform;
    public Button GenerateMapButton;
    private bool _active;
    private RootGenConfigData _activeData;
    private List<Button> buttons;
    
    private void Awake() {
        GenerateMapButton.onClick.AddListener(GenerateMap);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable() {
        LoadItems();
    }

    private void OnDisable() {
        ClearItems();
    }

    public void ToggleShowHide() {
        if (_active) {
            Hide();
        }
        else {
            Show();
        }
    }

    public void Show() {
        _active = true;
        RootLog.Log("Setting LoadConfig to active.");
        this.gameObject.SetActive(true);
    }

    public void Hide() {
        _active = false;
        RootLog.Log("Setting LoadConfig to active.");
        this.gameObject.SetActive(false);
    }

    public string[] GetPersistentData(string extension) {
        string[] result =
            Directory.GetFiles(Application.persistentDataPath, "*." + extension);
        Array.Sort(result);
        return result;
    }

    public string[] GetPersistentDataFileNames(string extension) {
        string[] result =
            Directory.GetFiles(Application.persistentDataPath, "*." + extension);
        Array.Sort(result);

        for (int i = 0; i < result.Length; i++) {
            result[i] = Path.GetFileNameWithoutExtension(result[i]);
        }

        return result;
    }

    public void LoadItems() {
        string[] names = GetPersistentDataFileNames("json");
        foreach (string name in names) {
            GameObject buttonObj = new GameObject("Load " + name + " Button");
            Button buttonMono = buttonObj.AddComponent<Button>();
            buttonObj.AddComponent<Image>();
            buttonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(
                100f,
                30f
            );

            GameObject textObj = new GameObject("Load " + name + " Text");
            textObj.SetParent(buttonObj, false);

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = name;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontSize = 14;
            buttonText.color = Color.black;
            buttonText.font = UnityBuiltin.Font("Arial");

            textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(
                100f,
                30f
            );

            buttonObj.transform.SetParent(ContentTransform, false);
            buttonObj.AddComponent<LayoutElement>();
            buttonMono.onClick.AddListener(
                () => { LoadItem(name); }
            );
        }
    }

    public void LoadItem(string name) {
        _activeData = RootGenConfigData.Load(name);
        RootLog.Log(name + " loaded.", Severity.Information, "LoadConfigMenu");
    }

    private void ClearItems() {
        foreach (Transform transform in ContentTransform) {
            Destroy(transform.gameObject);
        }
    }

    private void GenerateMap() {
        if (_activeData != null) {
            RootGen.GenerateMap(this, _activeData);
        }
        else {
            RootLog.Log(
                "No active data to load.",
                Severity.Warning,
                "LoadConfigMenu"
            );
        }
    }
}
