using System;
using System.Collections.Generic;

public class Animal : Entity, Drawable
{
    public static int IdCounter = 0;
    public const float MOVE_SPEED = 10f;

    private int Id;
    public Tile Home;
    private Vector2 Destination;

    public Animal(Tile home, Texture2D texture) : base()
    {
        Id = IdCounter++;
        Home = home;
        Position = home.GetPosition();
        Destination = Position;
        Scale = 0.04f;
        image = texture;
        Globals.Ybuffer.Add(this);
    }

    public float GetMaxY()
    {
        return Position.Y + (Scale * image.Height) + (Id * 0.000001f);
    }

    // Animal behavior: move toward a spot within the bounding rectangle, then pick a new spot
    public override void Update()
    {
        Rectangle bounds = Home.BaseSprite.GetBounds();
        if (Vector2.Distance(Position, Destination) < bounds.Width / 8f)
        {
            Destination.X = bounds.X + bounds.Width * Globals.Rand.NextFloat(0.1f, 0.9f);
            Destination.Y = bounds.Y + bounds.Height * Globals.Rand.NextFloat(0.1f, 0.9f);
        }
        Vector2 direction = Destination - Position;
        direction.Normalize();
        Position += direction * MOVE_SPEED * Globals.Time;
    }
}

public class TileAnimal : Tile
{
    private List<Entity> Animals;
    public TileType AnimalType;

    public TileAnimal(Vector2 position, Texture2D baseTexture, Texture2D tileFeatureTexture) 
        : base(TileType.WILD_ANIMAL, position, baseTexture, tileFeatureTexture)
    {
        // TileAnimal will default to WILD_ANIMAL type, allowing hunting RawMeat.GAME
        // AnimalType will replace tile type once a Ranch is built, allowing specific goods to be farmed
        Animals = new();
        AnimalType = (TileType)Globals.Rand.Next((int)TileType.ANIMAL, (int)TileType.WILD_ANIMAL);

        Texture2D animalTexture = null;
        switch (AnimalType)
        {
            // Elephants are a special case, can only be hunted for ivory, not farmed
            case TileType.ELEPHANT: animalTexture = Sprites.Elephant; Type = TileType.ELEPHANT; break;
            
            case TileType.COW: animalTexture = Sprites.Cow; break;
            case TileType.DONKEY: animalTexture = Sprites.Donkey; break;
            case TileType.DUCK: animalTexture = Sprites.Duck; break;
            case TileType.FOWL: animalTexture = Sprites.Fowl; break;
            case TileType.GAZELLE: animalTexture = Sprites.Gazelle; break;
            case TileType.GOAT: animalTexture = Sprites.Goat; break;
            case TileType.PIG: animalTexture = Sprites.Pig; break;
            case TileType.SHEEP: animalTexture = Sprites.Sheep; break;
            case TileType.GOOSE: animalTexture = Sprites.Goose; break;
            case TileType.QUAIL: animalTexture = Sprites.Quail; break;
            default: animalTexture = Sprites.Cow; break;
        }
        
        // Add 2-5 animals to the tile
        int numAnimals = Globals.Rand.Next(2, 5);
        for (int i = 0; i < numAnimals; i++)
            Animals.Add(new Animal(this, animalTexture));
    }

    // In addition to the base tile behavior, call Update on each animal in the tile
    public override void Update()
    {
        base.Update();
        foreach (Entity animal in Animals)
            animal.Update();
    }
}