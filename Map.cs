using System;
using System.Collections.Generic;

public class Map
{
    private readonly Point _mapTileSize = new(6, 15);
    private readonly Sprite[,] _tiles;
    private readonly List<Building> _buildings;
    private Building _editBuilding;

    public Point TileSize { get; private set; }
    public Point MapSize { get; private set; }
    public Vector2 Origin { get; private set; }

    private const float HEXAGON_HEIGHT_RATIO = 0.8660254f;
    private const float SCALE_CONSTANT = 0.1f;

    public Map()
    {
        _tiles = new Sprite[_mapTileSize.X, _mapTileSize.Y];
        _buildings = new List<Building>();
        _editBuilding = null;

        // Load all of the tile textures
        List<Texture2D> textures = new();
        textures.Add(Globals.Content.Load<Texture2D>("desert"));
        textures.Add(Globals.Content.Load<Texture2D>("desert_hill"));
        textures.Add(Globals.Content.Load<Texture2D>("desert_hill2"));

        // 512x512
        TileSize = new(textures[0].Width, textures[0].Height);
        MapSize = new(TileSize.X * _mapTileSize.X, TileSize.Y * _mapTileSize.Y);
        Origin = new(MapSize.X / 2, MapSize.Y / 2);

        Random random = new();

        for (int y = 0; y < _mapTileSize.Y; y++)
        {
            for (int x = 0; x < _mapTileSize.X; x++)
            {
                // Assign random tile textures
                int r = random.Next(0, textures.Count);

                // Line hexagons horizontally, leaving a gap of 1/2 width
                float xpos = x * TileSize.X * 1.5f;

                // Hexagons don't fill up a bounding box, but they have a fixed ratio between width and height
                float ypos = y * TileSize.Y * HEXAGON_HEIGHT_RATIO * 0.5f;

                // Every other row should be shifted over 75% of its width so that the uppper-left corner of its
                // bounding box lines up with the bottom-right corner of the hexagon above (the bounding boxes will overlap)
                if (y % 2 == 1)
                {
                    xpos += TileSize.X * 0.75f;
                }

                _tiles[x, y] = new(textures[r], new(xpos, ypos));
            }
        }
    }

    public void Update()
    {
        if (_editBuilding != null)
        {
            // Make the currently editing buliding follow the mouse pointer
            _editBuilding.sprite.Position = InputManager.MousePos;

            // Confirm and add the building (stop editing)
            if (InputManager.ConfirmBuilding)
            {
                _editBuilding.sprite.SpriteColor = Color.White;
                _buildings.Insert(0, _editBuilding);
                _editBuilding = null;
            }
            else if (InputManager.Mode != InputManager.BUILD_MODE)
            {
                _editBuilding = null;
            }

            // Resize the buliding before placing it (scroll wheel while in build mode)
            if (InputManager.Mode == InputManager.BUILD_MODE && InputManager.ScrollValue > 0)
            {
                _editBuilding.sprite.ScaleUp(SCALE_CONSTANT);
            }
            else if (InputManager.Mode == InputManager.BUILD_MODE && InputManager.ScrollValue < 0)
            {
                _editBuilding.sprite.ScaleDown(SCALE_CONSTANT);
            }
        }
        // When build mode is first enabled, create a building at the mouse cursor
        else if (_editBuilding == null && InputManager.Mode == InputManager.BUILD_MODE)
        {
            Building b = Building.Random();
            b.sprite.Position = InputManager.MousePos;
            _editBuilding = b;
            _editBuilding.sprite.SpriteColor = new Color(Color.LightBlue, 0.3f);
        }
    }

    public void Draw()
    {
        // Draw map tiles
        for (int y = 0; y < _mapTileSize.Y; y++)
        {
            for (int x = 0; x < _mapTileSize.X; x++) 
            {
                _tiles[x, y].Draw();
            }
        }

        // If the user is placing a building, draw that temporary sprite
        if (_editBuilding != null)
        {
            _editBuilding.Draw();
        }

        // Draw each permanent building
        foreach (Building b in _buildings)
        {
            b.Draw();
        }
    }
}