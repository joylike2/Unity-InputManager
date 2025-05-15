using UnityEngine;
using LifeLogs.InputSystem;
using UnityEngine.InputSystem;

public class GamePlayViewInputSample : MonoBehaviour, IInputReceiver {
    public Rigidbody characterRigidbody;

    public float moveSpeed = 1f;
    
    private bool _isMoving;
    private Vector2 _moveInput;

    private void OnEnable() {
        InputManager.Instance.RegisterReceiver(this);
        InputManager.Instance.SwitchActionMap("Player");  //액션맵 이름을 실제 사용할 액션맵으로 변경하세요.
    }
    
    // private void OnDisable() {
    //     InputManager.Instance.UnregisterReceiver(this);
    // }
    
    private void Update() {
        if (!_isMoving) return;

        Vector3 moveDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
        Vector3 move = moveDirection * (moveSpeed * Time.fixedDeltaTime);
        Vector3 newPosition = characterRigidbody.position + move;
        characterRigidbody.MovePosition(newPosition);
    }
    
    //입력 액션 이벤트 처리 (IInputReceiver 구현)
    public void OnInputPerformed(string actionMap, string actionName, InputAction.CallbackContext context) {
        if (actionName == "Move") {
            _moveInput = context.ReadValue<Vector2>();
            _isMoving = _moveInput.sqrMagnitude > 0.01f;
        }
    }
}