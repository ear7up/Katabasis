using System;
using Katabasis;

public class SaveMenu : CloseablePanel
{
    public static SaveMenu Instance = null;

    public SaveMenu(SpriteTexture texture) : base(texture)
    {
        SetMargin(top: 15, left: 15);
        Add(BuildButton(SaveSlot.AUTO, "AutoSave"));
        Add(BuildButton(SaveSlot.QUICK, "QuickSave"));
        Add(BuildButton(SaveSlot.SLOT1, "Slot 1"));
        Add(BuildButton(SaveSlot.SLOT2, "Slot 2"));
        Add(BuildButton(SaveSlot.SLOT3, "Slot 3"));
        Instance = this;
    }

    public static OverlapLayout BuildButton(SaveSlot slot, string slotText)
    {
        OverlapLayout olayout = new();

        UIElement button = new(Sprites.MenuButton, 1f, OverwriteSave);
        button.HoverImage = Sprite.Create(Sprites.MenuButtonHover, Vector2.Zero);
        button.UserData = slot;

        //slotText += "\n" + KatabasisGame.Instance.GetLastModified(slot);
        TextSprite buttonText = new(Sprites.Font, Color.White, Color.Black, text: slotText);
        buttonText.SetPadding(left: 35, top: 20);
        buttonText.ScaleDown(0.40f);

        olayout.Add(button);
        olayout.Add(buttonText);
        olayout.SetMargin(bottom: 5, left: 1);
        
        return olayout;
    }

    public static void OverwriteSave(Object clicked = null)
    {
        UIElement element = (UIElement)clicked;
        Katabasis.KatabasisGame.Instance.Save((SaveSlot)element.UserData);
        Instance.ClosePanel();
    }
}