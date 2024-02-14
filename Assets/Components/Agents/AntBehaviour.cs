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


    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int antPosition = Vector3Int.FloorToInt(transform.position);
        AbstractBlock blockBeneath = WorldManager.Instance.GetBlock(antPosition.x, antPosition.y - 1, antPosition.z);
        Vector3Int bestBlockPosition = Vector3Int.zero;


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
            Debug.Log("Current Health: " + currentHealth);
            decreaseAmount = Mathf.FloorToInt(healthDeclineAccumulator);
            currentHealth -= decreaseAmount; // Apply the accumulated decrease
            healthDeclineAccumulator -= decreaseAmount; // Subtract the applied amount from the accumulator
            currentHealth = Mathf.Max(currentHealth, 0); // Ensure health doesn't go below 0

        }

        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        WorldManager.Instance.AddAnt(currentPosition);

        if (currentHealth <= 0)
        {
            Debug.Log("Ant died.");
            Destroy(gameObject);
            WorldManager.Instance.RemoveAnt(currentPosition);
        }





        int highestMulchBlockHeight = int.MinValue;
        int highestGrassBlockHeight = int.MinValue;
        Vector3Int bestMulchBlockPosition = Vector3Int.zero;
        Vector3Int bestGrassBlockPosition = Vector3Int.zero;

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
                    { // Ensures the ant doesn't go inside of a block
                        if (nearbyBlock is MulchBlock && (blockPosition.y > highestMulchBlockHeight))
                        {
                            highestMulchBlockHeight = blockPosition.y;
                            bestMulchBlockPosition = blockPosition;
                        }
                        else if (nearbyBlock is GrassBlock && currentHealth > 20 && (blockPosition.y > highestGrassBlockHeight))
                        {
                            highestGrassBlockHeight = blockPosition.y;
                            bestGrassBlockPosition = blockPosition;
                        }
                    }
                }
            }
        }
        if (highestGrassBlockHeight >= highestMulchBlockHeight - 1 && (highestGrassBlockHeight >= (antPosition.y - 2)))
        {
            MoveToBlock(bestGrassBlockPosition);
            Debug.Log("1");
        }
        else if(highestMulchBlockHeight > antPosition.y)
        {
            MoveToBlock(bestMulchBlockPosition);
            Debug.Log("2");
        }
        else if (blockBeneath is MulchBlock)
        {
            Debug.Log("3");
            //Debug.Log("Ate mulch.");
            currentHealth = Mathf.Min(currentHealth + mulchHealthAmount, maxHealth);

            WorldManager.Instance.SetBlock(antPosition.x, antPosition.y - 1, antPosition.z, new AirBlock());
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);

        }else if (blockBeneath is GrassBlock)
        {
            Debug.Log("4");
            WorldManager.Instance.SetBlock(antPosition.x, antPosition.y - 1, antPosition.z, new AirBlock());
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);

        }
        //Move lower if not on a grass or mulch block
        else if (highestGrassBlockHeight >= (antPosition.y-2) )
        {
            Debug.Log("5");
            MoveToBlock(bestGrassBlockPosition);
        }
        else if(highestMulchBlockHeight >= (antPosition.y - 2))
        {
            Debug.Log("6");
            MoveToBlock(bestMulchBlockPosition);

        }
        else
        {
            Debug.Log("Ant stuck.");
        }


          
    }

    void MoveToBlock(Vector3Int blockPosition)
    {
        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        Vector3 newPos = new Vector3(blockPosition.x, blockPosition.y + 1, blockPosition.z);
        Vector3Int newPosition = Vector3Int.FloorToInt(newPos);
        Debug.Log("a");
        if (WorldManager.Instance.TryMoveAnt(currentPosition, newPosition))
        {
            Debug.Log("b");
            transform.position = newPos; // Successfully moved
        }
        else 
        {
          
        }
    }

}


