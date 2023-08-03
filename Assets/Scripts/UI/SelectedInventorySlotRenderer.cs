using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public class SelectedInventorySlotRenderer : MonoBehaviour
    {
        [SerializeField] private TMP_Text textMesh;
        [SerializeField] private InputManager inputManager;

        private int currentInventorySlot;

        private void Start()
        {
            inputManager.PlayerInventorySlot.performed += OnInventorySlotSelected;
        }

        private void OnDisable()
        {
            inputManager.PlayerInventorySlot.performed -= OnInventorySlotSelected;
        }

        private void OnInventorySlotSelected(InputAction.CallbackContext context)
        {
            currentInventorySlot = int.Parse(context.control.name);
            textMesh.text = currentInventorySlot.ToString();
        }
    }
}