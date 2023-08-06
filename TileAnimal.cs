using System;
using System.Collections.Generic;

public enum AnimalType
{
    PIG, COW, SHEEP, DUCK, FOWL, DONKEY, GOAT, GAZELLE, BEE, ELEPHANT
}

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
    public AnimalType TileAnimalType;
    private List<Entity> Animals;

    public TileAnimal(Vector2 position, Texture2D baseTexture, Texture2D tileFeatureTexture) 
        : base(TileType.ANIMAL, position, baseTexture, tileFeatureTexture)
    {
        Animals = new();
        Array animalTypes = Enum.GetValues(typeof(AnimalType));
        TileAnimalType = (AnimalType)Globals.Rand.Next(animalTypes.Length);

        Texture2D animalTexture = null;
        switch (TileAnimalType)
        {
            //case AnimalType.BEE: animalTexture = Sprites.Bee; break;
            case AnimalType.COW: animalTexture = Sprites.Cow; break;
            case AnimalType.DONKEY: animalTexture = Sprites.Donkey; break;
            //case AnimalType.DUCK: animalTexture = Sprites.Duck; break;
            //case AnimalType.ELEPHANT: animalTexture = Sprites.Elephant; break;
            //case AnimalType.FOWL: animalTexture = Sprites.Fowl; break;
            //case AnimalType.GAZELLE: animalTexture = Sprites.Gazelle; break;
            //case AnimalType.GOAT: animalTexture = Sprites.Goat; break;
            case AnimalType.PIG: animalTexture = Sprites.Pig; break;
            //case AnimalType.SHEEP: animalTexture = Sprites.Sheep; break;
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