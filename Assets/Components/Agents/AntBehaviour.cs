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


        if (currentHealth <= 0)
        {
            Debug.Log("Ant died.");
            Destroy(gameObject);
        }

        Vector3Int antPosition = Vector3Int.FloorToInt(transform.position);
        AbstractBlock blockBeneath = WorldManager.Instance.GetBlock(antPosition.x, antPosition.y - 1, antPosition.z);

        //Eat mulch
        if (blockBeneath is MulchBlock)
        {
            Debug.Log("Ate mulch.");
            currentHealth = Mathf.Min(currentHealth + mulchHealthAmount, maxHealth);
            WorldManager.Instance.SetBlock(antPosition.x, antPosition.y - 1, antPosition.z, new AirBlock());
            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        }


        //Scan for mulch and move there if the mulch exist


        Vector3Int antPosition2 = Vector3Int.RoundToInt(transform.position);
        for (int x = -scanRadius; x <= scanRadius; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                for (int z = -scanRadius; z <= scanRadius; z++)
                {

                    AbstractBlock nearbyBlock = WorldManager.Instance.GetBlock(antPosition.x + x, antPosition.y + y, antPosition.z + z);
                    if (nearbyBlock is MulchBlock)
                    {
                        transform.position = new Vector3(antPosition.x + x,antPosition.y + y + 1, antPosition.z + z);
                    }
                }
            }
        }
    }
}
