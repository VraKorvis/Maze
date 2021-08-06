using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class CooldownAbilitySystem : ComponentSystem {
    protected override void OnUpdate() {
        Entities.WithAll<SharedButton, CooldownAbilityButton>().ForEach(
            (Entity e, ref CooldownAbilityButton cooldown) => {
                cooldown.timer -= Time.DeltaTime;
                var but = EntityManager.GetSharedComponentData<SharedButton>(e);
                but.button.interactable = false;
                var text = but.button.GetComponentInChildren<Text>();
                text.enabled = true;
                text.text = ((int) cooldown.timer).ToString();
                if (cooldown.timer < 0) {
                    PostUpdateCommands.RemoveComponent<CooldownAbilityButton>(e);
                    but.button.interactable = true;
                    text.enabled = false;
                }
            });
    }
}