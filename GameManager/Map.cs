using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Map
{
    public Tile[][] Tiles { get; set; }
    public Vector2 Origin { get; set; }
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

    private readonly Point _mapDimensions = new(127, 127);

    public Tile HighlightedTile;

    public static Point TileSize { get; private set; }

    public const int VerticalOverlap = 30;
    public static int HorizontalOverlap;

    private const float HEXAGON_HEIGHT_RATIO = 0.8660254f;
    private const float SCALE_CONSTANT = 0.1f;

    public static Vector2 NorthernTip;
    public static Vector2 SouthEast;
    public static Vector2 SouthWest;

    public Map()
    {
        Tiles = new Tile[_mapDimensions.Y][];
        for (int col = 0; col < Tiles.Length; col++)
            Tiles[col] = new Tile[_mapDimensions.X];

        Decorations = new();

        // 500x345
        TileSize = new(Sprites.desertTextures[0].Texture.Width, Sprites.desertTextures[0].Texture.Height);
        MapSize = new(TileSize.X * _mapDimensions.X, (TileSize.Y - VerticalOverlap) * _mapDimensions.Y);

        HorizontalOverlap = TileSize.X / 2;

        NorthernTip = new Vector2(MapSize.X / 2f, 0f);
        SouthEast = new Vector2(TileSize.X / 2f, TileSize.Y / 2f - VerticalOverlap);
        SouthWest = new Vector2(-TileSize.X / 2f, TileSize.Y / 2f - VerticalOverlap);

        Origin = Vector2.Zero;
    }

    public Tile GenerateTile(Vector2 coordinate, Vector2 pos)
    {
        Tile tile = null;
        SpriteTexture texture = null;
        TileType tileType = TileType.DESERT;

        Array plantTypes = Enum.GetValues(typeof(Goods.FoodPlant));
        Goods.FoodPlant plantType = Goods.FoodPlant.NONE;
        MineralType mineralType = MineralType.NONE;

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

        // 5% chance to add animals to plain desert tiles
        if (r < 0.05)
            tile = TileAnimal.Create(coordinate, pos, texture);
        else
            tile = Tile.Create(coordinate, tileType, pos, texture);

        if (mineralType != MineralType.NONE)
            tile.Minerals = mineralType;

        if (plantType != Goods.FoodPlant.NONE)
            tile.SetPlantType(plantType);

        return tile;
    }

    public void Generate()
    {
        Vector2 pos = Vector2.Zero;
        for (int col = 0; col < _mapDimensions.Y; col++)
        {
            pos = NorthernTip + SouthWest * col;
            for (int row = 0; row < _mapDimensions.X; row++)
            {
                pos += SouthEast;
                Tiles[col][row] = GenerateTile(new Vector2(row, col), pos);
            }
        }

        Origin = GetOriginTile().GetPosition();

        ComputeNeighbors();
        GenerateForests();
        GenerateRivers();
    }

    public void ComputeNeighbors()
    {
        // Second iteration to assign neighbors
        for (int y = 0; y < _mapDimensions.Y; y++)
        {
            for (int x = 0; x < _mapDimensions.X; x++)
            {
                Tile t = Tiles[y][x];
                Tile ne = null;
                Tile se = null;
                Tile nw = null;
                Tile sw = null;

                if (y - 1 > 0)
                    ne = Tiles[y - 1][x];
                if (x + 1 < _mapDimensions.X)
                    se = Tiles[y][x + 1];
                if (x - 1 > 0)
                    nw = Tiles[y][x - 1];
                if (y + 1 < _mapDimensions.Y)
                    sw = Tiles[y + 1][x];

                t.Neighbors = new Tile[]{ ne, se, sw, nw };
            }
        }
    }

    public Tile GetOriginTile()
    {
        // Midpoint, rounded up wil be the origin for odd-sized maps,
        // even-sized mapps have no true origin, so this will give the tile SW of the center
        return Tiles[_mapDimensions.Y / 2][_mapDimensions.X / 2];
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
        int steps1 = Globals.Rand.Next(2, _mapDimensions.X);
        Tile t = startingFromTop ? Tiles[0][0] : Tiles[_mapDimensions.Y - 1][_mapDimensions.X - 1];

        for (int i = 0; i < steps1; i++)
            t = t.Neighbors[startingFromTop ? (int)Cardinal.SE : (int)Cardinal.NW];

        // Place a random number of river tiles toward the center
        int length1 = Globals.Rand.Next(_mapDimensions.Y / 3, 3 * _mapDimensions.Y / 4);
        for (int i = 0; i < length1 && t != null; i++)
        {
            t.MakeRiver();
            t = t.Neighbors[startingFromTop ? (int)Cardinal.SW : (int)Cardinal.NE];
        }
    }

    public void GenerateForests()
    {
        // Make some forests, e.g. about 80 for a 128x128 map
        int NUM_FORESTS = (int)(0.005 * _mapDimensions.X * _mapDimensions.Y);
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
        int ycoord = Globals.Rand.Next(_mapDimensions.Y);
        int xcoord = Globals.Rand.Next(_mapDimensions.X);
        Tile start = Tiles[ycoord][xcoord];

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

    // Calculates grid position from a world position in constant time
    public Tile TileAtPos(Vector2 pos)
    {
        Tile originTile = GetOriginTile();
        Vector2 offsetFromOrigin = pos - originTile.GetPosition();

        // No idea why this is off by 10% but it is
        double yoff = 1.1 * offsetFromOrigin.Y / (TileSize.Y - VerticalOverlap);
        double xoff = offsetFromOrigin.X / TileSize.X;

        int newy = (int)Math.Round(originTile.Coordinate.Y - xoff + yoff);
        int newx = (int)Math.Round(originTile.Coordinate.X + xoff + yoff);

        if (newx > 0 && newx < _mapDimensions.X && newy > 0 && newy < _mapDimensions.Y)
            return Tiles[newy][newx];
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

    public static Vector2 rotate(Vector2 v, float delta) {
    return new Vector2(
        v.X * MathF.Cos(delta) - v.Y * MathF.Sin(delta),
        v.X * MathF.Sin(delta) + v.Y * MathF.Cos(delta)
    );
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

        // Update tiles
        for (int y = 0; y < Tiles.Length; y++)
            for (int x = 0 ; x < Tiles[y].Length; x++)
                Tiles[y][x].Update();
    }

    public void DailyUpdate()
    {
        for (int y = 0; y < Tiles.Length; y++)
            for (int x = 0 ; x < Tiles[y].Length; x++)
                Tiles[y][x].DailyUpdate();
    }

    public void DrawTiles(Tile.DisplayType displayType)
    {
        // Draw map tiles
        for (int y = 0; y < _mapDimensions.Y; y++)
            for (int x = 0; x < _mapDimensions.X; x++)
                Tiles[y][x].Draw(displayType);

        // Draw extra data on top (e.g. fog of war or UI icons associated with tiles)
        for (int y = 0; y < _mapDimensions.Y; y++)
            for (int x = 0; x < _mapDimensions.X; x++)
                Tiles[y][x].DrawTopLayer();
    }

    public void DrawUI()
    {
        
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