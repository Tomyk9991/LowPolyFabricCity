using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rayLength = 0.2f;
    [SerializeField] private LayerMask groundedLayerMask;

    private GameObject hitObject;
    private CharacterController controller;
    private Vector3 playerVelocity;

    [SerializeField] private bool groundedPlayer;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    private Vector3 halfPlayerHeight;

    private void Start()
    {
        this.controller = GetComponent<CharacterController>();
        
        this.halfPlayerHeight = new Vector3(0, controller.height / 2.0f, 0.0f);
    }

    private void Update()
    {
        groundedPlayer = Physics.Raycast(new Ray(transform.position - halfPlayerHeight, Vector3.down), rayLength, groundedLayerMask);
        
        if (groundedPlayer && playerVelocity.y < 0.0f)
            playerVelocity.y = 0.1f;

        Vector2 movement = inputManager.PlayerMovement;
        Vector3 move = new Vector3(movement.x, 0f, movement.y);

        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0.0f;
        
        controller.Move(move * (Time.deltaTime * playerSpeed));

        if (inputManager.PlayerJumpedThisFrame && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
