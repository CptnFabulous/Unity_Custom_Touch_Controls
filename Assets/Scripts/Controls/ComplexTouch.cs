using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ComplexTouch : MonoBehaviour
{


    public bool fingerContact1 { get; private set; }
    public bool fingerContact2 { get; private set; }
    public void OnFingerContact1(InputValue input) => fingerContact1 = input.isPressed;
    public void OnFingerContact2(InputValue input) => fingerContact2 = input.isPressed;

    public static float screenScale => Mathf.Min(Screen.width, Screen.height);
}
