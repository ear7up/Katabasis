using System;
using System.Collections.Generic;

public class Map
{
    private readonly Point _mapTileSize = new(128, 128);
    private readonly Tile[] _tiles;

    private readonly SortedList<int, Building> _buildings;
    private Building _editBuilding;

    private Tile _highlightedTile;

    public static Point TileSize { get; private set; }
    public Point MapSize { get; private set; }
    public Vector2 Origin { get; private set; }

    private const float HEXAGON_HEIGHT_RATIO = 0.8660254f;
    private const float SCALE_CONSTANT = 0.1f;

    public Map()
    {
        _tiles = new Tile[_mapTileSize.X *_mapTileSize.Y];

        // Load all of the tile textures
        List<Texture2D> desertTextures = Sprites.LoadTextures("desert/flat", 30);
        List<Texture2D> desertHillTextures = Sprites.LoadTextures("desert/flat", 5);
        List<Texture2D> desertVegetationTextures = Sprites.LoadTextures("desert/vegetation", 6);
        List<Texture2D> desertBedouinTextures = Sprites.LoadTextures("desert/bedouin_camps", 5);

        // 500x345
        TileSize = new(desertTextures[0].Width, desertTextures[0].Height);
        MapSize = new(TileSize.X * _mapTileSize.X, TileSize.Y * _mapTileSize.Y);

        _buildings = new();
        _editBuilding = null;
        
        int VERTICAL_OVERLAP = 30;
        int HORIZONTAL_OVERLAP = TileSize.X / 2;
        Origin = new(MapSize.X / 2, MapSize.Y / 2);

        Random random = new();

        int row = 1;
        int tiles_per_row = 1;
        int tile_in_row = 0;

        for (int n = 0; n < _mapTileSize.Y * _mapTileSize.X; n++)
        {
            Texture2D texture = null;
            double r = random.NextDouble();

            // Assign random tile textures
            if (r < 0.65)
            {
                // 65% plain desert
                texture = desertTextures[random.Next(0, desertTextures.Count)];
            }
            else if (r < 0.8)
            {
                // 15% desert with hills
                texture = desertTextures[random.Next(0, desertHillTextures.Count)];
            }
            else if (r < 0.98)
            {
                // 18% desert with vegetation
                texture = desertVegetationTextures[random.Next(0, desertVegetationTextures.Count)];
            }
            else
            {
                // 2% bedouin camps
                texture = desertBedouinTextures[random.Next(0, desertBedouinTextures.Count)];
            }

            // Rows get bigger until halfway, then they get smaller
            float xpos = 0f;
            if (row > _mapTileSize.Y)    
                xpos = -Origin.X + ((TileSize.X / 2) * row) + (tile_in_row * TileSize.X);
            else
                xpos = Origin.X - ((TileSize.X / 2) * row) + (tile_in_row * TileSize.X);

            // Each row is half a tile size down, shave off a bit because each tile has a thick base
            float ypos = (TileSize.Y / 2) * row - (VERTICAL_OVERLAP * row);

            Texture2D feature = null;
            Tile tile = new(new(xpos, ypos), texture, feature);
            _tiles[n] = tile;

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
        }

        // Second iteration to assign neighbors
        row = 1;
        tiles_per_row = 1;
        tile_in_row = 0;

        for (int i = 0; i < _mapTileSize.X * _mapTileSize.Y; i++)
        {
            Tile t = _tiles[i];
            Tile ne = null;
            Tile se = null;
            Tile nw = null;
            Tile sw = null;

            bool halfway = row > _mapTileSize.Y;

            // Top-half, last node has no NE neighbor
            // Bottom-half, all nodes have NE neighbor
            if (tile_in_row < tiles_per_row - 1 || halfway)
            {
                // the row right after the midpoint is wrong
                ne = _tiles[i - tiles_per_row + ((row <= _mapTileSize.Y) ? 1 : 0)];
            }
            // Top-half, all nodes have SE neighbor (except the last node in the middle row)
            // Bottom-half, last node has no SE neighbor
            if (tile_in_row < tiles_per_row - 1 || (!halfway && row != _mapTileSize.Y))
            {
                se = _tiles[i + tiles_per_row + ((row < _mapTileSize.Y) ? 1 : 0)];
            }
            // Top-half, last node in row has no NW neighbor
            // Bottom-half, all nodes have NW neighbor
            if (tile_in_row > 0 || halfway)
            {
                nw = _tiles[i - tiles_per_row  - (halfway ? 1 : 0)];
            }
            // Top-half, all nodes have SW neighbor (except the first node in the middle row)
            // Bottom-half, first node has no SW Neighbor
            if (tile_in_row > 0 || !halfway)
            {
                sw = _tiles[i + tiles_per_row - ((row >= _mapTileSize.Y) ? 1 : 0)];
            }
            t.neighbors = new Tile[]{ ne, se, sw, nw };

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
        }

        GenerateRivers();

        // Fix the map origin to account for overlap and perspective
        Origin = new(MapSize.X / 2 - HORIZONTAL_OVERLAP, MapSize.Y / 2 - VERTICAL_OVERLAP * _mapTileSize.Y);
    }

