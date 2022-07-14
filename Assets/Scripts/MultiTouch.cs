using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class MultiTouch : MonoBehaviour
{
    public UnityEvent<MultiTouch> onValuesChanged;
    public UnityEngine.UI.Text debugWindow;
    
    int touchCount = 10;

    #region Properties
    public InputAction[] contactActions { get; private set; }
    public InputAction[] positionActions { get; private set; }
    public int contacts { get; private set; }
    public Vector2[] positions { get; private set; }
    #endregion

    private void Awake() => SetupActions();

    /// <summary>
    /// Sets up the InputActions for positions and contacts
    /// </summary>
    void SetupActions()
    {
        positions = new Vector2[touchCount];
        contactActions = new InputAction[touchCount];
        positionActions = new InputAction[touchCount];
        for (int i = 0; i < touchCount; i++)
        {
            // Set up contact action
            contactActions[i] = new InputAction(
                name: "Finger Contact #" + (i + 1),
                type: InputActionType.Button,
                binding: "<Touchscreen>/touch" + i + "/press",
                // 0 registers a press, 1 registers a release, 2 checks for both.
                // 1 is important because if 2, event will run before position is properly assigned.
                // Contacts are updated anyway whenever the positions are
                interactions: "Press(behavior=1)",
                expectedControlType: "Touch"
                );

            // Set up position action
            positionActions[i] = new InputAction(
                name: "Finger Position #" + (i + 1),
                type: InputActionType.Value,
                binding: "<Touchscreen>/touch" + i + "/position",
                expectedControlType: "Touch"
                );

            // Add listeners to update positions and count when inputs change
            contactActions[i].performed += (_) => OnValuesChanged();
            positionActions[i].performed += (_) => OnValuesChanged();
        }
    }
    /// <summary>
    /// Runs when the contacts or positions change.
    /// </summary>
    void OnValuesChanged()
    {
        int touchNumber = 0;
        for (int i = 0; i < touchCount; i++)
        {
            if (contactActions[i].IsPressed())
            {
                positions[touchNumber] = positionActions[i].ReadValue<Vector2>();
                touchNumber++;
            }
        }
        contacts = touchNumber;
        onValuesChanged.Invoke(this);

        #region Debug!
        string message = "Frame: " + Time.frameCount + "\nTouch count: " + contacts;
        for (int i = 0; i < contacts; i++)
        {
            message += "\nPosition " + (i + 1) + ": " + positions[i];
        }
        Debug.Log(message);
        if (debugWindow != null) debugWindow.text = message;
        #endregion
    }

    #region Enable/disable InputActions to save processing power
    private void OnEnable() => SetInputActiveState(true);
    private void OnDisable() => SetInputActiveState(false);
    void SetInputActiveState(bool active)
    {
        for (int i = 0; i < touchCount; i++)
        {
            if (active)
            {
                contactActions[i].Enable();
                positionActions[i].Enable();
            }
            else
            {
                contactActions[i].Disable();
                positionActions[i].Disable();
            }
        }
    }
    #endregion
}
