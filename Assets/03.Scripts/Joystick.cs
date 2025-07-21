using UnityEngine;
using UnityEngine.EventSystems; // EventSystemsを使うために必要

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform joystickBackground; // ジョイスティックの外枠（親）のRectTransform
    [SerializeField] private RectTransform joystickThumb;      // ジョイスティックのつまみ（子）のRectTransform
    [SerializeField] private float moveRange = 100f;          // つまみが動く範囲（半径）

    public Vector2 InputDirection { get; private set; } // ジョイスティックの入力方向（正規化されたベクトル）

    private Vector2 joystickCenter; // ジョイスティックの外枠の中心座標
    private Vector2 touchStartPos;  // タッチ開始時の位置

    void Start()
    {
        // 開始時に外枠の中心座標を計算
        joystickCenter = joystickBackground.position;
        // つまみを中央にリセット
        joystickThumb.position = joystickCenter;
        InputDirection = Vector2.zero; // 初期化
    }

    // ポインター（指/マウス）が押された時
    public void OnPointerDown(PointerEventData eventData)
    {
        touchStartPos = eventData.position; // タッチ開始位置を記録
        joystickThumb.position = touchStartPos; // つまみをタッチ位置に移動
        UpdateJoystick(eventData); // ジョイスティックの状態を更新
    }

    // ポインターがドラッグされている時
    public void OnDrag(PointerEventData eventData)
    {
        UpdateJoystick(eventData); // ジョイスティックの状態を更新
    }

    // ポインターが離された時
    public void OnPointerUp(PointerEventData eventData)
    {
        // つまみを中央に戻す
        joystickThumb.position = joystickCenter;
        InputDirection = Vector2.zero; // 入力方向をリセット
    }

    private void UpdateJoystick(PointerEventData eventData)
    {
        // 現在のポインター位置とタッチ開始位置の差を計算
        Vector2 direction = eventData.position - touchStartPos;

        // つまみが動く範囲（半径）を超えないように制限
        float distance = Vector2.ClampMagnitude(direction, moveRange).magnitude;

        // つまみを移動
        joystickThumb.position = joystickCenter + direction.normalized * distance;

        // 入力方向を正規化して保存
        InputDirection = direction.normalized * (distance / moveRange);
    }
}