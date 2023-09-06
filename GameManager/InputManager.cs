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
    public static bool HoldConsumed;
    public static bool MouseDown;

    public static bool RClicked;
    public static bool RClickConsumed;
    public static object RClickConsumer;

    public static bool ShiftHeld;
    public static bool SavePressed;
    public static bool LoadPressed;

    public static Keys KeyConsumed;
    public static Object KeyConsumer;

    public static bool Paused = false;

    // Trigger to place a building and exit build mode
    public static bool ConfirmBuilding = false;

    // Current mouse position, will be corrected for camera transformations by GameManager
    public static Vector2 MousePos = Vector2.Zero;
    public static Vector2 WorldMousePos = Vector2.Zero;
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
        ConfirmBuilding = false;
    }

    public static bool UnconsumedKeypress(Keys key)
    {
        return 
            lastKeyboardState.IsKeyDown(key) && 
            keyboardState.IsKeyUp(key) &&
            KeyConsumed != key;
    }

    public static bool WasPressed(Keys key)
    {
        return 
            lastKeyboardState.IsKeyDown(key) && 
            keyboardState.IsKeyUp(key);
    }

    public static void ConsumeKeypress(Keys key, object consumer)
    {
        KeyConsumed = key;
        KeyConsumer = consumer;
    }

    public static void ConsumeHold(object consumer)
    {
        HoldConsumed = true;
    }

    public static bool UnconsumedHold()
    {
        return ClickAndHold && !HoldConsumed;
    }

    private static void ProcessBuildingInputs()
    {
        // Disable build mode on right click
        if (UnconsumedRClick())
        {
            ConsumeRClick(null);
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

    public static void ConsumeRClick(Object consumer)
    {
        RClickConsumed = true;
        RClickConsumer = consumer;
    }

    public static bool UnconsumedRClick()
    {
        return RClicked && !RClickConsumed;
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

        KeyConsumed = Keys.None;
        KeyConsumer = null;

        RClicked = lastMouseState.RightButton == ButtonState.Released && 
            mouseState.RightButton == ButtonState.Pressed;
        RClickConsumed = false;
        RClickConsumer = null;

        ClickAndHold = 
            (mouseState.LeftButton == ButtonState.Pressed && 
            lastMouseState.LeftButton != ButtonState.Pressed);
        HoldConsumed = false;

        // Ignore off-screen clicks
        if (!KatabasisGame.Viewport.TitleSafeArea.Contains(ScreenMousePos))
        {
            Clicked = false;
            ClickAndHold = false;
        }

        MouseDown = (mouseState.LeftButton == ButtonState.Pressed);
        
        ShiftHeld = (keyboardState.IsKeyDown(Keys.LeftShift) && lastKeyboardState.IsKeyDown(Keys.LeftShift)) ||
                    (keyboardState.IsKeyDown(Keys.RightShift) && lastKeyboardState.IsKeyDown(Keys.RightShift));

        
        SavePressed = (keyboardState.IsKeyUp(Keys.F4) && lastKeyboardState.IsKeyDown(Keys.F4));
        LoadPressed = (keyboardState.IsKeyUp(Keys.F5) && lastKeyboardState.IsKeyDown(Keys.F5));

        // Toggle show borders with the 'H' key
        if (keyboardState.IsKeyUp(Keys.H) && lastKeyboardState.IsKeyDown(Keys.H))
            Config.ShowBorders = !Config.ShowBorders;

        // Default to camera mode controls unless the mode blocks camera movement
        switch (Mode)
        {
            case BUILD_MODE : ProcessBuildingInputs(); break;
        }
        
        // Cancel tile mode with right click
        if (Mode == TILE_MODE && (UnconsumedRClick() || UnconsumedKeypress(Keys.Escape)))
        {
            if (UnconsumedRClick())
                ConsumeRClick(null);
            SwitchToMode(CAMERA_MODE);
        }

        if (keyboardState.IsKeyUp(Keys.Space) && lastKeyboardState.IsKeyDown(Keys.Space))
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