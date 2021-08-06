using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract  class AbstractEnemyController : AbstractPlayerController {
    protected GameObject tPoint;
    protected LayerMask layer;
    protected bool spiningNow;

    [HideInInspector] public Vector3[] ways;

    public override void Move() {
        SetPosition();
        StartCoroutine(SmoothMovement());
    }

    public virtual IEnumerator SmoothMovement() {
        while (isMoving) {
            rb2d.MovePosition(rb2d.position + (Vector2)rb2dTransform.up * speed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    public virtual void SetPosition() {
        rb2dTransform.position = ways[0];
    }
    

}
