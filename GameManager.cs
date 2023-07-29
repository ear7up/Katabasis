using System;

namespace Katabasis;

public class GameManager
{
    private readonly Map _map;
    private readonly Camera _camera;

    public GameManager()
    {
        _map = new();
        //_camera = new();
        _camera = new(KatabasisGame.Viewport, _map.Origin);
        //_camera.SetBounds(_map.MapSize, _map.TileSize);
    }

    public void Update()
    {
        InputManager.Update();
        _camera.UpdateCamera(KatabasisGame.Viewport);

        // Calculate the real mouse position by inverting the camera transformations
        InputManager.MousePos = Vector2.Transform(InputManager.MousePos, Matrix.Invert(_camera.Transform));

        _map.Update();
    }

    public void Draw()
    {
        Globals.SpriteBatch.Begin(transformMatrix: _camera.Transform);
        _map.Draw();
        Globals.SpriteBatch.End();

        // Draw the UI on top of the map, do not apply transformations to it
    }
}