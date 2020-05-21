using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using RootExtensions;
using RootLogging;
using RootUtils.Validation;
using RootUtils.UnityLifecycle;

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
    private HexMap _grid;
    private Transform _swivel;
    private Transform _stick;
    private float _zoom = 1f;
    private float _rotationAngle;
    private IEnumerator _steadyPan;
// TODO: Temporariliy storing the outer radius
//       of a given hex in a local variable,
//       using reassignment to update it as a
//       reference value for camera movement
//       and constraining camera position.
//       Need alternative approach to this.
    private static float _hexOuterRadius;

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

    public HexMap TargetGrid {
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
    public static void AttachCamera(HexMap grid, float hexOuterRadius) {
        HexGridCamera resultMono;
        _hexOuterRadius = hexOuterRadius;

        if (InstanceValidation.InstanceExists<HexGridCamera>()) {
            resultMono = InstanceValidation.GetFirstInstance<HexGridCamera>();
            resultMono.transform.SetParent(grid.transform, false);
            resultMono.TargetGrid = grid;
            resultMono.enabled = true;
            if (InstanceValidation.SingleInstanceExists<HexGridCamera>()) {
                RootLog.Log(
                    "Single instance of camera already exists. " + 
                        "Attaching instance to grid.",
                    Severity.Information,
                    "HexGridCamera"
                );
            }
            else {
                RootLog.Log(
                    "Multiple instances of camera already exist. " + 
                        "Attaching first instance to grid.",
                    Severity.Information,
                    "HexGridCamera"
                );
            }
        }
        else {
            GameObject resultObj = new GameObject("HexGridCamera");
            resultMono = resultObj.AddComponent<HexGridCamera>();
            resultMono.transform.SetParent(grid.transform, false);
            resultMono.TargetGrid = grid;
            resultMono.enabled = true;
            
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

            if (grid.GridCenter) {
                resultMono.SetPosition(
                    grid,
                    grid.GridCenter.transform.position.x,
                    grid.GridCenter.transform.position.z,
                    hexOuterRadius
                );
            }
        }
    }

    public void ValidatePosition(HexMap grid, float hexOuterRadius) {
        AdjustPosition(grid, 0, 0, hexOuterRadius);
    }

    public void StartSteadyPan(
        Vector3 direction,
        float seconds,
        float speed,
        float hexOuterRadius
    ) {
        StopSteadyPan();
        _steadyPan =
            SteadyPanCoroutine(
                _grid,
                direction,
                seconds,speed,
                hexOuterRadius
            );

        StartCoroutine(_steadyPan);
    }

    public void StartEndlessSteadyPan(
        Vector3 direction,
        float speed,
        float hexOuterRadius
    ) {
        StopSteadyPan();
        _steadyPan = SteadyPanCoroutine(
            _grid,
            direction,
            -1,
            speed,
            hexOuterRadius
        );
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
            ValidatePosition(_grid, _hexOuterRadius);
        else
            return;           

        if (!SuspendInput) {
            ProcessInput(_hexOuterRadius);
        }
        
    }

    private void ProcessInput(float hexOuterRadius) {
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
            AdjustPosition(
                _grid,
                xDelta,
                zDelta,
                hexOuterRadius
            );
        }
    }

    private void AdjustZoom(float delta) {
        _zoom = Mathf.Clamp01(_zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, _zoom);
        _stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, _zoom);
        _swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    private void AdjustPosition(
        HexMap grid,
        float xDelta,
        float zDelta,
        float hexOuterRadius
    ) {
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
            grid.IsWrapping ?
                WrapPosition(grid, position, hexOuterRadius) :
                ClampPosition(grid, position, hexOuterRadius);
    }

    public void SetPosition(
        HexMap grid,
        float posX,
        float posZ,
        float hexOuterRadius
    ) {
        Vector3 position = new Vector3(
            posX,
            this.transform.localPosition.y,
            posZ
        );

        this.transform.localPosition =
            grid.IsWrapping ?
                WrapPosition(grid, position, hexOuterRadius) :
                ClampPosition(grid, position, hexOuterRadius);
    }

    private Vector3 ClampPosition (
        HexMap grid,
        Vector3 position,
        float hexOuterRadius
    ) {
// Get inner diameter of a given hex.
        float innerDiameter = 
            HexagonPoint.OuterToInnerRadius(hexOuterRadius) * 2f;

        float xMax =
            (grid.HexOffsetColumns - 0.5f) * innerDiameter;

        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax =
            (grid.HexOffsetRows - 1) * (1.5f * hexOuterRadius);

        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }

    private Vector3 WrapPosition(
        HexMap hexMap,
        Vector3 position,
        float hexOuterRadius
    ) {
        float innerDiameter = 
            HexagonPoint.InnerDiameterFromOuterRadius(hexOuterRadius);

        float thresholdWidth = hexMap.HexOffsetColumns * innerDiameter;

        while (position.x < 0f) {
            position.x += thresholdWidth;
        }

        while (position.x > thresholdWidth) {
            position.x -= thresholdWidth;
        }

        float thresholdHeight =
            (hexMap.HexOffsetRows) * (1.5f * hexOuterRadius);

        position.z = Mathf.Clamp(position.z, 0f, thresholdHeight);

        hexMap.CenterMap(position.x, hexOuterRadius);
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
        HexMap grid,
        Vector3 direction,
        float durationInSeconds,
        float speed,
        float hexOuterRadius
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

            SetPosition(
                grid,
                newPos.x,
                newPos.z,
                hexOuterRadius
            );

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
