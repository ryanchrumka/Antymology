using System.Collections;
using System.Collections.Generic;
using Antymology.Terrain;
using UnityEngine;

public class AntBehaviour : MonoBehaviour
{
    public int maxHealth = 100; // The maximum health the ant can have.
    public int currentHealth; // The current health of the ant.
    public int healthDeclineRate = 1; // The rate at which health declines.
    public int mulchHealthAmount = 3; //The amount of health for each mulch block
    public float healthDeclineAccumulator = 0f;
    public int scanRadius = 3; // How far the ant can "see" Mulch block


    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

        healthDeclineAccumulator += healthDeclineRate * Time.deltaTime;
        //Debug.Log("Current Health: " + currentHealth);
        int decreaseAmount = Mathf.FloorToInt(healthDeclineAccumulator);
        currentHealth -= decreaseAmount; // Apply the accumulated decrease
        healthDeclineAccumulator -= decreaseAmount; // Subtract the applied amount from the accumulator
        currentHealth = Mathf.Max(currentHealth, 0); // Ensure health doesn't go below 0

        Vector3Int currentPosition = Vector3Int.FloorToInt(transform.position);
        WorldManager.Instance.AddAnt(currentPosition, gameObject);

        if (currentHealth <= 0)
        {
            Debug.Log("Ant died.");
            Destroy(gameObject);
            WorldManager.Instance.RemoveAnt(currentPosition);
        }

        Vector3Int antPosition = Vector3Int.FloorToInt(transform.position);
        AbstractBlock blockBeneath = WorldManager.Instance.GetBlock(antPosition.x, antPosition.y - 1, antPosition.z);
        Vector3Int bestMulchBlockPosition = Vector3Int.zero;



        //Scan for mulch and move there if the mulch exist

        int highestMulchBlockHeight = int.MinValue;
        for (int a = -scanRadius; a <= scanRadius; a++)
        {
            for (int b = -2; b <= 2; b++)
            {
                for (int c = -scanRadius; c <= scanRadius; c++)
                {
                    AbstractBlock nearbyBlock = WorldManager.Instance.GetBlock(antPosition.x + a, antPosition.y + b, antPosition.z + c);

                    // Ensures ant doesn't go inside of a block
                    AbstractBlock blockAbove = WorldManager.Instance.GetBlock(antPosition.x + a, antPosition.y + b + 1, antPosition.z + c);
                    if (nearbyBlock is MulchBlock && blockAbove is AirBlock)
                    {
                        if ((antPosition.y + b )> highestMulchBlockHeight)
                        {
                            highestMulchBlockHeight = (antPosition.y + b);
                            bestMulchBlockPosition = new Vector3Int(antPosition.x + a, antPosition.y + b, antPosition.z + c);
                        }
                        
                    }
                }
            }
        }
        if (highestMulchBlockHeight > antPosition.y)
        {
            Vector3 newPos = new Vector3(bestMulchBlockPosition.x, bestMulchBlockPosition.y + 1, bestMulchBlockPosition.z);
            Vector3Int newPosition = Vector3Int.FloorToInt(newPos);
            currentPosition = Vector3Int.FloorToInt(transform.position);

            if (WorldManager.Instance.TryMoveAnt(currentPosition, newPosition, gameObject))
            {
                // Move is allowed, update ant's position
                transform.position = newPosition; // Convert back to world position if necessary
            }

        }

        else if (blockBeneath is MulchBlock)
        {
            Debug.Log("Ate mulch.");
            currentHealth = Mathf.Min(currentHealth + mulchHealthAmount, maxHealth);
            WorldManager.Instance.SetBlock(antPosition.x, antPosition.y - 1, antPosition.z, new AirBlock());
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        }
        else if (highestMulchBlockHeight >= (antPosition.y-2) )
        {
            Vector3 newPos = new Vector3(bestMulchBlockPosition.x, bestMulchBlockPosition.y + 1, bestMulchBlockPosition.z);
            Vector3Int newPosition = Vector3Int.FloorToInt(newPos);
            currentPosition = Vector3Int.FloorToInt(transform.position);

            if (WorldManager.Instance.TryMoveAnt(currentPosition, newPosition, gameObject))
            {
                // Move is allowed, update ant's position
                transform.position = newPosition; // Convert back to world position if necessary
            }
        }
        else
        {
            //Move behavior

        }

            
    }
}
