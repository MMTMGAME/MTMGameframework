using System;
using UnityEngine;
using UnityEngine.InputSystem;

//手势检测
public class SwipeDetection : MonoBehaviour
{
    private Vector2 touchStartPosition;
    private Vector2 touchEndPosition;
    private bool touchInProgress = false;

    public Action<InputAction.CallbackContext> swipeUp;
    public Action<InputAction.CallbackContext> swipeDown;
    public Action<InputAction.CallbackContext> swipeLeft;
    public Action<InputAction.CallbackContext> swipeRight;
    private void Update()
    {
        // Check if the touchscreen is available
        if (Touchscreen.current == null) return;
        
        if (Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (!touchInProgress)
            {
                touchInProgress = true;
                touchStartPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
        }
        else
        {
            if (touchInProgress)
            {
                touchInProgress = false;
                touchEndPosition = Touchscreen.current.primaryTouch.position.ReadValue();

                Vector2 direction = touchEndPosition - touchStartPosition;
                if (direction.magnitude > 100) // Threshold for swipe detection, can be adjusted
                {
                    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                    {
                        // Horizontal swipe
                        if (direction.x > 0)
                        {
                            Debug.Log("Swipe Right");
                            // Handle swipe right
                            swipeRight?.Invoke(default);
                        }
                        else
                        {
                            Debug.Log("Swipe Left");
                            // Handle swipe left
                            swipeLeft?.Invoke(default);
                        }
                    }
                    else
                    {
                        // Vertical swipe
                        if (direction.y > 0)
                        {
                            Debug.Log("Swipe Up");
                            // Handle swipe up
                            swipeUp?.Invoke(default);
                        }
                        else
                        {
                            Debug.Log("Swipe Down");
                            // Handle swipe down
                            swipeDown?.Invoke(default);
                        }
                    }
                }
            }
        }
    }
}
