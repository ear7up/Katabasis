// PatrolTask - protect an area, attacking any enemies
// MarchTask - would look really cool to get some marching in formation going
// TrainTask - increase fighting ability
using System.Runtime.CompilerServices;

public class MilitaryTasks
{


}

public class DeploymentTask : Task
{
    // Serialized content
    public Vector2 Destination { get; set; }
    public Tile DestinationTile { get; set; }

    public DeploymentTask()
    {
        Destination = Vector2.Zero;
        SetAttributes("On deployment");
    }

    public static DeploymentTask Create(Vector2 destination)
    {
        DeploymentTask task = new();
        task.SetAttributes(destination);
        return task;
    }

    public void SetAttributes(Vector2 destination)
    {
        Destination = destination;
        DestinationTile = Globals.Model.TileMap.TileAtPos(destination);
    }

    public override TaskStatus Execute(Person p)
    {   
        Rectangle bounds = DestinationTile.BaseSprite.GetBounds();
        float distance = Vector2.Distance(p.Position, Destination);

        if (distance < bounds.Width / 2f)
            DestinationTile.Explored = true;

        if (distance < bounds.Width / 8f)
        {
            Destination = new Vector2(0f, 0f);
            Destination += new Vector2(
                bounds.X + bounds.Width * Globals.Rand.NextFloat(0.1f, 0.9f),
                bounds.Y + bounds.Height * Globals.Rand.NextFloat(0.1f, 0.9f));
        }
        Vector2 direction = Destination - p.Position;
        direction.Normalize();
        p.Position += direction * Person.MOVE_SPEED * Globals.Time;

        return Status;
    }
}