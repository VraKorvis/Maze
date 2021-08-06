using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySettings : MonoBehaviour {
    public static AbilitySettings instance;

    [SerializeField]
    private Dropdown dropdownFirstAbility;

    [SerializeField]
    private Dropdown dropdownSecondAbility;

    private string[] abilitiesArray;

    private AbilityType abilityFirstButton;
    private AbilityType abilitySecondButton;

    public AbilityType AbilityFirstButton => abilityFirstButton;
    public AbilityType AbilitySecondButton => abilitySecondButton;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        //DontDestroyOnLoad(this);
        abilitiesArray = Enum.GetNames(typeof(AbilityType));

        dropdownFirstAbility.onValueChanged.AddListener(OnValueChangedFirstAbility);
        dropdownSecondAbility.onValueChanged.AddListener(OnValueChangedSecondAbility);
        dropdownFirstAbility.ClearOptions();
        dropdownSecondAbility.ClearOptions();
        dropdownFirstAbility.AddOptions(new List<string>(abilitiesArray));
        dropdownSecondAbility.AddOptions(new List<string>(abilitiesArray));
        dropdownFirstAbility.value = 0;
        dropdownSecondAbility.value = 1;
        abilityFirstButton = GetAbilityTypeFromString(abilitiesArray[0]);
        abilitySecondButton = GetAbilityTypeFromString(abilitiesArray[1]);

    }

    private void OnValueChangedFirstAbility(int value) {
        abilityFirstButton = GetAbilityTypeFromString(abilitiesArray[value]);
    }

    private void OnValueChangedSecondAbility(int value) {
        abilitySecondButton = GetAbilityTypeFromString(abilitiesArray[value]);
    }

    private AbilityType GetAbilityTypeFromString(string value) {
        Enum.TryParse(value, out AbilityType type);
        return type;
    }
}