using System;

public static class InputManager
{
    private static KeyboardState keyboardState, lastKeyboardState;
    private static MouseState mouseState, lastMouseState;
    private static float currentMouseWheelValue, previousMouseWheelValue;

    public const int CAMERA_MODE = 1;
    public const int BUILD_MODE = 2;
    public static int Mode = CAMERA_MODE;

    public static float ScrollValue;

    // Camera movement
    private static Vector2 _dragStart;
    public static Vector2 MouseDrag = Vector2.Zero;

    // Trigger to reset the camera to the default position and zoom
    public static bool CameraReset = false;

    // Trigger to place a building and exit build mode
    public static bool ConfirmBuilding = false;

    // Current mouse position, will be corrected for camera transformations by GameManager
    public static Vector2 MousePos = Vector2.Zero;

    private static void DetermineMode()
    {
        // Toggle build mode when 'B' is pressed
        if (keyboardState.IsKeyUp(Keys.B) && lastKeyboardState.IsKeyDown(Keys.B))
        {
            if (Mode == BUILD_MODE)
            {
                SwitchToCameraMode();
            }
            else
            {
                Mode = BUILD_MODE;
            }
        }
    }

    // Switch to camera mode and re-initialize variables
    private static void SwitchToCameraMode()
    {
        Mode = CAMERA_MODE;
        _dragStart = Vector2.Zero;
        MouseDrag = Vector2.Zero;
        ConfirmBuilding = false;
    }

    private static void ProcessCameraInputs()
    {
        // If 'R' was pressed, set the CameraReset state
        CameraReset = keyboardState.IsKeyUp(Keys.R) && lastKeyboardState.IsKeyDown(Keys.R);

        // Track click-and-drag
        if (mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton != ButtonState.Pressed)
        {
            _dragStart = new Vector2(mouseState.X, mouseState.Y);
        }
        else if (_dragStart != Vector2.Zero && lastMouseState.LeftButton == ButtonState.Pressed)
        {
            // MouseDrag smoothly, move camera in the opposite direction to drag
            Vector2 pos = new Vector2(mouseState.X, mouseState.Y);
            MouseDrag = -1 * (pos - _dragStart);
            _dragStart = pos;
        }
        else if (mouseState.LeftButton != ButtonState.Pressed)
        {
            _dragStart = Vector2.Zero;
            MouseDrag = Vector2.Zero;
        }
    }

    private static void ProcessBuildingInputs()
    {
        // Disable build mode on right click
        if (lastMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed)
        {
            SwitchToCameraMode();
        }
        // Confirm the building if left click is pressed
        else if (lastMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed)
        {
            ConfirmBuilding = true;
        }
        else if (ConfirmBuilding)
        {
            SwitchToCameraMode();
        }
        else
        {
            ConfirmBuilding = false;
        }
    }

    public static void Update()
    {
        lastKeyboardState = keyboardState;
        lastMouseState = mouseState;

        keyboardState = Keyboard.GetState();
        mouseState = Mouse.GetState();

        MousePos.X = mouseState.X;
        MousePos.Y = mouseState.Y;

        previousMouseWheelValue = currentMouseWheelValue;
        currentMouseWheelValue = mouseState.ScrollWheelValue;
        ScrollValue = currentMouseWheelValue - previousMouseWheelValue;

        DetermineMode();

        switch (Mode)
        {
            case BUILD_MODE : ProcessBuildingInputs(); break;
            case CAMERA_MODE: ProcessCameraInputs();   break;
            default: ProcessCameraInputs(); break;
        }
    }

    // Register a mode along with instructions along with events for enabling and disabling it
    // and what callbacks to trigger for specific events while in that mode?
}