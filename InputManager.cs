using System;
using Katabasis;

public static class InputManager
{
    private static KeyboardState keyboardState, lastKeyboardState;
    private static MouseState mouseState, lastMouseState;
    private static float currentMouseWheelValue, previousMouseWheelValue;

    public const int CAMERA_MODE = 1;
    public const int BUILD_MODE = 2;
    public const int TILE_MODE = 3;
    public static int Mode = CAMERA_MODE;

    public static float ScrollValue;

    public static bool Clicked;
    public static bool ClickConsumed;
    public static object ClickConsumer;
    public static bool ClickAndHold;
    public static bool MouseDown;

    // Pressing B no longer toggles build mode, it just toggles the build ui
    // clicking on a building type in the UI is what enables build mode
    public static bool BPressed;
    public static bool IPressed;
    public static bool XPressed;
    public static bool PlusPressed;
    public static bool MinusPressed;
    public static bool ShiftHeld;
    public static bool SavePressed;

    // Camera movement
    private static Vector2 _dragStart;
    public static Vector2 MouseDrag = Vector2.Zero;

    // Trigger to reset the camera to the default position and zoom
    public static bool CameraReset = false;

    public static bool Paused = false;

    // Trigger to place a building and exit build mode
    public static bool ConfirmBuilding = false;

    // Current mouse position, will be corrected for camera transformations by GameManager
    public static Vector2 MousePos = Vector2.Zero;
    public static Vector2 ScreenMousePos = Vector2.Zero;

    private static void DetermineMode()
    {
        if (keyboardState.IsKeyUp(Keys.T) && lastKeyboardState.IsKeyDown(Keys.T))
        {
            if (Mode == TILE_MODE)
            {
                SwitchToMode(CAMERA_MODE);
            }
            else
            {
                SwitchToMode(TILE_MODE);
            }
        }
    }

    // Switch to camera mode and re-initialize variables
    public static void SwitchToMode(int mode)
    {
        Mode = mode;
        _dragStart = Vector2.Zero;
        MouseDrag = Vector2.Zero;
        ConfirmBuilding = false;
    }

    private static void ProcessCameraInputs()
    {
        // If 'R' was pressed, set the CameraReset state
        CameraReset = keyboardState.IsKeyUp(Keys.R) && lastKeyboardState.IsKeyDown(Keys.R);

        // Track click-and-drag
        if (ClickAndHold)
        {
            _dragStart = new Vector2(mouseState.X, mouseState.Y);
        }
        else if (!Clicked && _dragStart != Vector2.Zero && lastMouseState.LeftButton == ButtonState.Pressed)
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
            SwitchToMode(CAMERA_MODE);
        }
        // Confirm the building if left click is pressed
        else if (Clicked)
        {
            ConfirmBuilding = true;
        }
        else if (ConfirmBuilding)
        {
            if (!ShiftHeld)
                SwitchToMode(CAMERA_MODE);
            else
                ConfirmBuilding = false;
        }
        else
        {
            ConfirmBuilding = false;
        }
    }

    public static void ConsumeClick(Object consumer)
    {
        ConfirmBuilding = false;
        ClickConsumed = true;
        ClickConsumer = consumer;
    }

    public static bool UnconsumedClick()
    {
        return Clicked && !ClickConsumed;
    }

    public static void Update()
    {
        lastKeyboardState = keyboardState;
        lastMouseState = mouseState;

        keyboardState = Keyboard.GetState();
        mouseState = Mouse.GetState();

        // MousePos will be deprojected by the GameManager using the Camera
        MousePos.X = mouseState.X;
        MousePos.Y = mouseState.Y;

        // ScreenMousePos will always refer to the position of the mouse on the screen (e.g. for static UI)
        ScreenMousePos.X = mouseState.X;
        ScreenMousePos.Y = mouseState.Y;

        previousMouseWheelValue = currentMouseWheelValue;
        currentMouseWheelValue = mouseState.ScrollWheelValue;
        ScrollValue = currentMouseWheelValue - previousMouseWheelValue;

        DetermineMode();

        Clicked = (mouseState.LeftButton == ButtonState.Released && 
                  lastMouseState.LeftButton == ButtonState.Pressed);
        ClickConsumed = false;
        ClickConsumer = null;

        ClickAndHold = 
            (mouseState.LeftButton == ButtonState.Pressed && 
            lastMouseState.LeftButton != ButtonState.Pressed);

        // Ignore off-screen clicks
        if (!KatabasisGame.Viewport.TitleSafeArea.Contains(ScreenMousePos))
        {
            Clicked = false;
            ClickAndHold = false;
        }

        MouseDown = (mouseState.LeftButton == ButtonState.Pressed);

        BPressed = keyboardState.IsKeyUp(Keys.B) && lastKeyboardState.IsKeyDown(Keys.B);
        IPressed = keyboardState.IsKeyUp(Keys.I) && lastKeyboardState.IsKeyDown(Keys.I);
        XPressed = keyboardState.IsKeyUp(Keys.X) && lastKeyboardState.IsKeyDown(Keys.X);
        PlusPressed = keyboardState.IsKeyUp(Keys.OemPlus) && lastKeyboardState.IsKeyDown(Keys.OemPlus);
        MinusPressed = keyboardState.IsKeyUp(Keys.OemMinus) && lastKeyboardState.IsKeyDown(Keys.OemMinus);
        
        ShiftHeld = (keyboardState.IsKeyDown(Keys.LeftShift) && lastKeyboardState.IsKeyDown(Keys.LeftShift)) ||
                    (keyboardState.IsKeyDown(Keys.RightShift) && lastKeyboardState.IsKeyDown(Keys.RightShift));

        SavePressed = (keyboardState.IsKeyDown(Keys.LeftControl) &&
            keyboardState.IsKeyUp(Keys.S) && lastKeyboardState.IsKeyDown(Keys.S));

        // Toggle show borders with the 'H' key
        if (keyboardState.IsKeyUp(Keys.H) && lastKeyboardState.IsKeyDown(Keys.H))
            Config.ShowBorders = !Config.ShowBorders;

        // Default to camera mode controls unless the mode blocks camera movement
        switch (Mode)
        {
            case BUILD_MODE : ProcessBuildingInputs(); break;
            case CAMERA_MODE: ProcessCameraInputs();   break;
            default: ProcessCameraInputs(); break;
        }
        
        // Cancel tile mode with right click
        if (Mode == TILE_MODE && 
            lastMouseState.RightButton == ButtonState.Released && 
            mouseState.RightButton == ButtonState.Pressed)
        {
            SwitchToMode(CAMERA_MODE);
        }

        if (keyboardState.IsKeyUp(Keys.P) && lastKeyboardState.IsKeyDown(Keys.P))
            Paused = !Paused;

        if ((keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt)) && 
             keyboardState.IsKeyUp(Keys.Enter) && lastKeyboardState.IsKeyDown(Keys.Enter))
        {
            KatabasisGame.Instance.ToggleFullscreen();
        }
    }

    // Register a mode along with instructions along with events for enabling and disabling it
    // and what callbacks to trigger for specific events while in that mode?
}