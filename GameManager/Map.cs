using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Map
{
    public Tile[] tiles { get; set; }
    public List<Sprite> Decorations { get; set; }

    public bool Unused {
        get { return false; }
        set { 
            foreach (Sprite decoration in Decorations)
                Globals.Ybuffer.Add(decoration);
        }
    }

    // Computed in constructor from texture
    [JsonIgnore]
    public Point MapSize { get; private set; }

    [JsonIgnore]
    public Vector2 Origin { get; private set; }

    private readonly Point _mapTileSize = new(128, 128);

    private Building _editBuilding = null;

    public Tile HighlightedTile;

    public static Point TileSize { get; private set; }

    public int VerticalOverlap;
    public int HorizontalOverlap;

    private const float HEXAGON_HEIGHT_RATIO = 0.8660254f;
    private const float SCALE_CONSTANT = 0.1f;

    public Map()
    {
        tiles = new Tile[_mapTileSize.X *_mapTileSize.Y];
        Decorations = new();

        // 500x345
        TileSize = new(Sprites.desertTextures[0].Texture.Width, Sprites.desertTextures[0].Texture.Height);
        MapSize = new(TileSize.X * _mapTileSize.X, TileSize.Y * _mapTileSize.Y);

        Origin = new(MapSize.X / 2, MapSize.Y / 2);

        VerticalOverlap = 30;
        HorizontalOverlap = TileSize.X / 2;

        _editBuilding = null;

        // Fix the map origin to account for overlap and perspective
        Origin = new(MapSize.X / 2 - HorizontalOverlap, MapSize.Y / 2 - VerticalOverlap * _mapTileSize.Y);
    }

    public void Generate()
    {
        Array plantTypes = Enum.GetValues(typeof(Goods.FoodPlant));

        int row = 1;
        int tiles_per_row = 1;
        int tile_in_row = 0;
        for (int n = 0; n < _mapTileSize.Y * _mapTileSize.X; n++)
        {
            SpriteTexture texture = null;
            TileType tileType = TileType.DESERT;
            MineralType mineralType = MineralType.NONE;
            Goods.FoodPlant plantType = Goods.FoodPlant.NONE;
            double r = Globals.Rand.NextDouble();

            // Assign random tile textures
            if (r < 0.65)
            {
                // 65% plain desert
                texture = Sprites.RandomDesert();
            }
            else if (r < 0.8)
            {
                // 15% desert with hills
                texture = Sprites.RandomHills();
                tileType = TileType.HILLS;
                mineralType = MineralInfo.Random();
            }
            else if (r < 0.98)
            {
                // 18% desert with vegetation
                texture = Sprites.RandomVegetation();
                tileType = TileType.VEGETATION;

                if (Globals.Rand.NextFloat(0.0f, 1.0f) <= 0.1f)
                    plantType = (Goods.FoodPlant)plantTypes.GetValue(Globals.Rand.Next(plantTypes.Length));
            }
            else
            {
                // 2% bedouin camps
                texture = Sprites.RandomCamp();
                tileType = TileType.CAMP;
            }

            // Rows get bigger until halfway, then they get smaller
            float xpos = 0f;
            if (row > _mapTileSize.Y)    
                xpos = -Origin.X + ((TileSize.X / 2) * row) + (tile_in_row * TileSize.X);
            else
                xpos = Origin.X - ((TileSize.X / 2) * row) + (tile_in_row * TileSize.X);

            // Each row is half a tile size down, shave off a bit because each tile has a thick base
            float ypos = (TileSize.Y / 2) * row - (VerticalOverlap * row);

            SpriteTexture feature = null;
            Tile tile = null;

            // 5% chance to add animals to plain desert tiles
            if (r < 0.05)
                tile = TileAnimal.Create(new(xpos, ypos), texture, feature);
            else
                tile = Tile.Create(tileType, new(xpos, ypos), texture, feature);

            if (mineralType != MineralType.NONE)
                tile.Minerals = mineralType;

            if (plantType != Goods.FoodPlant.NONE)
                tile.SetPlantType(plantType);

            tiles[n] = tile;

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

        ComputeNeighbors();
        GenerateForests();
        GenerateRivers();
    }

    public void ComputeNeighbors()
    {
        // Second iteration to assign neighbors
        int row = 1;
        int tiles_per_row = 1;
        int tile_in_row = 0;

        for (int i = 0; i < _mapTileSize.X * _mapTileSize.Y; i++)
        {
            Tile t = tiles[i];
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
                int offset = i - tiles_per_row + ((row <= _mapTileSize.Y) ? 1 : 0);
                if (row == _mapTileSize.Y + 1)
                    offset++;
                ne = tiles[offset];
            }
            // Top-half, all nodes have SE neighbor (except the last node in the middle row)
            // Bottom-half, last node has no SE neighbor
            if (tile_in_row < tiles_per_row - 1 || (!halfway && row <= _mapTileSize.Y))
            {
                int offset = i + tiles_per_row + ((row < _mapTileSize.Y) ? 1 : 0);
                if (row == _mapTileSize.Y)
                    offset--;
                se = tiles[offset];
            }
            // Top-half, last node in row has no NW neighbor
            // Bottom-half, all nodes have NW neighbor
            if (tile_in_row > 0 || halfway)
            {
                int offset = i - tiles_per_row  - (halfway ? 1 : 0);
                if (row == _mapTileSize.Y + 1)
                    offset++;
                nw = tiles[offset];
            }
            // Top-half, all nodes have SW neighbor (except the first node in the middle row)
            // Bottom-half, first node has no SW Neighbor
            if (tile_in_row > 0 || !halfway)
            {
                int offset = (row >= _mapTileSize.Y) ? 1 : 0;
                if (row == _mapTileSize.Y)
                    offset++;
                sw = tiles[i + tiles_per_row - offset];
            }
            t.Neighbors = new Tile[]{ ne, se, sw, nw };

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
    }

    public Tile GetOriginTile()
    {
        // Midpoint, rounded up wil be the origin for odd-sized maps,
        // even-sized mapps have no true origin, so this will give the tile SW of the center
        return tiles[(int)(tiles.Length / 2f + 0.5)];
    }

    public void GenerateRivers()
    {
        GenerateRiver(startingFromTop: true);
        GenerateRiver(startingFromTop: true);
        GenerateRiver(startingFromTop: true);
        GenerateRiver(startingFromTop: false);
        GenerateRiver(startingFromTop: false);
    }

    public void GenerateRiver(bool startingFromTop)
    {
        // Head a random number of steps south east from the starting tile
        int steps1 = Globals.Rand.Next(2, _mapTileSize.X);
        Tile t = startingFromTop ? tiles[0] : tiles[tiles.Length - 1];

        for (int i = 0; i < steps1; i++)
            t = t.Neighbors[startingFromTop ? (int)Cardinal.SE : (int)Cardinal.NW];

        // Place a random number of river tiles toward the center
        int length1 = Globals.Rand.Next(_mapTileSize.Y / 3, 3 * _mapTileSize.Y / 4);
        for (int i = 0; i < length1; i++)
        {
            t.MakeRiver();
            t = t.Neighbors[startingFromTop ? (int)Cardinal.SW : (int)Cardinal.NE];
        }
    }

    public void GenerateForests()
    {
        // Make some forests, e.g. about 80 for a 128x128 map
        int NUM_FORESTS = (int)(0.005 * _mapTileSize.X * _mapTileSize.Y);
        for (int i = 0; i < NUM_FORESTS; i++)
            GenerateForest();
    }

    public void ConvertToForest(Tile tile)
    {
        if (tile.Type != TileType.WILD_ANIMAL)
            tile.SetTileType(TileType.FOREST);
        tile.Minerals = MineralType.NONE;
        tile.BaseSprite.SetNewSpriteTexture(Sprites.RandomForest());
    }

    public void GenerateForest()
    {
        int i = Globals.Rand.Next(tiles.Length);
        Tile start = tiles[i];

        int w = Globals.Rand.Next(3, 6);

        // Traverse south-east from the start tile
        // At each step, draw a number of 
        Tile row = start;
        Tile column = start;
        for (int x = 0; x < w; x++)
        {
            row = row.Neighbors[(int)Cardinal.SE];
            if (row == null)
                break;

            ConvertToForest(row);

            // Draw some number of tiles to the northeast for this row
            int h = w;
            if (Globals.Rand.Next(2) == 0)
                h += (int)(w * Globals.Rand.NextFloat(0.2f, 0.3f));
            else
                h -= (int)(w * Globals.Rand.NextFloat(0.2f, 0.3f));

            column = row;

            for (int y = 0; y < h / 2; y++)
            {
                column = column.Neighbors[(int)Cardinal.NE];
                if (column == null)
                    break;
                ConvertToForest(column);
            }

            // Draw some number of tiles to the southwest for this row
            column = row;

            for (int y = 0; y < h / 2; y++)
            {
                column = column.Neighbors[(int)Cardinal.SW];
                if (column == null)
                    break;
                ConvertToForest(column);
            }
        }
    }

    public Tile TileAtPos(Vector2 pos)
    {
        // TODO: this should probably use a quad tree or something, searching >16,000 tiles is slow and unnecessary
        foreach (Tile t in tiles)
        {
            // Vaguely inside the bounding box for the tile (close enough tbh)
            float dist = Vector2.Distance(pos, t.BaseSprite.Position);
            if (dist < TileSize.X / 3.2)
            {
                return t;
            }
        }
        return null;
    }

    public void UnhighlightTile()
    {
        if (HighlightedTile != null)
        {
            HighlightedTile.Unhighlight();
            HighlightedTile.BaseSprite.SpriteColor = Color.White;
            HighlightedTile = null;
        }
    }

    public void MakeHighlightTileRed()
    {
        if (HighlightedTile != null)
        {
            HighlightedTile.BaseSprite.SpriteColor = Color.OrangeRed;
        }
    }

    public void Update()
    {
        if (InputManager.Mode == InputManager.TILE_MODE)
        {
            Tile t = TileAtPos(InputManager.MousePos);
            if (t != null && t != HighlightedTile)
            {
                // Clear the highlighted tile
                if (HighlightedTile != null)
                    UnhighlightTile();

                HighlightedTile = t;
                HighlightedTile.Highlight();
            }
        }
        else if (HighlightedTile != null)
        {
            UnhighlightTile();
        }

        if (_editBuilding != null)
        {
            // Make the currently editing building follow the mouse pointer
            _editBuilding.Sprite.Position = InputManager.MousePos;

            Tile location = TileAtPos(InputManager.MousePos);
            if (Building.ValidPlacement(_editBuilding, location))
                _editBuilding.Sprite.SpriteColor = new Color(Color.LightBlue, 0.3f);
            else
                _editBuilding.Sprite.SpriteColor = new Color(Color.OrangeRed, 0.3f);

            // Confirm and add the building (stop editing)
            if (InputManager.ConfirmBuilding)
            {
                _editBuilding.Sprite.SpriteColor = Color.White;
                AddBuilding(_editBuilding);
            
                // Keep building more of the same type if shift is held
                if (InputManager.ShiftHeld)
                    CreateEditBuilding(_editBuilding.Type);
                else
                    _editBuilding = null;
            }
            // Resize the building before placing it (scroll wheel while in build mode)
            else if (InputManager.Mode == InputManager.BUILD_MODE && InputManager.ScrollValue > 0)
            {
                _editBuilding.Sprite.ScaleUp(SCALE_CONSTANT);
            }
            else if (InputManager.Mode == InputManager.BUILD_MODE && InputManager.ScrollValue < 0)
            {
                _editBuilding.Sprite.ScaleDown(SCALE_CONSTANT);
            }
            else if (InputManager.Mode != InputManager.BUILD_MODE)
            {
                _editBuilding = null;
            }            
        }
        else if (_editBuilding == null && InputManager.Mode == InputManager.BUILD_MODE)
        {
            // When build mode is first enabled, create a building at the mouse cursor
            //CreateEditBuilding();
        }

        // Update tiles
        foreach (Tile t in tiles)
            t.Update();
    }

    public void DailyUpdate()
    {
        foreach (Tile t in tiles)
            t.DailyUpdate();
    }

    public void CreateEditBuilding(BuildingType buildingType = BuildingType.NONE)
    {
        Building b = Building.Random(type: buildingType, temporary: true);
        b.Sprite.Position = InputManager.MousePos;
        _editBuilding = b;
        _editBuilding.Sprite.SpriteColor = new Color(Color.LightBlue, 0.3f);
    }

    public void ClearEditBuilding()
    {
        _editBuilding = null;
    }

    public void DrawTiles()
    {
        Tile.DisplayType displayType = Tile.DisplayType.DEFAULT;
        if (_editBuilding != null && _editBuilding.Type == BuildingType.FARM)
            displayType = Tile.DisplayType.SOIL_QUALITY;
        else if (_editBuilding != null && _editBuilding.Type == BuildingType.MINE)
            displayType = Tile.DisplayType.MINERALS;
        else if (_editBuilding != null && _editBuilding.Type == BuildingType.RANCH)
            displayType = Tile.DisplayType.PLACING_RANCH;
        else if (InputManager.Mode == InputManager.TILE_MODE)
            displayType = Tile.DisplayType.BUYING_TILE;

        // Draw map tiles
        int numTiles = _mapTileSize.X * _mapTileSize.Y;
        for (int n = 0; n < numTiles; n++)
        {
            tiles[n].Draw(displayType);

            // Debuging - show where the sprite's position is (it's more or less in the center of the isometric shape)
            //Sprites.Circle.Position = tiles[n].BaseSprite.Position;
            //Sprites.Circle.Draw();
        }

        // Draw fog on top
        if (!Config.ShowFog)
            return;

        for (int n = 0; n < numTiles; n++)
            tiles[n].DrawTopLayer();
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
        Tile t = b.Location;
        if (t == null)
            t = TileAtPos(b.Sprite.Position);

        if (t == null)
            Console.WriteLine("Failed to find tile at position " + b.Sprite.Position.ToString());
        else if (!Building.ConfirmBuilding(b, t))
            Console.WriteLine("Failed to add building at tile " + t.ToString());
    }

    public void AddDecoration(Sprite decoration)
    {
        Decorations.Add(decoration);
        Globals.Ybuffer.Add(decoration);
    }
}