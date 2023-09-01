using System;
using System.Collections.Generic;

public class Animal : Entity, Drawable
{
    public static int IdCounter = 0;
    public const float MOVE_SPEED = 10f;

    private int Id { get; set; }
    public Tile Home { get; set; }
    public Vector2 Destination { get; set; }

    public Animal() : base()
    {
        Id = IdCounter++;
        Globals.Ybuffer.Add(this);
    }

    public static Animal Create(Tile home, SpriteTexture spriteTexture)
    {
        Animal animal = new();
        animal.Home = home;
        animal.Position = home.GetPosition();
        animal.Destination = animal.Position;
        animal.SetNewSpriteTexture(spriteTexture);
        animal.Scale = 0.04f;
        return animal;
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
            Destination = new Vector2(0f, 0f);
            Destination += new Vector2(
                bounds.X + bounds.Width * Globals.Rand.NextFloat(0.1f, 0.9f),
                bounds.Y + bounds.Height * Globals.Rand.NextFloat(0.1f, 0.9f));
        }
        Vector2 direction = Destination - Position;
        direction.Normalize();
        Position += direction * MOVE_SPEED * Globals.Time;
    }
}

public class TileAnimal : Tile
{
    public List<Animal> Animals { get; set; }
    public TileType AnimalType { get; set; }

    public TileAnimal() : base()
    {
        Discriminator = TileDiscriminator.TileAnimal;
        Animals = new();
    }

    public static TileAnimal Create(
        Vector2 position, 
        SpriteTexture baseTexture)
    {
        TileAnimal tile = new();
        tile.SetAttributes(TileType.WILD_ANIMAL, position, baseTexture);
        return tile;
    }

    public override void SetAttributes(
        TileType type, 
        Vector2 position, 
        SpriteTexture baseTexture)
    {
        // TileAnimal will default to WILD_ANIMAL type, allowing hunting RawMeat.GAME
        base.SetAttributes(type, position, baseTexture);

        // AnimalType will replace tile type once a Ranch is built, allowing specific goods to be farmed
        AnimalType = (TileType)Globals.Rand.Next((int)TileType.ANIMAL + 1, (int)TileType.WILD_ANIMAL);

        SpriteTexture animalTexture = null;
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
            //case TileType.GIRAFFE: animalTexture = Sprites.Giraffe; break;
            default: animalTexture = Sprites.Cow; break;
        }
        
        // Add 2-5 animals to the tile
        int numAnimals = Globals.Rand.Next(2, 5);
        for (int i = 0; i < numAnimals; i++)
            Animals.Add(Animal.Create(this, animalTexture));

        if (Config.ShowFog)
            foreach (Animal animal in Animals)
                animal.Hidden = true;
    }

    // In addition to the base tile behavior, call Update on each animal in the tile
    public override void Update()
    {
        base.Update();
        foreach (Animal animal in Animals)
        {
            if (Config.ShowFog && !Explored)
                animal.Hidden = true;
            animal.Update();
        }
    }

    public override void Explore()
    {
        base.Explore();
        foreach (Animal animal in Animals)
            animal.Hidden = false;
    }

    public override string GetResource()
    {
        return Globals.Title(AnimalType.ToString());
    }
}