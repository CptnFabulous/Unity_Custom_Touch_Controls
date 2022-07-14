using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AdvancedTouchInput : MonoBehaviour
{
    public MultiTouch inputHandler;
    /// <summary>
    /// How many fingers need to be held down to perform the required movement?
    /// </summary>
    protected abstract int requiredNumberOfTouches { get; }
    /// <summary>
    /// Have all the necessary inputs changed to register that the player is genuinely making a movement?
    /// </summary>
    protected abstract bool inputsHaveChanged { get; }
    
    bool previouslyActive = false;

    private void Awake() => inputHandler.onValuesChanged.AddListener((_) => OnValuesChanged());
    void OnValuesChanged()
    {
        bool active = inputHandler.contacts == requiredNumberOfTouches;

        // Checks both if 'active' on the previous frame as well as the current one
        // This ensures old positions are reset upon pressing the correct number of fingers
        // So weird deltas aren't recorded from when the player previously lifted their finger
        if (active && !previouslyActive)
        {
            ResetOldInputs();
        }
        if (active && previouslyActive && inputsHaveChanged)
        {
            ProcessInputs();
            ResetOldInputs();
        }

        previouslyActive = active;
    }

    /// <summary>
    /// Act on new inputs once they're confirmed to be new updates and leftover data has been scrubbed
    /// </summary>
    protected abstract void ProcessInputs();
    /// <summary>
    /// Overwrite old inputs to match the current ones, so differences can be accurately checked next time
    /// </summary>
    protected abstract void ResetOldInputs();

    #region Static functions for converting values
    public static float screenScale => Mathf.Min(Screen.width, Screen.height);
    public static Vector3 ScreenToWorldPoint(Camera viewingCamera, Vector2 screenPosition, float distance)
    {
        Vector3 value = new Vector3(screenPosition.x, screenPosition.y, distance);
        return viewingCamera.ScreenToWorldPoint(value);
    }
    #endregion
}
