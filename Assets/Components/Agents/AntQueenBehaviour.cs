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

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (health >= 34)
        {
            TryPlaceBlock();

        }
        else
        {
            // Implement roaming behavior to find other ants.
            MoveTowardsNearestHighPheromone();
            CheckForNearbyAntsAndReceiveHealth();
        }
    }

    void TryPlaceBlock()
    {
        Vector3Int lowestPoint = FindLowestPoint();
        if (!lowestPoint.Equals(Vector3Int.FloorToInt(transform.position)))
        {
            MoveToBlock(lowestPoint);
            PlaceBlockAt(lowestPoint);
            health -= 33; // Deduct 1/3 of health for placing a block.


        }
        else
        {

            Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
            PlaceBlockAt(currentPosition);
            health -= 33; // Deduct 1/3 of health for placing a block.
            MoveUp();
        }
    }

    Vector3Int FindLowestPoint()
    {
        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        Vector3Int lowestPoint = currentPosition;
        int searchRadius = 3;
        int lowestY = currentPosition.y;

        // Check immediate surroundings, defined by a 3x3 area centered on the ant.
        // Check immediate surroundings, defined by a 3x3 area centered on the ant.
        for (int a = -searchRadius; a <= searchRadius; a++)
        {
            for (int b = 0; b <= searchRadius; b++)
            {
                for (int c = -searchRadius; c <= searchRadius; c++)
                {

                    Vector3Int checkPosition = new Vector3Int(currentPosition.x + a, currentPosition.y - b, currentPosition.z + c);
                    AbstractBlock blockBelow = WorldManager.Instance.GetBlock(checkPosition.x, checkPosition.y, checkPosition.z);
                    AbstractBlock potentialMove = WorldManager.Instance.GetBlock(checkPosition.x, checkPosition.y + 1, checkPosition.z);
                    Vector3Int potential = new Vector3Int(checkPosition.x, checkPosition.y + 1, checkPosition.z);
                    // If the block below is not Air and is lower than the current lowest, update lowestPoint.
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
        Debug.Log("lowest y: " + lowestPoint.y);
        Debug.Log("current pos: " + currentPosition.y);
        if (lowestPoint.y.Equals(currentPosition.y - 1))
        {
            return currentPosition; // Indicates no lower point was found.
        }
        return lowestPoint;
    }

    void PlaceBlockAt(Vector3Int point)
    {
        WorldManager.Instance.SetBlock(point.x, point.y, point.z, new NestBlock());
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

    private void MoveDown()
    {
        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        Vector3 newPos = new Vector3(currentPosition.x, currentPosition.y - 1, currentPosition.z);
        Vector3Int newPosition = Vector3Int.FloorToInt(newPos);
        AbstractBlock potentialMove = WorldManager.Instance.GetBlock(currentPosition.x, currentPosition.y + 1, currentPosition.z);
        transform.position = newPos; // Successfully moved

    }


    void MoveTowardsNearestHighPheromone()
    {
        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        float highestPheromoneLevel = 0;
        Vector3Int closestHighPheromonePosition = currentPosition; // Initialize with current position.
        float closestDistance = 200; // Initialize with a very high value.

        int searchRadius = 20; // Define search radius.
        int searchHeight = 8; // Define search height range above and below the current position.

        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int y = -searchHeight; y <= searchHeight; y++)
            {
                for (int z = -searchRadius; z <= searchRadius; z++)
                {
                    Vector3Int checkPosition = currentPosition + new Vector3Int(x, y, z);
                    AbstractBlock block = WorldManager.Instance.GetBlock(checkPosition.x, checkPosition.y, checkPosition.z);

                    if (block is AirBlock airBlock)
                    {
                        float pheromoneLevel = airBlock.getPheromoneLevel(); // Assuming a method to get pheromone level.
                        float distance = Vector3.Distance(currentPosition, checkPosition); // Use Vector3.Distance for 3D distance calculation.
                                                                                           // Check if this block has a higher pheromone level and is closer than previously recorded blocks.
                        if (pheromoneLevel > highestPheromoneLevel || (pheromoneLevel == highestPheromoneLevel && distance < closestDistance))
                        {
                            highestPheromoneLevel = pheromoneLevel;
                            closestHighPheromonePosition = checkPosition;
                            closestDistance = distance;
                        }
                    }
                }
            }
        }

        // Move 1 step towards the closest high pheromone concentration block if it's not the current position.
        if (closestHighPheromonePosition != currentPosition)
        {
            // Determine the step to move in each direction.
            Vector3Int directionToMove = closestHighPheromonePosition - currentPosition;
            Vector3Int moveStep = new Vector3Int((int)Mathf.Sign(directionToMove.x), (int)Mathf.Sign(directionToMove.y), (int)Mathf.Sign(directionToMove.z));

            // Optional: Validate the move (e.g., check for obstacles or terrain height differences).
            Vector3Int newPosition = currentPosition + moveStep;

            transform.position += new Vector3(moveStep.x, moveStep.y, moveStep.z);
            Vector3Int currentPos = Vector3Int.FloorToInt(transform.position);
            AbstractBlock blockBelow = WorldManager.Instance.GetBlock(currentPos.x, currentPos.y - 1, currentPos.z);
            if (blockBelow is AirBlock) ;
            if (WorldManager.Instance.TryMoveAnt(currentPosition, newPosition))
            {
                MoveDown();
            }

        }
    }

    void CheckForNearbyAntsAndReceiveHealth()
    {
        foreach (var ant in WorldManager.Instance.allAnts)
        {
            if (ant != this && Vector3.Distance(transform.position, ant.transform.position) <= 2)
            {
                TransferHealthFromAnt(ant);
                break; // Assuming health from one ant at a time.
            }
        }
    }

    void TransferHealthFromAnt(AntBehaviour donorAnt)
    {
        // Check if the donor ant has enough health to give.
        if (donorAnt.currentHealth > 20)
        {
            int healthTransferAmount = Mathf.Min(66, donorAnt.currentHealth - 20);
            healthTransferAmount = Mathf.Min(100 - health, healthTransferAmount);
            donorAnt.DecreaseHealth(healthTransferAmount); // Decrease donor's health.
            health += healthTransferAmount; // Increase queen's health.

        }

    }



}
