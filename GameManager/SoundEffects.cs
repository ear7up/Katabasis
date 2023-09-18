using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

public class SoundEffects
{
    public static SoundEffect StoneButtonPress;
    public static SoundEffect BuildingSound;
    public static SoundEffect MoneySound;
    public static SoundEffect DiceSound;
    public static List<SoundEffectInstance> Looping;

    public static void Load(ContentManager content)
    {
        Looping = new();
        StoneButtonPress = content.Load<SoundEffect>("audio/stoneButtonPress");
        BuildingSound = content.Load<SoundEffect>("audio/buildingSound");
        MoneySound = content.Load<SoundEffect>("audio/moneySound");
        DiceSound = content.Load<SoundEffect>("audio/dice");
    }

    public static void Play(SoundEffect effect)
    {
        effect.Play();
    }

    public static int Loop(SoundEffect effect)
    {
        SoundEffectInstance instance = effect.CreateInstance();
        instance.IsLooped = true;
        instance.Play();

        Looping.Add(instance);
        return Looping.Count - 1;
    }

    public static void StopLooping(int index)
    {
        if (index < 0 || index >= Looping.Count)
            return;
        Looping[index].Stop();
        Looping.RemoveAt(index);
    }

    public static void VolumeUp()
    {
        SoundEffect.MasterVolume += 0.1f;
    }

    public static void VolumeDown()
    {
        SoundEffect.MasterVolume -= 0.1f;
    }
}