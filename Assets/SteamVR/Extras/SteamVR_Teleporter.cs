using System;
using UnityEngine;

public class SteamVR_Teleporter : MonoBehaviour {
  public TeleportType TeleportType = TeleportType.TeleportTypeUseCollider;
  public GameObject TargetWidget;
  public Color LineColor = Color.red;
  public float LineWidth = 2f;

  private GameObject m_TargetWidgetInstance;
  private LineRenderer m_Line;
  private bool m_Clicking;

  private static Transform Reference {
    get {
      var top = SteamVR_Render.Top();
      return top != null ? top.origin : null;
    }
  }

  private void Start() {
    var trackedController = GetOrAddComponent<SteamVR_TrackedController>();
    trackedController.PadUnclicked += OnPadUnclicked;
    trackedController.PadClicked += OnPadClicked;

    m_Line = GetOrAddComponent<LineRenderer>();

    if (TeleportType == TeleportType.TeleportTypeUseTerrain) {
      // Start the player at the level of the terrain
      var t = Reference;
      if (t != null) t.position = new Vector3(t.position.x, Terrain.activeTerrain.SampleHeight(t.position), t.position.z);
    }

    if (TargetWidget) {
      m_TargetWidgetInstance = Instantiate(TargetWidget);
      m_TargetWidgetInstance.transform.SetParent(transform, true);
      m_TargetWidgetInstance.gameObject.SetActive(false);
    }
  }

  private void LateUpdate() {
    if (!m_Clicking) {
      HideWidget();
      return;
    }

    Vector3 pos;

    if (GetTargetPosition(out pos)) { ShowWidget(pos); }
    else { HideWidget(); }
  }

  private void HideWidget() { SetWidget(false, Vector3.zero); }
  private void ShowWidget(Vector3 pos) { SetWidget(true, pos); }

  private void SetWidget(bool active, Vector3 targetPosition) {
    if (m_TargetWidgetInstance) {
      m_TargetWidgetInstance.SetActive(active);
      if (active) {
        m_TargetWidgetInstance.transform.position = targetPosition;
      }
    }

    m_Line.enabled = active;

    if (!active) return;

    m_Line.SetPositions(new []{targetPosition, transform.position});
    m_Line.SetColors(LineColor, LineColor);
    m_Line.SetWidth(LineWidth, LineWidth);
  }

  private T GetOrAddComponent<T>() where T : Component {
    var component = GetComponent<T>();
    if (!component) component = gameObject.AddComponent<T>();
    return component;
  }

  private void OnPadClicked(object sender, ClickedEventArgs e) { m_Clicking = true; }

  private void OnPadUnclicked(object sender, ClickedEventArgs e) {
    m_Clicking = false;
    Vector3 targetPosition;
    if (GetTargetPosition(out targetPosition)) { Reference.position = targetPosition; }
  }

  private bool GetTargetPosition(out Vector3 targetPosition) {
    var t = Reference;
    if (!t) {
      targetPosition = Vector3.zero;
      return false;
    }

    var ray = new Ray(transform.position, transform.forward);
    bool hasGroundTarget;
    float dist;

    switch (TeleportType) {
      case TeleportType.TeleportTypeUseTerrain:
        RaycastHit terrainHitInfo;
        var tc = Terrain.activeTerrain.GetComponent<TerrainCollider>();
        hasGroundTarget = tc.Raycast(ray, out terrainHitInfo, 1000f);
        dist = terrainHitInfo.distance;
        break;
      case TeleportType.TeleportTypeUseCollider:
        RaycastHit colliderHitInfo;
        hasGroundTarget = Physics.Raycast(ray, out colliderHitInfo);
        dist = colliderHitInfo.distance;
        break;
      case TeleportType.TeleportTypeUseZeroY:
        var refY = t.position.y;
        var plane = new Plane(Vector3.up, -refY);
        hasGroundTarget = plane.Raycast(ray, out dist);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    if (!hasGroundTarget) {
      targetPosition = Vector3.zero;
      return false;
    }

    var headPosOnGround = new Vector3(SteamVR_Render.Top().head.localPosition.x, 0.0f, SteamVR_Render.Top().head.localPosition.z);
    targetPosition = ray.origin + ray.direction*dist - new Vector3(t.GetChild(0).localPosition.x, 0f, t.GetChild(0).localPosition.z) - headPosOnGround;
    return true;
  }
}

public enum TeleportType {
  TeleportTypeUseTerrain,
  TeleportTypeUseCollider,
  TeleportTypeUseZeroY
}