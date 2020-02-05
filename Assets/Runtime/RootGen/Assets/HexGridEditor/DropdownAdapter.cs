using System;
using System.Collections.Generic;
using TMPro;

public class DropdownAdapter {
    private TMP_Dropdown _dropdown;
    private Dictionary<int, Action> _actions;
    public DropdownAdapter(TMP_Dropdown dropdown) {
        _dropdown = dropdown;
        _actions = new Dictionary<int, Action>();
        _dropdown.onValueChanged.AddListener(OnSelect);
    }

    public void AddOption(string optionName, Action action) {
        TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
        data.text = optionName;
        _dropdown.options.Add(data);
        _actions.Add(_dropdown.options.Count - 1, action);
    }

    public void OnSelect(int option) {
        _actions[option].Invoke();
    }
    
    public void Clear() {
        _actions = new Dictionary<int, Action>();
        _dropdown.options.Clear();
    }
}