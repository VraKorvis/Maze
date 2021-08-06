using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerController : AbstractPlayerController {
    public bool isSpiningNow;

    [Tooltip("Direction of movement. [0,1] - Up, [1.0] - right, etc")]
    public Vector3 vector_direction;

    [Space] [Header("Tilemap, grid")] [SerializeField] [Tooltip("Ground layer")]
    private Tilemap groundTilemap;

    [SerializeField] [Tooltip("Wall layer. character check wall during the moving")]
    private Tilemap wallTilemap;

    [SerializeField] private Tilemap shadowTilemap;

    [HideInInspector] private GridInformation gridInf;

    [SerializeField] [Tooltip("Main grid of maze")]
    private GridLayout gridLayout;

    [SerializeField] private int gridSize = 1;

    private readonly Vector3 cellLocalPos = new Vector3(0.5f, 0.4f, 0.0f);

//    [SerializeField]
//    private float smoothCam = 0.05f;

    [Range(0.1f, 1f)] [Tooltip("Slowdown speed when turn. During the turn: speed = speed * motionCoef")]
    public float motionCoef;

    [Space] [Header("Just for test.Settings")] [SerializeField]
    private Slider slowDownSpeed;

    [SerializeField] private Slider characterSpeedSlider;

    [SerializeField] private Slider rotateSpeedSlider;

    [SerializeField] private Slider timeCountSLider;

    [SerializeField] private float timeCountForTurn = 0.5f;

    void Awake() {
        base.Init();
    }

    void Start() {
        gridInf = gridLayout.GetComponent<GridInformation>();
    }

    public void SetPlayerPosition(Vector3 pos) {
        StopAllCoroutines();
        Vector3Int cellPosition = gridLayout.LocalToCell(pos);
        rb2dTransform.position = gridLayout.CellToLocalInterpolated(cellPosition + cellLocalPos);
        rb2dTransform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public override void Move() {
        isMoving = true;
        StartCoroutine(SmoothMove());
    }

    public IEnumerator SmoothMove() {
        while (true) {
            while (isMoving) {
                motionCoef = slowDownSpeed.value;
                float motion = isSpiningNow ? motionCoef : 1f;
                speed = characterSpeedSlider.value * motion;

                var position = rb2dTransform.position;
                Vector2 startPos = position;
                Vector3 endPos = new Vector3(
                    startPos.x + System.Math.Sign(Mathf.RoundToInt(vector_direction.x)) * gridSize,
                    startPos.y + System.Math.Sign(Mathf.RoundToInt(vector_direction.y)) * gridSize);

                Vector3Int currentCell = groundTilemap.WorldToCell(position);
                Vector3 centerCell = groundTilemap.GetCellCenterWorld(currentCell);

                Vector3Int targetCellPos = wallTilemap.WorldToCell(endPos);
                Vector3 targetCenterCell = groundTilemap.GetCellCenterWorld(targetCellPos);

                Vector3 nPos;

                var isObstacle = wallTilemap.HasTile(targetCellPos);
                if (!isObstacle) {
                    nPos = Vector3.MoveTowards(rb2dTransform.position, targetCenterCell, speed * Time.deltaTime);
                    rb2d.MovePosition(nPos);
                }
                else {
                    nPos = Vector3.MoveTowards(rb2dTransform.position, centerCell, speed * Time.deltaTime);
                    rb2d.MovePosition(nPos);
                    if (Vector3.Distance(rb2dTransform.position, centerCell) < 0.001f) {
                        isMoving = false;
                        break;
                    }
                }

                yield return new WaitForFixedUpdate();
            }

            yield return null;
        }
    }

    private void DrawRay(Vector2 startP, Vector2 endP, bool obstracle) {
        Color c = obstracle ? Color.red : Color.green;
        Debug.DrawLine(startP, endP, c, 100);
    }

    public override IEnumerator TurnCharacter() {
        while (true) {
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                Vector2 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                if (pos.x > 0.5) {
                    TurnTo(Direction.Right);
                }
                else if (pos.x < 0.5) {
                    TurnTo(Direction.Left);
                }
            }

            yield return null;
        }
    }

    public void TurnTo(Direction direction) {
        int sign = direction == Direction.Right ? -1 : 1;

        if (isSpiningNow) {
            StopCoroutine("Turn");
            float currentAngel = rb2dTransform.rotation.z;
            ////bug 
            float divisionRightAngle = Mathf.Abs(currentAngel % 90) * sign;
            float divisionLeftAngle = 90 - Mathf.Abs(currentAngel % 90) * sign;
            float angle = direction == Direction.Right ? divisionRightAngle : divisionLeftAngle;

//            // fix 
//            float divisionRightAngle = Mathf.Abs(currentAngel % 90) * -1;
//            float divisionLeftAngle = 90 - Mathf.Abs(currentAngel % 90);
//            float angle = direction == Direction.Right ? divisionRightAngle : divisionLeftAngle;

            StartCoroutine(Turn(angle));
        }
        else {
            vector_direction = Quaternion.Euler(0, 0, 90 * sign) * vector_direction;
            StartCoroutine(Turn(sign * 90));
        }
    }

    private void StartMoving() {
        isMoving = true;
    }

    public IEnumerator Turn(float angle) {
        isSpiningNow = true;
        float timeCount = 0.0f;
        Quaternion endRot = rb2dTransform.rotation * Quaternion.Euler(0, 0, angle);
        Quaternion start_rb2Rot = rb2dTransform.rotation;

        while (timeCount < 1f) {
            timeCountForTurn = timeCountSLider.value;
            if (!isMoving && timeCount > timeCountForTurn) StartMoving();
            float tmp_sm_step = Mathf.SmoothStep(0, 1f, timeCount);
            float smooth_time = Mathf.SmoothStep(0, 1f, tmp_sm_step);
            rb2dTransform.rotation = Quaternion.Slerp(start_rb2Rot, endRot, smooth_time);
            speedAngle = rotateSpeedSlider.value;
            timeCount += Time.deltaTime * speedAngle;
            yield return new WaitForFixedUpdate();
        }

        rb2dTransform.rotation = endRot;
        isSpiningNow = false;
        yield return null;
    }

//    public void OnTriggerEnter2D(Collider2D other) {
//        if (other.CompareTag("EnemySpirit")) {
//            StopAllCoroutines();
//            GameController.instance.Restart();
//        }
//
//        if (other.CompareTag("Exit")) {
//            StopAllCoroutines();
//            GameController.instance.LoadMenu();
//        }
//    }
}

public enum Direction {
    Right,
    Left,
    Bot,
    Top
}