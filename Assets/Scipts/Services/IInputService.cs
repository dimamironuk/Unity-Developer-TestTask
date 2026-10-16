using UnityEngine;

public interface IInputService
{
    bool WasTapped { get; }
    bool IsPressed { get; }
    Vector2 Delta { get; }
}