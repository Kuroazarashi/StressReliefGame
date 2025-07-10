using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // Cinemachineを使用するために必要
using System.Collections;

public class PlayerPunchController : MonoBehaviour
{
    [SerializeField] private float punchForce = 1000f;
    [SerializeField] private float upwardForceMultiplier = 0.5f;

    private PlayerInputActions playerInputActions;

    // ★★★ここを修正！★★★
    [SerializeField] private Unity.Cinemachine.CinemachineCamera virtualCameraComponent;

    // スローモーション関連の変数
    [SerializeField] private float slowMotionDuration = 3.0f;
    [SerializeField] private float slowMotionTimeScale = 0.1f;

    private Animator animator;

    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on Player or its children!");
        }
    }

    void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Gameplay.Punch.performed += OnPunchPerformed;
    }

    void OnDisable()
    {
        playerInputActions.Gameplay.Punch.performed -= OnPunchPerformed;
        playerInputActions.Disable();
    }

    void OnPunchPerformed(InputAction.CallbackContext context)
    {
        if (animator != null)
        {
            animator.SetTrigger("PunchTrigger");
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Rigidbody hitRigidbody = hit.collider.GetComponent<Rigidbody>();

            if (hitRigidbody != null)
            {
                Vector3 punchDirection = (hit.point - transform.position).normalized;
                punchDirection.y += upwardForceMultiplier;
                punchDirection = punchDirection.normalized;

                hitRigidbody.AddForce(punchDirection * punchForce, ForceMode.Impulse);

                StartCoroutine(DoSlowMotion());

                // ★★★ここも修正します★★★
                if (virtualCameraComponent != null)
                {
                    virtualCameraComponent.Follow = hitRigidbody.transform;
                    virtualCameraComponent.LookAt = hitRigidbody.transform;
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(10);
                }
            }
        }
    }

    IEnumerator DoSlowMotion()
    {
        float originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowMotionDuration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // ★★★ここも修正します★★★
        if (virtualCameraComponent != null)
        {
            virtualCameraComponent.Follow = transform; // Playerに戻す
            virtualCameraComponent.LookAt = transform;
        }
    }
}