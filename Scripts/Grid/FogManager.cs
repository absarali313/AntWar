using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)] // Ensure this runs BEFORE Ants update their vision to prevent flickering!
public class FogManager : MonoBehaviour
{
    // Track only tiles that are currently Visible, instead of scanning ALL tiles every frame
    private static HashSet<Tile> visibleTiles = new HashSet<Tile>();

    public static void MarkVisible(Tile t)
    {
        visibleTiles.Add(t);
    }

    void Update()
    {
        // Only iterate the small set of currently-visible tiles, not every tile in the world!
        foreach (Tile t in visibleTiles)
        {
            if (t.fogState == FogState.Visible)
            {
                t.fogState = FogState.Explored;
                t.SetExplored();
                t.UpdateFogVisual();
            }
        }

        visibleTiles.Clear();
    }
}
