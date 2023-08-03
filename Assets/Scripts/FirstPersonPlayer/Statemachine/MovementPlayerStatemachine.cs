using UnityEngine;

namespace FirstPersonPlayer.Statemachine
{
    public class MovementPlayerStatemachine : Statemachine
    {
        private readonly InputManager inputManager;
        private readonly Transform cameraTransform;
        private readonly float rayLength;
        private readonly LayerMask groundedLayerMask;
        private readonly CharacterController controller;
        private Vector3 playerVelocity;
        public bool GroundedPlayer { get; private set; }
        public float PlayerSpeed { get; set; }
        public float JumpHeight { get; set; }
        public float GravityValue { get; set; }
        private readonly Vector3 halfPlayerHeight;
        private readonly Transform transform;
        
        public MovementPlayerStatemachine(InputManager inputManager, Transform cameraTransform, float rayLength, LayerMask groundedLayerMask, CharacterController controller, Transform transform)
        {
            this.inputManager = inputManager;
            this.cameraTransform = cameraTransform;
            this.rayLength = rayLength;
            this.groundedLayerMask = groundedLayerMask;
            this.controller = controller;
            this.transform = transform;
            
            this.halfPlayerHeight = new Vector3(0, controller.height / 2.0f, 0.0f);
        }
        
        public override void OnUpdate()
        {
            GroundedPlayer = Physics.Raycast(new Ray(transform.position - halfPlayerHeight, Vector3.down), rayLength, groundedLayerMask);
        
            if (GroundedPlayer && playerVelocity.y < 0.0f)
                playerVelocity.y = 0.1f;

            Vector2 movement = inputManager.PlayerMovement;
            Vector3 move = new Vector3(movement.x, 0f, movement.y);

            move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
            move.y = 0.0f;
        
            controller.Move(move * (Time.deltaTime * PlayerSpeed));

            if (inputManager.PlayerJumpedThisFrame && GroundedPlayer)
            {
                playerVelocity.y += Mathf.Sqrt(JumpHeight * -3.0f * GravityValue);
            }

            playerVelocity.y += GravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }

        public override void OnDisable()
        {
            
        }
    }
}