using System;
using System.Collections.Generic;

public class BuildingPlacer
{
    public const float SCALE_CONSTANT = 0.1f;

    public Building _editBuilding;

    public BuildingPlacer()
    {

    }

    public void Update()
    {
        if (_editBuilding == null)
            return;
    
        // Make the currently editing building follow the mouse pointer
        _editBuilding.Sprite.Position = InputManager.MousePos;
        if (_editBuilding.ConstructionSprite != null)
            _editBuilding.ConstructionSprite.Position = InputManager.MousePos;

        Tile location = Globals.Model.TileMap.TileAtPos(InputManager.MousePos);

        // Use color to indicate if the placement is allowable
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
        if (InputManager.Mode == InputManager.BUILD_MODE && InputManager.ScrollValue > 0)
            _editBuilding.Sprite.ScaleUp(SCALE_CONSTANT);
        else if (InputManager.Mode == InputManager.BUILD_MODE && InputManager.ScrollValue < 0)
            _editBuilding.Sprite.ScaleDown(SCALE_CONSTANT);
        else if (InputManager.Mode != InputManager.BUILD_MODE)
            _editBuilding = null;
    }

    public void CreateEditBuilding(BuildingType buildingType, BuildingSubType subType = BuildingSubType.NONE)
    {
        Building b = Building.Random(buildingType, subType, temporary: true);
        b.Sprite.Position = InputManager.MousePos;
        _editBuilding = b;
        _editBuilding.Sprite.SpriteColor = new Color(Color.LightBlue, 0.3f);
    }

    // Keep building list sorted by y order
    public void AddBuilding(Building b)
    {
        Tile t = b.Location;
        if (t == null)
            t = Globals.Model.TileMap.TileAtPos(b.Sprite.Position);

        List<Goods> materials = b.GetMaterials();

        float materialCost = Building.MaterialCost(materials);
        float laborCost = Building.LaborCost(b.Type, b.SubType);

        // TODO: Alert the user they could not afford the building
        if (!Config.InstantBuilding && Globals.Model.Player1.Person.Money < materialCost + laborCost)
        {
            Console.WriteLine($"Could not afford building (cost = ${materialCost + laborCost})");
            return;
        }

        if (t == null)
        {
            Console.WriteLine("Failed to find tile at position " + b.Sprite.Position.ToString());
        }
        else if (Building.ConfirmBuilding(b, t))
        {
            // Set buliding to under construction and wait for builders
            if (!Config.InstantBuilding)
            {
                // Labor costs paid up front, material costs may vary when MarketOrder is fulfilled
                Globals.Model.Player1.Person.Money -= laborCost;
                OrderMaterials(materials);

                ConstructionRequest req = ConstructionRequest.Create(t, b, materials, laborCost);
                Globals.Model.ConstructionQueue.AddRequest(req);

                // TODO: Signal the building to switch to the under construction sprite
            }
        }
        else
            Console.WriteLine("Failed to add building at tile " + t.ToString());
    }
    
    public void OrderMaterials(List<Goods> materials)
    {
        foreach (Goods goods in materials)
        {
            MarketOrder buyOrder = MarketOrder.Create(Globals.Model.Player1.Person, true, new Goods(goods));
            Globals.Model.Market.PlaceBuyOrder(buyOrder);
        }
    }

    public void ClearEditBuilding()
    {
        _editBuilding = null;
    }

    public void DrawUI()
    {
        // If the user is placing a building, draw that temporary sprite
        if (_editBuilding != null)
        {
            _editBuilding.Draw();
        }
    }
}