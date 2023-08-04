public class Camera
{
    public float Zoom { get; set; }
    public Vector2 Position { get; set; }
    public Rectangle Bounds { get; protected set; }
    public Rectangle VisibleArea { get; protected set; }
    public Matrix Transform { get; protected set; }
    public Matrix InverseViewMatrix { get; protected set; }
    public Person Following { get; protected set; }
    
    private Vector2 StartingPosition;
    private const float DEFAULT_ZOOM = 0.4f;
    private const float MIN_ZOOM = 0.15f;
    private const float MAX_ZOOM = 3.0f;

    public Camera(Viewport viewport, Vector2 position)
    {
        Bounds = viewport.Bounds;
        Zoom = DEFAULT_ZOOM;
        Position = position;
        StartingPosition = position;
    }

    public Vector2 DeprojectScreenPosition(Vector2 position)
    {
        return Vector2.Transform(position, InverseViewMatrix);
    }
    public Vector2 DeprojectScreenPosition(Point position) // for MouseState.Position
    {
        return Vector2.Transform(new Vector2(position.X, position.Y), InverseViewMatrix);
    }

    private void UpdateVisibleArea()
    {
        InverseViewMatrix = Matrix.Invert(Transform);

        var tl = Vector2.Transform(Vector2.Zero, InverseViewMatrix);
        var tr = Vector2.Transform(new Vector2(Bounds.X, 0), InverseViewMatrix);
        var bl = Vector2.Transform(new Vector2(0, Bounds.Y), InverseViewMatrix);
        var br = Vector2.Transform(new Vector2(Bounds.Width, Bounds.Height), InverseViewMatrix);

        var min = new Vector2(
            MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
            MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
        var max = new Vector2(
            MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
            MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
        VisibleArea = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
    }

    private void UpdateMatrix()
    {
        Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateScale(Zoom, Zoom, 1f) *
                Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));

        UpdateVisibleArea();
    }

    public void MoveCamera(Vector2 movePosition)
    {
        Vector2 newPosition = Position + movePosition;
        Position = newPosition;
    }

    public void Follow(Person person)
    {
        Following = person;
    }

    public void Unfollow()
    {
        Following = null;
    }

    public void AdjustZoom(float zoomAmount)
    {
        Zoom = MathHelper.Clamp(Zoom + zoomAmount, MIN_ZOOM, MAX_ZOOM);
    }

    public void Reset()
    {
        Position = StartingPosition;
        Zoom = DEFAULT_ZOOM;
    }

    public void UpdateCamera(Viewport bounds)
    {
        if (InputManager.Mode == InputManager.CAMERA_MODE && InputManager.CameraReset)
        {
            Reset();
        }

        Bounds = bounds.Bounds;
        UpdateMatrix();

        Vector2 cameraMovement = Vector2.Zero;
        int moveSpeed;

        if (Zoom > .8f)
        {
            moveSpeed = 15;
        }
        else if (Zoom < .8f && Zoom >= .6f)
        {
            moveSpeed = 20;
        }
        else if (Zoom < .6f && Zoom > .35f)
        {
            moveSpeed = 25;
        }
        else if (Zoom <= .35f)
        {
            moveSpeed = 30;
        }
        else
        {
            moveSpeed = 10;
        }

        // Allow WASD even while not in CAMERA_MODE
        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            cameraMovement.Y = -moveSpeed;
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            cameraMovement.Y = moveSpeed;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            cameraMovement.X = -moveSpeed;
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            cameraMovement.X = moveSpeed;
        }

        if (InputManager.Mode == InputManager.CAMERA_MODE)
        {
            // Drag movement
            if (InputManager.MouseDrag != Vector2.Zero)
            {
                cameraMovement = Vector2.Transform(InputManager.MouseDrag, Matrix.Invert(Matrix.CreateScale(Zoom)));
            }

            // Zooming with scroll wheel
            if (InputManager.ScrollValue > 0)
            {
                AdjustZoom(.05f);
            }
            else if (InputManager.ScrollValue < 0)
            {
                AdjustZoom(-.05f);
            }
        }

        if (Following != null)
            Position = Following.Position;
        else
            MoveCamera(cameraMovement);
    }
}