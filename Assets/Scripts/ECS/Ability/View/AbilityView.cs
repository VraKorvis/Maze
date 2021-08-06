using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Maze.Data.Storages;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AbilityView : MonoBehaviour {
    // need set 2 ability from Menu where chose Ability
    public Button[] abilityButtons;
    private Entity[] cashedButtEntities;

    private Entity[] cashedAbilityEntities;
    private int[] rankAbilities;

    private string jsonFromAbilityFileForAndroid = String.Empty;

    private ShamanAbilitiesData[] selectedAbility;

    public void Init() {
        var abilityInstance = AbilitySettings.instance;

        
        
        abilityButtons[0].onClick.AddListener(OnUseAbilityButton1Clicked);
        abilityButtons[1].onClick.AddListener(OnUseAbilityButton2Clicked);

        cashedAbilityEntities = new Entity[2];
        cashedButtEntities = new Entity[2];
        selectedAbility = new ShamanAbilitiesData[2];
        rankAbilities = new[] {1, 2};

       
        AbilityType[] abilitysType = new AbilityType[2];
        try {
            AbilityType firstButton = abilityInstance.AbilityFirstButton;
            AbilityType secondButton = abilityInstance.AbilitySecondButton;
            abilitysType[0] = firstButton;
            abilitysType[1] = secondButton;
        }
        catch (NullReferenceException e) {
            Debug.Log("Ability's buttons are not set. " + e);
            return;
        }

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var archetype = em.CreateArchetype(typeof(SharedButton));
        for (int i = 0; i < abilityButtons.Length; i++) {
            string fullPath = AbilityFactory.GetPath(abilitysType[i]);
            if (fullPath.Contains("://") || fullPath.Contains(":///")) {
                // #if UNITY_ANDROID
                //                 StartCoroutine(LoadAbilityJsonForAndroid(fullPath));
                //                 Debug.LogWarning("Loading www...... " + fullPath);
                // #endif
                // UnityWebRequest www = UnityWebRequest.Get(fullPath);
                // while (!www.isDone) {
                //     Debug.LogWarning("Loading www...... " + fullPath);
                // }
                //
                // jsonFromAbilityFileForAndroid = www.downloadHandler.text;
#if UNITY_ANDROID
                StartCoroutine(LoadAbilityJsonForAndroid(fullPath));
#endif
            }
            else {
                using (File.OpenRead(fullPath)) {
                    jsonFromAbilityFileForAndroid = File.ReadAllText(fullPath);
                }
            }

            selectedAbility[i] =
                AbilityFactory.CreateAbilityDataFromJson(abilitysType[i], jsonFromAbilityFileForAndroid);
            cashedAbilityEntities[i] = em.CreateEntity();
            cashedButtEntities[i] = em.CreateEntity(archetype);
            AbilityFactory.CreateAndAddAbilityToEntity(cashedAbilityEntities[i], abilitysType[i], selectedAbility[i],
                rankAbilities[i]);

            em.SetSharedComponentData(cashedButtEntities[i],
                new SharedButton() {
                    button = abilityButtons[i],
                });
        }

        for (int i = 0; i < abilitysType.Length; i++) {
            var abilityType = abilitysType[i];
            var text = abilityButtons[i].gameObject.GetComponentInChildren<Text>();
            if (text == null) continue;
            switch (abilityType) {
                case (AbilityType.Fear): text.text  = "Fear" ;
                    break;
                case (AbilityType.FrostNova): text.text  = "FrostNova" ;
                    break;
                case (AbilityType.Slowdown): text.text  = "Slowdown" ;
                    break;
                case (AbilityType.Jump): text.text  = "Jump" ;
                    break;
                case (AbilityType.BlinkAuto): text.text  = "BlinkAuto" ;
                    break;
                case (AbilityType.Invulnerability): text.text  = "Invulnerability" ;
                    break;
                case (AbilityType.HeroLights): text.text  = "HeroLights" ;
                    break;
                case (AbilityType.Sonar): text.text  = "Sonar" ;
                    break;
                case (AbilityType.ShowRag): text.text  = "ShowRag" ;
                    break;
                case (AbilityType.ShowHome): text.text  = "ShowHome" ;
                    break;
            }
        }
    }

    private IEnumerator LoadAbilityJsonForAndroid(string fullPath) {
        WWW www = new WWW(fullPath);
        if (!www.isDone) {
            yield return www;
            // Debug.LogWarning("Loading www...... " + fullPath);
        }

        //Debug.LogWarning("Load www...... " + www.text);
        jsonFromAbilityFileForAndroid = www.text;

        // TODO Check next code, doesnt work on android
        //  UnityWebRequest www = UnityWebRequest.Get(fullPath);
        //  yield return www.SendWebRequest();
        // jsonFromAbilityFileForAndroid = www.downloadHandler.text;
    }

    // Push ability button1, add ActivateAbilityTag, add Cooldown
    private void OnUseAbilityButton1Clicked() {
        if (ExistActivateAbility()) return;
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        em.AddComponentData(cashedAbilityEntities[0], new ActivateAbilityTag());

        em.AddComponentData(cashedButtEntities[0],
            new CooldownAbilityButton() {
                timer = selectedAbility[0].GetCooldown()[rankAbilities[0]],
            });
    }

    private bool ExistActivateAbility() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        foreach (var ab in cashedAbilityEntities) {
            if (em.HasComponent<ActivateAbilityTag>(ab)) {
                return true;
            }
        }

        return false;
    }

    // Push ability button2, add ActivateAbilityTag
    private void OnUseAbilityButton2Clicked() {
        if (ExistActivateAbility()) return;
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        em.AddComponentData(cashedAbilityEntities[1], new ActivateAbilityTag());
        em.AddComponentData(cashedButtEntities[1],
            new CooldownAbilityButton() {
                timer = selectedAbility[1].GetCooldown()[rankAbilities[1]],
            });
    }

    private void Update() {
        if (abilityButtons[0].interactable && Input.GetKeyUp(KeyCode.Alpha1)) {
            OnUseAbilityButton1Clicked();
        }

        if (abilityButtons[1].interactable && Input.GetKeyUp(KeyCode.Alpha2)) {
            OnUseAbilityButton2Clicked();
        }
    }
}