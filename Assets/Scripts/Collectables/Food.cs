using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Food : Collectable
{

    public float initialVolume = 5;
    public float caloriesPerVolume = 1;

    public float remainingVolume
    {
        get;
        private set;
    }

    private void Awake()
    {
        remainingVolume = initialVolume;
    }

    /// Returns calories consumed
    public float eat(float volumeToEat)
    {
        float actualVolumeToEat = Mathf.Min(volumeToEat, remainingVolume);
        remainingVolume -= actualVolumeToEat;

        if (remainingVolume == 0)
        {
            Destroy(this.gameObject);
        }

        return actualVolumeToEat * caloriesPerVolume;
    }

    /// Returns calories consumed
    public float eatAll()
    {
        Destroy(this.gameObject);
        return remainingVolume * caloriesPerVolume;
    }


}
