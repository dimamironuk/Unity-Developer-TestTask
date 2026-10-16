using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputService : IInputService
{
    public bool WasTapped => Pointer.current != null && Pointer.current.press.wasPressedThisFrame && !IsOverUi();
    public bool IsPressed => Pointer.current != null && Pointer.current.press.isPressed;
    public Vector2 Delta => IsPressed ? Pointer.current.delta.ReadValue() : Vector2.zero;

    private static bool IsOverUi() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
}