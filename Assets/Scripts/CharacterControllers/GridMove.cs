using UnityEngine;
using System.Collections;

class GridMove : MonoBehaviour {
    private PlayerController player;

    private float moveSpeed = 3f;
    private float gridSize = 1f;

    private enum Orientation {
        Horizontal,
        Vertical
    }

    private Orientation gridOrientation = Orientation.Vertical;
    private bool allowDiagonals = false;
    private bool correctDiagonalSpeed = true;
    [SerializeField] private Vector2 input;
    [SerializeField] private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float t;
    private float factor;

    void Awake() {
        player = GetComponent<PlayerController>();
    }

    public void Stop() {
        StopAllCoroutines();
        isMoving = false;
    }

    public void Move(Vector2 direction, float speed) {

        if (!isMoving) {
            input = direction;
            if (!allowDiagonals) {
                if (Mathf.Abs(input.x) > Mathf.Abs(input.y)) {
                    input.y = 0;
                } else {
                    input.x = 0;
                }
            }


            if (input != Vector2.zero) {
                moveSpeed = speed;
                StartCoroutine(CellMove(player.rb2dTransform));
            }
        }
    }

    public IEnumerator CellMove(Transform _transform) {
         
        isMoving = true;
        startPosition = _transform.position;
        t = 0;

        if (gridOrientation == Orientation.Horizontal) {
            endPosition = new Vector3(
                startPosition.x + System.Math.Sign(input.x) * gridSize,
                startPosition.y,
                startPosition.z + System.Math.Sign(input.y) * gridSize);
        } else {
            endPosition = new Vector3(
                startPosition.x + System.Math.Sign(input.x) * gridSize,
                startPosition.y + System.Math.Sign(input.y) * gridSize,
                startPosition.z);
        }

        if (allowDiagonals && correctDiagonalSpeed && input.x != 0 && input.y != 0) {
            factor = 0.7071f;
        } else {
            factor = 1f;
        }

        while (t < 1f) {
            t += Time.deltaTime * (moveSpeed / gridSize) * factor;
           Vector3 nPos = Vector3.Lerp(startPosition, endPosition, t);
           player.rb2d.MovePosition(nPos);
           yield return null;
        }

        isMoving = false;
        yield return null;
    }
}