using System;
using System.Collections.Generic;

public class Map
{
    private readonly Point _mapTileSize = new(16, 16);
    private readonly Tile[,] _tiles;
    private readonly List<Building> _buildings;
    private Building _editBuilding;

    public Point TileSize { get; private set; }
    public Point MapSize { get; private set; }
    public Vector2 Origin { get; private set; }

    private const float HEXAGON_HEIGHT_RATIO = 0.8660254f;
    private const float SCALE_CONSTANT = 0.1f;

    public Map()
    {
        _tiles = new Tile[_mapTileSize.X, _mapTileSize.Y];
        _buildings = new List<Building>();
        _editBuilding = null;

        // Load all of the tile textures
        const int DESERT_TILECOUNT = 5;
        List<Texture2D> desertTextures = new();
        for (int i = 1; i <= DESERT_TILECOUNT; i++)
        {
            desertTextures.Add(Globals.Content.Load<Texture2D>($"desert/{i:000}"));
        }

        // 512x512
        TileSize = new(desertTextures[0].Width, desertTextures[0].Height);
        MapSize = new(TileSize.X * _mapTileSize.X, TileSize.Y * _mapTileSize.Y);
        Origin = new(MapSize.X / 2, MapSize.Y / 2);

        Random random = new();

        int row = 1;
        int tiles_per_row = 1;
        int tile_in_row = 0;

        for (int y = 0; y < _mapTileSize.Y; y++)
        {
            for (int x = 0; x < _mapTileSize.X; x++)
            {
                // Assign random tile textures
                int r = random.Next(0, desertTextures.Count);
                float xpos = 0f;

                // Rows get bigger until halfway, then they get smaller
                if (row > _mapTileSize.Y)    
                    xpos = -Origin.X + ((TileSize.X / 2) * row) + (tile_in_row * TileSize.X);
                else
                    xpos = Origin.X - ((TileSize.X / 2) * row) + (tile_in_row * TileSize.X);

                // Each row is half a tile size down, shave off a bit because each tile has a thick base
                float ypos = (TileSize.Y / 2) * row - (30 * row);

                tile_in_row++;

                // Start a new row
                if (tile_in_row >= tiles_per_row)
                {
                    tile_in_row = 0;
                    row++;

                    // Halfway through, each row will have fewer
                    if (row > _mapTileSize.Y)
                        tiles_per_row--;
                    else
                        tiles_per_row++;
                }

                Texture2D feature = null;
                _tiles[x, y] = new(new(xpos, ypos), desertTextures[r], feature);
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