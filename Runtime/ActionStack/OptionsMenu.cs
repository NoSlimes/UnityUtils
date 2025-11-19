using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

#if false
public class OptionsMenu : ActionMonoBehaviour
{
    [SerializeField] private SettingControlPair[] settingControlPairs = Array.Empty<SettingControlPair>();
    [SerializeField] private Button applyButton, backButton;

    private bool shouldExit = false;
    private Dictionary<Selectable, object> previousSettings;

    private Coroutine applyButtonRoutine;

    private void Awake()
    {
        applyButton.onClick.AddListener(() =>
        {
            ApplySettings();
            UpdateApplyButton();
        });
        backButton.onClick.AddListener(() =>
        {
            RevertSettings();
            shouldExit = true;
        });
    }

    public override void OnStart(bool firstTime)
    {
        base.OnStart(firstTime);

        previousSettings = new Dictionary<Selectable, object>();

        foreach (var pair in settingControlPairs)
        {
            object loadedValue = LoadSetting(pair);
            previousSettings[pair.ControlObject] = loadedValue;
            SetControlValue(pair.ControlObject, loadedValue);
        }

        shouldExit = false;
        gameObject.SetActive(true);

        applyButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Apply";
    }


    private void RevertSettings()
    {
        foreach (var pair in settingControlPairs)
        {
            if (previousSettings.TryGetValue(pair.ControlObject, out var value))
            {
                if (pair.ControlObject is Toggle toggle && value is bool boolValue)
                {
                    toggle.isOn = boolValue;
                }
                else if (pair.ControlObject is Slider slider && value is float floatValue)
                {
                    slider.value = floatValue;
                }
            }
        }
    }

    private void ApplySettings()
    {
        foreach (var pair in settingControlPairs)
        {
            if (pair.ControlObject is Toggle toggle)
            {
                PlayerPrefs.SetInt(pair.SettingName, toggle.isOn ? 1 : 0);
            }
            else if (pair.ControlObject is Slider slider)
            {
                PlayerPrefs.SetFloat(pair.SettingName, slider.value);
            }
        }

        PlayerPrefs.Save();
    }

    private void SetControlValue(Selectable control, object value)
    {
        if (control is Toggle toggle && value is bool boolValue)
        {
            toggle.isOn = boolValue;
        }
        else if (control is Slider slider && value is float floatValue)
        {
            slider.value = floatValue;
        }
    }

    private object LoadSetting(SettingControlPair pair)
    {
        if (pair.ControlObject is Toggle)
        {
            return PlayerPrefs.GetInt(pair.SettingName, 0) == 1;
        }
        else if (pair.ControlObject is Slider slider)
        {
            return PlayerPrefs.GetFloat(pair.SettingName, slider.value);
        }

        return null;
    }

    public override void OnFinish()
    {
        base.OnFinish();

        gameObject.SetActive(false);
        previousSettings.Clear();
    }

    private void UpdateApplyButton()
    {
        if(applyButtonRoutine != null)
        {
            StopCoroutine(applyButtonRoutine);
        }

        applyButtonRoutine = StartCoroutine(FlashButtonText("Apply", "Applied!"));
    }

    private IEnumerator FlashButtonText(string origText, string intermText)
    {
        var TMP = applyButton.GetComponentInChildren<TMPro.TMP_Text>();
        if (TMP != null)
        {
            yield return TextTyper.TypeJitterDuration(TMP, intermText, 0.15f);
            yield return new WaitForSeconds(0.5f);
            yield return TextTyper.TypeJitterDuration(TMP, origText, 0.15f);
        }
    }

    public override bool IsDone()
    {
        return shouldExit;
    }

    [System.Serializable]
    public struct SettingControlPair
    {
        public string SettingName;
        public Selectable ControlObject;
    }
}
#endif