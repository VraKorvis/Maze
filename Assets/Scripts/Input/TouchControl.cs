using UnityEngine;
using System.Collections;
using Cinemachine;

public class TouchControl : MonoBehaviour {
    [SerializeField] private AbstractPlayerController player;

    [SerializeField] private CinemachineVirtualCamera vCam;
    public float speed = 1.0f;

    private Vector3 startPos;
    private bool isMoving;

    private bool drag = false;
    private bool zoom = false;

    private Vector3 initialTouchPosition;
    private Vector3 initialCameraPosition;
    private Vector3 initialTouch0Position;
    private Vector3 initialTouch1Position;
    private Vector3 initialMidPointScreen;
    private float initialOrthographicSize;

    void Update() {
        int touchCount = Input.touchCount;
        if (((PlayerController) player).isSpiningNow) return;
        if (touchCount == 1) {
            Turn(Input.touches[0].position);
        }
        //if (Input.touchCount == 1) {
        //    zoom = false;
        //    Touch touch0 = Input.GetTouch(0);
        //    if (IsTouching(touch0)) {
        //        if (!drag) {
        //            initialTouchPosition = touch0.position;
        //            initialCameraPosition = this.transform.position;
        //            drag = true;
        //        } else {
        //            Vector2 delta = Camera.main.ScreenToWorldPoint(touch0.position) - Camera.main.ScreenToWorldPoint(initialTouchPosition);

        //            Vector3 newPos = initialCameraPosition;
        //            newPos.x -= delta.x;
        //            newPos.y -= delta.y;
        //            this.transform.position = newPos;
        //        }
        //    }
        //    if (!IsTouching(touch0)) {
        //        drag = false;
        //    }
        //} else {
        //    drag = false;
        //}

        if (Input.touchCount == 2) {
            drag = false;

            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (!zoom) {
                initialTouch0Position = touch0.position;
                initialTouch1Position = touch1.position;
                initialCameraPosition = this.transform.position;
                initialOrthographicSize = Camera.main.orthographicSize;
                initialMidPointScreen = (touch0.position + touch1.position) / 2;

                zoom = true;
            } else {
                this.transform.position = initialCameraPosition;
                Camera.main.orthographicSize = initialOrthographicSize;

                float scaleFactor = GetScaleFactor(touch0.position,
                    touch1.position,
                    initialTouch0Position,
                    initialTouch1Position);

                Vector2 currentMidPoint = (touch0.position + touch1.position) / 2;
                Vector3 initialPointWorldBeforeZoom = Camera.main.ScreenToWorldPoint(initialMidPointScreen);

                Camera.main.orthographicSize = initialOrthographicSize / scaleFactor;
                vCam.m_Lens.OrthographicSize = initialOrthographicSize / scaleFactor;

                Vector3 initialPointWorldAfterZoom = Camera.main.ScreenToWorldPoint(initialMidPointScreen);
                Vector2 initialPointDelta = initialPointWorldBeforeZoom - initialPointWorldAfterZoom;

                Vector2 oldAndNewPointDelta =
                    Camera.main.ScreenToWorldPoint(currentMidPoint) -
                    Camera.main.ScreenToWorldPoint(initialMidPointScreen);

                Vector3 newPos = initialCameraPosition;
                newPos.x -= oldAndNewPointDelta.x - initialPointDelta.x;
                newPos.y -= oldAndNewPointDelta.y - initialPointDelta.y;

                this.transform.position = newPos;
            }
        } else {
            zoom = false;
        }
    }

    static bool IsTouching(Touch touch) {
        return touch.phase == TouchPhase.Began ||
               touch.phase == TouchPhase.Moved ||
               touch.phase == TouchPhase.Stationary;
    }

    public static float GetScaleFactor(Vector2 position1, Vector2 position2, Vector2 oldPosition1,
        Vector2 oldPosition2) {
        float distance = Vector2.Distance(position1, position2);
        float oldDistance = Vector2.Distance(oldPosition1, oldPosition2);

        if (oldDistance == 0 || distance == 0) {
            return 1.0f;
        }

        return distance / oldDistance;
    }

    private void Turn(Vector2 pos) {
        Vector2 dist = Camera.main.ScreenToViewportPoint(pos);
        if (dist.x > 0.5) {
            ((PlayerController) player).TurnTo(Direction.Right);
        } else if (dist.x < 0.5) {
            ((PlayerController) player).TurnTo(Direction.Left);
        }
    }
}