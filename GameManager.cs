using System;
using System.Collections.Generic;

namespace Katabasis;

public class GameManager
{
    private readonly Sprite _sky;
    private readonly Map _map;
    private readonly Camera _camera;
    private List<Person> _people;

    public const int TICKS_PER_DAY = 3600;
    private long TickCounter = 0;
    

    public GameManager()
    {
        _sky = new Sprite(Globals.Content.Load<Texture2D>("sky"), Vector2.Zero);
        _sky.Scale = 2f;
        _map = new();
        _camera = new(KatabasisGame.Viewport, _map.Origin);
        //_camera.SetBounds(_map.MapSize, _map.TileSize);
        _people = new();

        Goods.CalcGoodsTypecounts();
        for (int i = 0 ; i < 1000; i++)
        {
            _people.Add(Person.CreatePerson(_map.Origin));
        }

        RunTests();
    }

    public void RunTests()
    {
        //MarketTests.RunTests();
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

        TickCounter++;
        if (TickCounter > TICKS_PER_DAY)
        {
            TickCounter = 0;
            foreach (Person p in _people)
            {
                p.DailyUpdate();
            }
        }
    }

    public void Draw()
    {
        // Draw background independent of transformations
        Globals.SpriteBatch.Begin();
        _sky.Draw();
        Globals.SpriteBatch.End();

        Globals.SpriteBatch.Begin(transformMatrix: _camera.Transform);
        _map.DrawTiles();
        foreach (Person p in _people)
        {
            p.Draw();
        }
        _map.DrawBuildings();
        _map.DrawUI();
        Globals.SpriteBatch.End();

        // Draw the UI on top of the map, do not apply transformations to it
    }
}