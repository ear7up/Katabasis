using System;
using System.Collections.Generic;

namespace Katabasis;

public class GameManager
{
    private readonly Map _map;
    private readonly Camera _camera;
    private List<Person> _people;

    public GameManager()
    {
        _map = new();
        _camera = new(KatabasisGame.Viewport, _map.Origin);
        //_camera.SetBounds(_map.MapSize, _map.TileSize);
        _people = new();

        for (int i = 0 ; i < 1000; i++)
        {
            _people.Add(Person.CreatePerson(_map.Origin));
        }
    }

    public void Update()
    {
        InputManager.Update();
        _camera.UpdateCamera(KatabasisGame.Viewport);

        // Calculate the real mouse position by inverting the camera transformations
        InputManager.MousePos = Vector2.Transform(InputManager.MousePos, Matrix.Invert(_camera.Transform));

        foreach (Person p in _people)
        {
            p.Update();
        }

        _map.Update();
    }

    public void Draw()
    {
        Globals.SpriteBatch.Begin(transformMatrix: _camera.Transform);
        _map.Draw();
        foreach (Person p in _people)
        {
            p.Draw();
        }
        Globals.SpriteBatch.End();

        // Draw the UI on top of the map, do not apply transformations to it
    }
}