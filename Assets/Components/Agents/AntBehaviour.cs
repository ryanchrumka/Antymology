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
    private Vector3Int lastPosition;
    private Vector3Int lastPosition2;
    public bool isDonor = false;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        WorldManager.Instance.RegisterAnt(this);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int antPosition = Vector3Int.FloorToInt(transform.position);
        AbstractBlock blockBeneath = WorldManager.Instance.GetBlock(antPosition.x, antPosition.y - 1, antPosition.z);
        Vector3Int bestBlockPosition = Vector3Int.zero;

        AbstractBlock block = WorldManager.Instance.GetBlock(antPosition.x, antPosition.y, antPosition.z);
        if (block is AirBlock airBlock)
        {
            airBlock.DepositPheromones(100);
        }

        healthDeclineAccumulator += healthDeclineRate * Time.deltaTime;
        //Debug.Log("Current Health: " + currentHealth);
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
  
        if (currentHealth <= 0)
        {
            Debug.Log("Ant died.");
            WorldManager.Instance.UnregisterAnt(this);
            Destroy(gameObject);
            WorldManager.Instance.RemoveAnt(currentPosition);
        }





        int highestMulchBlockHeight = -50000;
        int highestGrassBlockHeight = -50000;
        int secondHighestMulchBlockHeight = -50000;
        int secondHighestGrassBlockHeight = -50000;
        Vector3Int bestMulchBlockPosition = Vector3Int.zero;
        Vector3Int bestGrassBlockPosition = Vector3Int.zero;
        Vector3Int blockAbovePosition = Vector3Int.zero;
        Vector3Int secondBestMulchBlockPosition = Vector3Int.zero;
        Vector3Int secondBestGrassBlockPosition = Vector3Int.zero;

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
                    if (nearbyBlock is MulchBlock)
                    {
                        if (blockPosition.y > highestMulchBlockHeight)
                        {
                            // Update second highest before updating highest
                            secondHighestMulchBlockHeight = highestMulchBlockHeight;
                            secondBestMulchBlockPosition = bestMulchBlockPosition;

                            highestMulchBlockHeight = blockPosition.y;
                            bestMulchBlockPosition = blockPosition;
                        }
                        else if (blockPosition.y > secondHighestMulchBlockHeight)
                        {
                            secondHighestMulchBlockHeight = blockPosition.y;
                            secondBestMulchBlockPosition = blockPosition;
                        }
                    }
                    else if (nearbyBlock is GrassBlock && currentHealth > 20)
                    {
                        if (blockPosition.y > highestGrassBlockHeight)
                        {
                            // Update second highest before updating highest
                            secondHighestGrassBlockHeight = highestGrassBlockHeight;
                            secondBestGrassBlockPosition = bestGrassBlockPosition;

                            highestGrassBlockHeight = blockPosition.y;
                            bestGrassBlockPosition = blockPosition;
                        }
                        else if (blockPosition.y > secondHighestGrassBlockHeight)
                        {
                            secondHighestGrassBlockHeight = blockPosition.y;
                            secondBestGrassBlockPosition = blockPosition;
                        }
                    }
                }
            }
        }
        bool moved = false;

        if (highestGrassBlockHeight > highestMulchBlockHeight - 2 && (blockAbovePosition != antPosition))
        {
            moved = MoveToBlock(bestGrassBlockPosition, secondBestGrassBlockPosition);
           
        }
        else if(highestMulchBlockHeight > antPosition.y)
        {
            moved = MoveToBlock(bestMulchBlockPosition, secondBestMulchBlockPosition);
            if (isDonor)
            {
                Debug.Log("2");
            }
        }
        else if (blockBeneath is MulchBlock)
        {
            // moved = MoveToBlock(bestMulchBlockPosition);
            moved = true;
            if (isDonor)
            {
                Debug.Log("3");
            }
            //Debug.Log("Ate mulch.");
            currentHealth = Mathf.Min(currentHealth + mulchHealthAmount, maxHealth);

            WorldManager.Instance.SetBlock(antPosition.x, antPosition.y - 1, antPosition.z, new AirBlock());
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);

        }else if (blockBeneath is GrassBlock)
        {
           // MoveToBlock(bestMulchBlockPosition);
            if (isDonor)
            {
                Debug.Log("4");
            }
            WorldManager.Instance.SetBlock(antPosition.x, antPosition.y - 1, antPosition.z, new AirBlock());
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);

        }
        //Move lower if not on a grass or mulch block
        else if (highestGrassBlockHeight >= (antPosition.y-2) )
        {
            moved = MoveToBlock(bestGrassBlockPosition, secondBestGrassBlockPosition);
            if (isDonor)
            {
                Debug.Log("5");
            }
        }
        else if(highestMulchBlockHeight >= (antPosition.y - 2))
        {
            if (isDonor)
            {
                Debug.Log("6");
            }
            moved = MoveToBlock(bestMulchBlockPosition, secondBestMulchBlockPosition);

        }
        if (!moved)
        {
            if (secondBestGrassBlockPosition.y >= (antPosition.y - 2))
            {
                Vector3 newPos = new Vector3(secondBestGrassBlockPosition.x, secondBestGrassBlockPosition.y + 1, secondBestGrassBlockPosition.z);
                Vector3Int newPosition = Vector3Int.FloorToInt(newPos);
                transform.position = newPosition;
            }else if(secondBestMulchBlockPosition.y >= antPosition.y - 2)
            {

                Vector3 newPos = new Vector3(secondBestMulchBlockPosition.x, secondBestMulchBlockPosition.y + 1, secondBestMulchBlockPosition.z);
                Vector3Int newPosition = Vector3Int.FloorToInt(newPos);
                transform.position = newPosition;
            }
        }

          
    }

    public bool MoveToBlock(Vector3Int blockPosition, Vector3Int secondBestBlockPosition)
    {
        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        bool moved = false;

        Vector3Int newPosition = blockPosition + Vector3Int.up; // Adjust for ground level.
        if (WorldManager.Instance.TryMoveAnt(currentPosition, newPosition, this.gameObject))
        {
            transform.position = newPosition; // Successfully moved to the best position.
            moved = true;
        }
        else
        {
            newPosition = secondBestBlockPosition + Vector3Int.up; // Adjust for ground level.
            if (WorldManager.Instance.TryMoveAnt(currentPosition, newPosition, this.gameObject))
            {
                transform.position = newPosition; // Successfully moved to the second-best position.
                moved = true;
            }
        }

        return moved;
    }



    public void DecreaseHealth(int amount)
    {
        isDonor = true;
        Debug.Log("Donor health: " + currentHealth);
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Ensure health does not go negative.
    }
}


