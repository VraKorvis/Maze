using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

[UpdateInGroup(typeof(PreSimulationSystemGroup))]
public class TouchInputSystem : ComponentSystem {
    private EntityQuery worldTurnOffGroup;

//    public delegate void ClickAction(Entity entity);
//    public static event ClickAction OnClicked = delegate {};
//
//    private void OnClick(Entity entity, ClickAction clickAction) {}

    protected override void OnCreate() {
        worldTurnOffGroup = GetEntityQuery(typeof(WorldTurn));

//        Entity entity = Entity.Null;
//        OnClick(entity, (ent) => {
//            Debug.Log($"Click: {ent}");
//        });
    }

    public float speed = 1.0f;

    private Vector3 startPos;
    private bool isMoving;

//    private bool drag = false;
//    private bool zoom = false;

    private Vector3 initialTouchPosition;
    private Vector3 initialCameraPosition;
    private Vector3 initialTouch0Position;
    private Vector3 initialTouch1Position;
    private Vector3 initialMidPointScreen;
    private float initialOrthographicSize;

    protected override void OnUpdate() {
#if UNITY_EDITOR || PLATFORM_STANDALONE_WIN
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            Vector3 touchPos = Input.mousePosition;
            CreateClickToTurnOff(touchPos);
        }
#else
        int touchCount = Input.touchCount;
        if (touchCount == 1) {
            Vector3 touchPos = Input.touches[0].position;
            if (Input.touches[0].phase == TouchPhase.Began) {
               CreateClickToTurnOff(touchPos);
            }
        }
#endif
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

        //TODO ZOOM look TouchControll

//		if (Input.touchCount == 2) {
//			drag = false;
//
//			Touch touch0 = Input.GetTouch(0);
//			Touch touch1 = Input.GetTouch(1);
//
//			if (!zoom) {
//				initialTouch0Position   = touch0.position;
//				initialTouch1Position   = touch1.position;
//				initialCameraPosition   = this.transform.position;
//				initialOrthographicSize = Camera.main.orthographicSize;
//				initialMidPointScreen   = (touch0.position + touch1.position) / 2;
//
//				zoom = true;
//			} else {
//				this.transform.position      = initialCameraPosition;
//				Camera.main.orthographicSize = initialOrthographicSize;
//
//				float scaleFactor = GetScaleFactor(touch0.position,
//					touch1.position,
//					initialTouch0Position,
//					initialTouch1Position);
//
//				Vector2 currentMidPoint             = (touch0.position + touch1.position) / 2;
//				Vector3 initialPointWorldBeforeZoom = Camera.main.ScreenToWorldPoint(initialMidPointScreen);
//
//				Camera.main.orthographicSize = initialOrthographicSize / scaleFactor;
//				vCam.m_Lens.OrthographicSize = initialOrthographicSize / scaleFactor;
//
//				Vector3 initialPointWorldAfterZoom = Camera.main.ScreenToWorldPoint(initialMidPointScreen);
//				Vector2 initialPointDelta          = initialPointWorldBeforeZoom - initialPointWorldAfterZoom;
//
//				Vector2 oldAndNewPointDelta =
//					Camera.main.ScreenToWorldPoint(currentMidPoint) -
//					Camera.main.ScreenToWorldPoint(initialMidPointScreen);
//
//				Vector3 newPos = initialCameraPosition;
//				newPos.x -= oldAndNewPointDelta.x - initialPointDelta.x;
//				newPos.y -= oldAndNewPointDelta.y - initialPointDelta.y;
//
//				this.transform.position = newPos;
//			}
//		} else {
//			zoom = false;
//		}
    }

    //TODO duct tape
    private void CreateClickToTurnOff(Vector3 touchPos) {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        Vector2 dist = Camera.main.ScreenToViewportPoint(touchPos);
        if (dist.y < 0.28f) {
            return;
        }

        Clockwise clockwise = Clockwise.Clock;
        if (dist.x > 0.5) {
            clockwise = Clockwise.Clock;
        }
        else if (dist.x < 0.5) {
            clockwise = Clockwise.CounterClock;
        }

        var worldTurnOffEntity = worldTurnOffGroup.GetSingletonEntity();
        var worldTurnOff = em.GetComponentData<WorldTurn>(worldTurnOffEntity);
        if (worldTurnOff.isSpinningNow) {
            if (em.HasComponent<ClickToTurn>(worldTurnOffEntity)) {
                var click = em.GetComponentData<ClickToTurn>(worldTurnOffEntity);
                //TODO hardcode, test turn off mode
                bool block = DebugSettings.Instance.BlockSpam;
                if (block && click.Clockwise == clockwise) return;
                em.RemoveComponent<ClickToTurn>(worldTurnOffEntity);
            }

            if (em.HasComponent<TurnTag>(worldTurnOffEntity)) {
                em.RemoveComponent<TurnTag>(worldTurnOffEntity);
            }
        }

        // var entity = em.CreateEntity();
        em.AddComponentData(worldTurnOffEntity, new ClickToTurn() {
            Clockwise = clockwise,
            needCalculateAngle = true,
        });
    }
}