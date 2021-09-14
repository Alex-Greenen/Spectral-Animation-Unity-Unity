using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class StateDropdown : MonoBehaviour
{

    private TMP_Dropdown dropdown;
    private SkeletonController skeleton;
    // Start is called before the first frame update
    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        skeleton = FindObjectsOfType<SkeletonController>()[0];
        PopulateDropDownWithEnum(dropdown, skeleton.currentState);

        dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdown); });//Add listener to Event
    }

    public static void PopulateDropDownWithEnum(TMP_Dropdown dropdown, Enum targetEnum)//You can populate any dropdown with any enum with this method
    {
        Type enumType = targetEnum.GetType();//Type of enum(FormatPresetType in my example)
        List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

        for (int i = 0; i < Enum.GetNames(enumType).Length; i++)//Populate new Options
        {
            newOptions.Add(new TMP_Dropdown.OptionData(Enum.GetName(enumType, i)));
        }

        dropdown.ClearOptions();//Clear old options
        dropdown.AddOptions(newOptions);//Add new options

        dropdown.value = (int) (SkeletonController.State) targetEnum;
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        skeleton.currentState = (SkeletonController.State) change.value; //Convert dropwdown value to enum
    }
}