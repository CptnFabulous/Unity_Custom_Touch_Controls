using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileDebug : MonoBehaviour
{
    public static MobileDebug Instance
    {
        get
        {
            if (md == null)
            {
                md = FindObjectOfType<MobileDebug>();
            }
            return md;
        }
    }
    static MobileDebug md;

    public Camera viewCamera;
    public Text console;
    public LineRenderer debugLine;

    private void Awake()
    {
        debugLine.useWorldSpace = true;
    }

    public static void Log(string message) => Instance.console.text = "\n" + message;

    public void DrawScreenLine(Vector2[] positions)
    {
        debugLine.positionCount = positions.Length;
        for (int i = 0; i < positions.Length; i++)
        {
            debugLine.SetPosition(i, AdvancedTouchInput.ScreenToWorldPoint(viewCamera, positions[i], viewCamera.nearClipPlane));
        }
    }
    public void DrawScreenLine(Vector2 start, Vector2 end)
    {
        debugLine.positionCount = 2;
        Vector3 worldStart = AdvancedTouchInput.ScreenToWorldPoint(viewCamera, start, viewCamera.nearClipPlane + float.Epsilon);
        Vector3 worldEnd = AdvancedTouchInput.ScreenToWorldPoint(viewCamera, end, viewCamera.nearClipPlane + float.Epsilon);
        debugLine.SetPosition(0, worldStart);
        debugLine.SetPosition(1, worldEnd);
    }
}
