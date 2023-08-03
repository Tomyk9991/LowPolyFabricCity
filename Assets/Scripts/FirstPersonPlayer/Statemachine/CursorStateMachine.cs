using UnityEngine;

namespace FirstPersonPlayer.Statemachine
{
    public class  CursorStateMachine : Statemachine
    {
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                
                if (visible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        private bool visible;

        public CursorStateMachine()
        {
            Visible = false;
        }
        
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Visible = !Visible;
            }
        }

        public override void OnDisable()
        {
            Visible = true;
        }
    }
}