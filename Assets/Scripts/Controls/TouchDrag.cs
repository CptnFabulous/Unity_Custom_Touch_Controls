using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class TouchDrag : ComplexTouch
{

    public UnityEvent<Vector2> onDrag;

    public Vector2 oldFingerPosition { get; private set; }
    public Vector2 newFingerPosition { get; private set; }

    bool previouslyActive;

    public void OnFingerPosition1(InputValue input)
    {
        newFingerPosition = input.Get<Vector2>();
        bool active = fingerContact1 && !fingerContact2; // Checks if only one finger is pressed to the screen

        // If active
        // If was also active last frame (ensures start position is recorded before inputs are made)
        // If position actually moves
        if (active && previouslyActive && newFingerPosition != oldFingerPosition)
        {
            Vector2 scaledPositionDifference = (newFingerPosition - oldFingerPosition) / screenScale;
            onDrag.Invoke(scaledPositionDifference);
        }

        oldFingerPosition = newFingerPosition;
        previouslyActive = active;
    }
    
}
