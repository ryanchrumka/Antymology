using System.Collections;
using System.Collections.Generic;
using Antymology.Terrain;
using UnityEngine;

public class AntBehaviour : MonoBehaviour
{
    public int maxHealth = 100; // The maximum health the ant can have.
    public int currentHealth; // The current health of the ant.
    public int healthDeclineRate = 1; // The rate at which health declines.
    public int mulchHealthAmount = 3; //The amount of health given for each mulch block
    public float healthDeclineAccumulator = 0f;
    public int scanRadius = 2; // How far the ant can "see" Mulch block
    public bool stuck;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        WorldManager.Instance.RegisterAnt(this);
        stuck = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int antPosition = Vector3Int.FloorToInt(transform.position);
        AbstractBlock blockBeneath = WorldManager.Instance.GetBlock(antPosition.x, antPosition.y - 1, antPosition.z);
        Vector3Int bestBlockPosition = Vector3Int.zero;

        // Ant deposits 20 pheromones on their current block
        AbstractBlock block = WorldManager.Instance.GetBlock(antPosition.x, antPosition.y, antPosition.z);
        if (block is AirBlock airBlock)
        {
            airBlock.DepositPheromones(20);
        }

        healthDeclineAccumulator += healthDeclineRate * Time.deltaTime;
        int decreaseAmount = Mathf.FloorToInt(healthDeclineAccumulator);
        currentHealth -= decreaseAmount; // Apply the accumulated decrease
        healthDeclineAccumulator -= decreaseAmount; // Subtract the applied amount from the accumulator
        currentHealth = Mathf.Max(currentHealth, 0); // Ensure health doesn't go below 0

        //Repeats the subtract health again if on an acid block so that overall decrease is 2x
        if (blockBeneath is AcidicBlock)
        {
            healthDeclineAccumulator += healthDeclineRate * Time.deltaTime;

            decreaseAmount = Mathf.FloorToInt(healthDeclineAccumulator);
            currentHealth -= decreaseAmount; // Apply the accumulated decrease
            healthDeclineAccumulator -= decreaseAmount; // Subtract the applied amount from the accumulator
            currentHealth = Mathf.Max(currentHealth, 0); // Ensure health doesn't go below 0

        }

        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        WorldManager.Instance.AddAnt(currentPosition);

        // If the ant's health drops below zero it is removed from the map
        if (currentHealth <= 0)
        {
            WorldManager.Instance.UnregisterAnt(this);
            Destroy(gameObject);
            WorldManager.Instance.RemoveAnt(currentPosition);
        }

        scan();

    }

    // Scans the environment within the specified scanRadius for mulch and grass blocks.
    // Determines the best move based on the highest block found and executes the movement if possible.
    void scan()
    {
        Vector3Int antPosition = Vector3Int.FloorToInt(transform.position);
        AbstractBlock blockBeneath = WorldManager.Instance.GetBlock(antPosition.x, antPosition.y - 1, antPosition.z);
        Vector3Int bestBlockPosition = Vector3Int.zero;

        int highestMulchBlockHeight = -50000;
        int highestGrassBlockHeight = -50000;
        Vector3Int bestMulchBlockPosition = Vector3Int.zero;
        Vector3Int bestGrassBlockPosition = Vector3Int.zero;
        Vector3Int blockAbovePosition = Vector3Int.zero;

        //Scans x, y, z to find the highest mulch block and highest grass block
        for (int a = -scanRadius; a <= scanRadius; a++)
        {
            for (int b = -2; b <= 2; b++)
            {
                for (int c = -scanRadius; c <= scanRadius; c++)
                {
                    Vector3Int blockPosition = new Vector3Int(antPosition.x + a, antPosition.y + b, antPosition.z + c);
                    AbstractBlock nearbyBlock = WorldManager.Instance.GetBlock(blockPosition.x, blockPosition.y, blockPosition.z);

                    // Ensures ant doesn't go inside of a block
                    AbstractBlock blockAbove = WorldManager.Instance.GetBlock(blockPosition.x, blockPosition.y + 1, blockPosition.z);
                    if (blockAbove is AirBlock)
                    {
                        // Selects best mulch block if it is the highest one
                        if (nearbyBlock is MulchBlock && (blockPosition.y > highestMulchBlockHeight))
                        {
                            highestMulchBlockHeight = blockPosition.y;
                            bestMulchBlockPosition = blockPosition;
                        }
                        // Selects best grass block if it is the highest one
                        else if (nearbyBlock is GrassBlock && currentHealth > 20 && (blockPosition.y >= highestGrassBlockHeight))
                        {
                            highestGrassBlockHeight = blockPosition.y;
                            bestGrassBlockPosition = blockPosition;
                            blockAbovePosition = new Vector3Int(blockPosition.x, blockPosition.y + 1, blockPosition.z);
                        }
                    }
                }
            }
        }

        // If the highest grass block is within 1 height of the highest mulch block, the ant moves to it if it has health
        // This is so that ants with plenty of health prioritize digging over eating if it has enough health
        if (highestGrassBlockHeight > highestMulchBlockHeight - 2 && (blockAbovePosition != antPosition) && currentHealth > 20)
        {
            MoveToBlock(bestGrassBlockPosition);
        }
        // Moves to grass if it is the highest
        else if (highestMulchBlockHeight > antPosition.y)
        {
            MoveToBlock(bestMulchBlockPosition);
        }
        // If no higher blocks, but block below is mulch, eat it
        else if (blockBeneath is MulchBlock)
        {
            currentHealth = Mathf.Min(currentHealth + mulchHealthAmount, maxHealth);

            WorldManager.Instance.SetBlock(antPosition.x, antPosition.y - 1, antPosition.z, new AirBlock());
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);

        }
        // If no higher blocks, but block below is grass, dig it
        else if (blockBeneath is GrassBlock)
        {
            WorldManager.Instance.SetBlock(antPosition.x, antPosition.y - 1, antPosition.z, new AirBlock());
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);

        }
        // If there aren't higher blocks, but a grass block within 2 below, move to it
        else if (highestGrassBlockHeight >= (antPosition.y - 2))
        {
            MoveToBlock(bestGrassBlockPosition);
        }
        // If there aren't higher blocks, but a mulch block within 2 below, move to it
        else if (highestMulchBlockHeight >= (antPosition.y - 2))
        {
            MoveToBlock(bestMulchBlockPosition);

        }


    }

    // Makes sure that the ant can move to the desired block.
    // If the ant is stuck (from a bug I can't fix), force it through after waiting
    void MoveToBlock(Vector3Int blockPosition)
    {
        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        Vector3 newPos = new Vector3(blockPosition.x, blockPosition.y + 1, blockPosition.z);
        Vector3Int newPosition = Vector3Int.FloorToInt(newPos);

        // TryMoveAnt ensures there is no other ant in the position that the ant want to move
        if (WorldManager.Instance.TryMoveAnt(currentPosition, newPosition))
        {

            transform.position = newPos; // Successfully moved
        }
        // If the ant is stuck, force it to the next position, 
        // as it is likely unoccupied as the other ant would have moved
        else if (stuck)
        {
            transform.position = newPos;
        }
        // If the ant can't move, it is stuck
        else
        {
            stuck = true;
        }
    }

    // Called when in the prescence of a queen ant requiring a health donation
    public void DecreaseHealth(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Ensure health does not go negative.
    }
}


