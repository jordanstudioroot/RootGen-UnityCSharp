using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using RootExtensions;

public class HexGridCamera : MonoBehaviour
{  
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public
    public float stickMinZoom = -250;
    public float stickMaxZoom = -45;
    public float swivelMinZoom = 90;
    public float swivelMaxZoom = 45;
    public float moveSpeedMinZoom = 400;
    public float moveSpeedMaxZoom = 100;
    public float rotationSpeed = 180;

// ~~ private
    private HexGrid _grid;
    private Transform _swivel;
    private Transform _stick;
    private float _zoom = 1f;
    private float _rotationAngle;
    private IEnumerator _steadyPan;

// CONSTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DESTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DELEGATES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// EVENTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// ENUMS

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INTERFACES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// PROPERTIES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public
    public Transform Swivel {
        set {
            _swivel = value;
        }
    }

    public Transform Stick {
        set {
            _stick = value;
        }
    }

    public HexGrid TargetGrid {
        set {
            _grid = value;
        }
    }

    public bool SuspendInput {
        get; set;
    }

// ~~ private

// INDEXERS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// METHODS ~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public
    public static HexGridCamera GetCamera(HexGrid grid) {
        GameObject resultObj = new GameObject("HexGridCamera");
        HexGridCamera resultMono = resultObj.AddComponent<HexGridCamera>();
        
        GameObject swivelObj = new GameObject("Swivel");
        swivelObj.SetParent(resultObj, false);
        swivelObj.transform.localRotation = Quaternion.Euler(45, 0 ,0);

        resultMono.Swivel = swivelObj.transform;
        
        GameObject stickObj = new GameObject("Stick");
        stickObj.SetParent(swivelObj, false);
        stickObj.transform.localPosition = new Vector3(
            0, 0, -45
        );

        resultMono.Stick = stickObj.transform;
        
        GameObject cameraObj = new GameObject("Camera");
        cameraObj.SetParent(stickObj, false);
        cameraObj.tag = "MainCamera";
        cameraObj.transform.localRotation = Quaternion.Euler(5, 0, 0);

        Camera cameraMono = cameraObj.AddComponent<Camera>();
        cameraObj.AddComponent<PhysicsRaycaster>();
        cameraMono.nearClipPlane = 0.3f;
        cameraMono.farClipPlane = 1000f;
        cameraMono.depth = -1f;

        resultMono.transform.SetParent(grid.transform, false);
        resultMono.TargetGrid = grid;

        if (grid.Center2D) {
            resultMono.SetPosition(
                grid,
                grid.Center2D.transform.position.x,
                grid.Center2D.transform.position.z
            );
        }

        return resultMono;
    }

    public void ValidatePosition(HexGrid grid) {
        AdjustPosition(grid, 0, 0);
    }

    public void StartSteadyPan(Vector3 direction, float seconds, float speed) {
        StopSteadyPan();
        _steadyPan =
            SteadyPanCoroutine(
                _grid,
                direction,
                seconds,speed
            );

        StartCoroutine(_steadyPan);
    }

    public void StartEndlessSteadyPan(Vector3 direction, float speed) {
        StopSteadyPan();
        _steadyPan = SteadyPanCoroutine(_grid, direction, -1, speed);
        StartCoroutine(_steadyPan);
    }
    
    public void StopSteadyPan() {
        if (_steadyPan != null) {
            StopCoroutine(_steadyPan);
            _steadyPan = null;
            SuspendInput = false;
        }
    }

    public void SetYRotation(float angle) {
        _rotationAngle = angle;

        if (_rotationAngle < 0f) {
            _rotationAngle += 360f;
        }
        else if (_rotationAngle >= 360f) {
            _rotationAngle -= 360f;
        }

        transform.localRotation = Quaternion.Euler(0f, _rotationAngle, 0f);
    }

// ~~ private
    private void Awake() { }

    private void Update() {
        if (_grid)
            ValidatePosition(_grid);
        else
            return;           

        if (!SuspendInput) {
            ProcessInput();
        }
        
    }

    private void ProcessInput() {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");

        if (zoomDelta != 0f) {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");

        if (rotationDelta != 0f) {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");

        if (xDelta != 0f || zDelta != 0f) {
            AdjustPosition(_grid, xDelta, zDelta);
        }
    }

    private void AdjustZoom(float delta) {
        _zoom = Mathf.Clamp01(_zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, _zoom);
        _stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, _zoom);
        _swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    private void AdjustPosition(HexGrid grid, float xDelta, float zDelta) {
/* Multiply direction by rotation to point it in the direction of the rotation
* and keep the users movement in line with the camera.
*/
        Vector3 direction = 
            transform.localRotation *
            new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance =
            Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, _zoom) * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = 
            grid.Wrapping ? WrapPosition(grid, position) : ClampPosition(grid, position);
    }

    public void SetPosition(HexGrid grid, float posX, float posZ) {
        Vector3 position = new Vector3(
            posX,
            this.transform.localPosition.y,
            posZ
        );

        this.transform.localPosition =
            grid.Wrapping ? WrapPosition(grid, position) : ClampPosition(grid, position);
    }

    private Vector3 ClampPosition (HexGrid grid, Vector3 position) {
        float xMax =
            (grid.CellCountX - 0.5f) * HexMetrics.innerDiameter;

        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax =
            (grid.CellCountZ - 1) * (1.5f * HexMetrics.outerRadius);

        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }

    private Vector3 WrapPosition(HexGrid grid, Vector3 position) {
        float width = grid.CellCountX * HexMetrics.innerDiameter;

        while (position.x < 0f) {
            position.x += width;
        }

        while (position.x > width) {
            position.x -= width;
        }

        float zMax =
            (grid.CellCountZ - 1) * (1.5f * HexMetrics.outerRadius);

        position.z = Mathf.Clamp(position.z, 0f, zMax);

        grid.CenterMap(position.x);
        return position;
    }

    private void AdjustRotation(float delta) {
        _rotationAngle += delta * rotationSpeed * Time.deltaTime;

        if (_rotationAngle < 0f) {
            _rotationAngle += 360f;
        }
        else if (_rotationAngle >= 360f) {
            _rotationAngle -= 360f;
        }

        transform.localRotation = Quaternion.Euler(0f, _rotationAngle, 0f);
    }

    private IEnumerator SteadyPanCoroutine(
        HexGrid grid,
        Vector3 direction,
        float durationInSeconds,
        float speed
    ) {
        SuspendInput = true;

        if (durationInSeconds == -1) {
            durationInSeconds = Mathf.Infinity;
        }

        float elapsedSeconds = 0;

        while (elapsedSeconds < durationInSeconds) {
            Vector3 newPos = Vector3.Lerp(
                transform.position,
                transform.position + direction,
                Time.deltaTime * speed
            );

            SetPosition(grid, newPos.x, newPos.z);

            elapsedSeconds += Time.deltaTime;
            durationInSeconds = Mathf.Infinity;
            yield return new WaitForEndOfFrame();
        }

        SuspendInput = false;
    }

// STRUCTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// CLASSES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
}