    public Tile GetOriginTile()
    {
        // Midpoint, rounded up wil be the origin for odd-sized maps,
        // even-sized mapps have no true origin, so this will give the tile SW of the center
        return _tiles[(int)(_tiles.Length / 2f + 0.5)];
    }

    public void GenerateRivers()
    {
        List<Texture2D> desertRiverTextures = Sprites.LoadTextures("desert/river", 16);
        Random random = new();
        GenerateTopRiver(desertRiverTextures, random);
        GenerateTopRiver(desertRiverTextures, random);
        GenerateTopRiver(desertRiverTextures, random);
        GenerateBottomRiver(desertRiverTextures, random);
        GenerateBottomRiver(desertRiverTextures, random);
    }

    public void GenerateTopRiver(List<Texture2D> desertRiverTextures, Random random)
    {
        // Head southeast a random number of times from the top tile
        int steps1 = random.Next(2, _mapTileSize.X);
        Tile top = _tiles[0];
        for (int i = 0; i < steps1; i++)
        {
            top = top.neighbors[(int)Tile.Cardinal.SE];
        }

        int length1 = random.Next(_mapTileSize.Y / 3, 3 * _mapTileSize.Y / 4);
        for (int i = 0; i < length1; i++)
        {
            // Overwrite the tile with a river and head south-west
            top.BaseSprite.Texture = desertRiverTextures[random.Next(0, desertRiverTextures.Count)];
            top = top.neighbors[(int)Tile.Cardinal.SW];
        }
    }

    public void GenerateBottomRiver(List<Texture2D> desertRiverTextures, Random random)
    {
        // Head northwest a random number of times from the bottom tile        
        int steps2 = random.Next(2, _mapTileSize.X);
        Tile bottom = _tiles[_tiles.Length - 1];
        for (int i = 0; i < steps2; i++)
        {
            bottom = bottom.neighbors[(int)Tile.Cardinal.NW];
        }
        
        int length2 = random.Next(_mapTileSize.Y / 3, 3 * _mapTileSize.Y / 4);
        for (int i = 0; i < length2; i++)
        {
            // Overwrite the tile with a river and head north-east
            bottom.BaseSprite.Texture = desertRiverTextures[random.Next(0, desertRiverTextures.Count)];
            bottom = bottom.neighbors[(int)Tile.Cardinal.NE];
        }
    }

    public void Update()
    {
        if (InputManager.Mode == InputManager.TILE_MODE)
        {
            // TODO: this should probably use a quad tree or something, searching >16,000 tiles is slow and unnecessary
            foreach (Tile t in _tiles)
            {
                // Vaguely inside the bounding box for the tile (close enough tbh)
                float dist = Vector2.Distance(InputManager.MousePos, t.BaseSprite.Position);
                if (dist < TileSize.X / 2)
                {
                    // Clear the highlighted tile
                    if (_highlightedTile != null)
                    {
                        _highlightedTile.Unhighlight();
                    }

                    _highlightedTile = t;
                    _highlightedTile.Highlight();
                }
            }
        }
        else if (_highlightedTile != null)
        {
            _highlightedTile.Unhighlight();
            _highlightedTile = null;
        }

        if (_editBuilding != null)
        {
            // Make the currently editing buliding follow the mouse pointer
            _editBuilding.sprite.Position = InputManager.MousePos;

            // Confirm and add the building (stop editing)
            if (InputManager.ConfirmBuilding)
            {
                _editBuilding.sprite.SpriteColor = Color.White;
                AddBuilding(_editBuilding);
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

    public void DrawTiles()
    {
        // Draw map tiles
        for (int n = 0; n < _mapTileSize.X * _mapTileSize.Y; n++)
        {
            _tiles[n].Draw();
        }
    }

    public void DrawUI()
    {
        // If the user is placing a building, draw that temporary sprite
        if (_editBuilding != null)
        {
            _editBuilding.Draw();
        }
    }

    // Keep building list sorted by y order
    public void AddBuilding(Building b)
    {
        int y = (int)b.sprite.Position.Y;
        _buildings.Add(y, b);
    }

    public void DrawBuildings()
    {
        // Draw each permanent building
        foreach (KeyValuePair<int, Building> b in _buildings)
        {
            b.Value.Draw();
        }
    }
}