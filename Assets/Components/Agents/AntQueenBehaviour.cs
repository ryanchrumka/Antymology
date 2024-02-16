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
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!searchingForAnts)
        {
            // Keep searching for the lowest point not above mulch until health is too low.
            if (health > 50) // Ensure she has more than 1/2 health left to place a block and find a new position.
            {
                Debug.Log("try place");
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
        Debug.Log("find lowest");
        Vector3Int lowestPoint = FindLowestPoint();
        if (lowestPoint != Vector3Int.zero) 
        {
            MoveToBlock(lowestPoint);
            PlaceBlockAt(lowestPoint);
            health -= 33; // Deduct 1/3 of health for placing a block.
           
            
        }
        else
        {
            if (true)
            {
                PlaceBlockAt(lowestPoint);
                health -= 33; // Deduct 1/3 of health for placing a block.
            }
        }
    }

    Vector3Int FindLowestPoint()
    {
        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        Vector3Int lowestPoint = currentPosition;
        int searchRadius = 3;
        int lowestY = int.MaxValue;

        // Check immediate surroundings, defined by a 3x3 area centered on the ant.
        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int z = -searchRadius; z <= searchRadius; z++)
            {
                Vector3Int checkPosition = new Vector3Int(currentPosition.x + x, currentPosition.y - 1, currentPosition.z + z);
                while (checkPosition.y > 0)
                {
                    AbstractBlock blockBelow = WorldManager.Instance.GetBlock(checkPosition.x, checkPosition.y, checkPosition.z);
                    AbstractBlock potentialMove = WorldManager.Instance.GetBlock(checkPosition.x, checkPosition.y + 1, checkPosition.z);
                    Vector3Int potential = new Vector3Int(checkPosition.x, checkPosition.y + 1, checkPosition.z);
                    // If the block below is not Air or Mulch, and is lower than the current lowest, update lowestPoint.
                    if (!(blockBelow is AirBlock) && checkPosition.y < lowestY && potentialMove is AirBlock)
                    {
                        lowestPoint = potential;
                        lowestY = checkPosition.y;
                        break; // Break the while loop once a solid ground is found.
                    }

                    // Move one block down.
                    checkPosition.y -= 1;
                }
            }
        }

        // If the lowest point found is still the initial position, no move is needed.
        if (lowestPoint == currentPosition)
        {
            Debug.Log("find lowest = false");
            return Vector3Int.zero; // Indicates no lower point was found.
        }
        Debug.Log("find lowest = true");
        return lowestPoint;
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


   private void MoveToBlock(Vector3Int blockPosition)
    {
        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        Vector3 newPos = new Vector3(blockPosition.x, blockPosition.y + 1, blockPosition.z);
        Vector3Int newPosition = Vector3Int.FloorToInt(newPos);
        transform.position = newPos; // Successfully moved
        
    }

    private bool MoveUp()
    {
        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        Vector3 newPos = new Vector3(currentPosition.x, currentPosition.y + 1, currentPosition.z);
        Vector3Int newPosition = Vector3Int.FloorToInt(newPos);
        AbstractBlock potentialMove = WorldManager.Instance.GetBlock(currentPosition.x, currentPosition.y + 1, currentPosition.z);
        if (potentialMove is AirBlock)
        {
            transform.position = newPos; // Successfully moved
            return true;
        }
        return false;

    }

}
