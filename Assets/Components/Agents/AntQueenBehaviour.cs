using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Antymology.Terrain;

public class AntQueenBehaviour : MonoBehaviour
{
    public int maxHealth = 100; // The maximum health the ant can have.
    public int health; // The current health of the ant.
    public int healthDeclineRate = 1; // The rate at which health declines.
    public int mulchHealthAmount = 3; //The amount of health for each mulch block
    public float healthDeclineAccumulator = 0f;
    private bool searchingForAnts = false; // State to control behavior.

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!searchingForAnts)
        {
            // Keep searching for the lowest point not above mulch until health is too low.
            if (health > health / 3) // Ensure she has more than 1/3 health left to place a block.
            {
                TryPlaceBlock();
            }
            else
            {
                searchingForAnts = true; // Switch behavior to search for ants.
            }
        }
        else
        {
            // Implement roaming behavior to find other ants.
            RoamForAnts();
        }
    }

    void TryPlaceBlock()
    {
        Vector3Int lowestPoint = FindLowestPoint();
        if (lowestPoint != Vector3Int.zero) // Assuming FindLowestPoint returns Vector3Int.zero if no suitable point found.
        {
            PlaceBlockAt(lowestPoint);
            health -= health / 3; // Deduct 1/3 of health for placing a block.
        }
    }

    Vector3Int FindLowestPoint()
    {
        
        return Vector3Int.zero;
    }

    void PlaceBlockAt(Vector3Int point)
    {
        WorldManager.Instance.SetBlock(point.x, point.y, point.z, new NestBlock());
        Debug.Log("Placed block at: " + point);
    }

    void RoamForAnts()
    {
        // Placeholder for roaming logic.
        // In the future, implement logic to follow pheromones or move randomly until other ants are found.
    }
}
