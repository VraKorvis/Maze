using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class EnemyWayPointMove : AbstractEnemyController {
    
    public void Awake() {
        base.Init();
    }

    public override IEnumerator TurnCharacter() {
        yield return null;
    }

    public override void Move() {
        isMoving = true;
        StartCoroutine(SmoothMovement());
    }

    public override IEnumerator SmoothMovement() {
        while (isMoving) {
            for (int i = 1; i < ways.Length; i++) {
                Vector2 end = ways[i];
                Vector2 start = rb2dTransform.position;
                float dist = Vector2.Distance(start, end);
                float time = 0;
                while (time < 1) {
                    Vector2 newPostion = Vector3.Lerp(start, end, time);
                    rb2d.MovePosition(newPostion);
                    time += 1 / dist * Time.fixedDeltaTime * speed;
                    yield return new WaitForFixedUpdate();
                }
            }
        }
    }



    protected IEnumerator SmoothMovement2() {
        while (isMoving) {
            for (int i = 1; i < ways.Length; i++) {
                Vector2 end = ways[i];
                Vector2 start = rb2dTransform.position;
                float sqrRemainingDistance = (start - end).sqrMagnitude;
                float time = 0;
                while (sqrRemainingDistance > float.Epsilon) {
                    // Vector3 newPostion = Vector3.MoveTowards(rb2d.position, end, inverseMoveTime * Time.fixedDeltaTime);
                    Vector2 newPostion = Vector3.Lerp(start, end, time);
                    rb2d.MovePosition(newPostion);
                    sqrRemainingDistance = (start - end).sqrMagnitude;
                    time += Time.fixedDeltaTime;
                    yield return null;
                }
            }
        }
    }
}