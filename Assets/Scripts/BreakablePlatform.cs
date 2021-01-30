using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class BreakablePlatform : MonoBehaviour
{
    public PlayerTracker playerTracker;
    public int maxFatness = 1;
    public float breakTimer = 1;

    void Update()
    {
        var playerInZone = playerTracker.triggered.FirstOrDefault();
        if (
            playerInZone != null 
            && playerInZone.Grounded 
            && playerInZone.FatnessLevel > maxFatness
        ) {
            breakTimer -= Time.deltaTime;
            if (breakTimer <= 0)
            {
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
