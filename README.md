# Assignment 3: Antymology

## Goal
My project models ant behaviour of ants building the largest nest possible. There are a set number of ants that dig through grass and each mulch blocks. They dig to make more room for the nest, and eat to stay alive. They scan their immediate surroundings and move to the highest block in the near proximity, as long as it is within a height of 2. The moving higher allows optimal clearing for nests as it will prevent crators with large towers of mulch and grass that can't be climbed. They prioritize digging over eating as long as they have enough health. They place pheromones on every block they are at. While the ants dig and eat, the queen places nest blocks. Since each block consumes 1/3rd of her health, once her health is low she moves to the block with the highest pheromone concentration in her near surronding until she can meet a regular ant to exchange health with. Provided this ant has enough health, it will transfer most of it's health to her so that she can continue to build the nest.

![](https://github.com/ryanchrumka/Antymology/blob/master/gif2_V3.gif)

![](https://github.com/ryanchrumka/Antymology/blob/master/Untitled_Project_V1.gif)

Although it was not created when I made the gifs, the number of nest blocks is tracked in the corner:

![](https://github.com/ryanchrumka/Antymology/blob/master/Screenshot.JPG)

### UI
To use the camera while in play mode, use the following commands:
- W: move forward
- A: move left
- S: move backward
- D: move right
- Q: move straight up
- E: move straight down
- leftShift: hold to move faster
- space: hold to stay in the same height (y) while moving forward or backwards
- arrows: rotate in place

### Ant Behaviour
Note that the queen is the ant with the red halo.
- Ants health is reduced by a fixed amount at every timestep.
- Ants can replenish health by consuming Mulch blocks, which are then removed from the world.
- Ants are limited in movement by a maximum height difference of 2 units between blocks.
- Ants can dig up blocks they are directly standing on and dug-up blocks are removed from the map.
- Standing on an AcidicBlock doubles the rate at which an ant's health decreases.
- Ants can transfer health to each other when occupying the same space, ensuring a zero-sum exchange.
- There is a queen ant responsible for creating nest blocks, costing her one-third of her maximum health per block.
- Ants, including the queen, cannot create new blocks or ants during each evaluation phase.
- The movement and actions of ants prioritize:
    - Moving to the highest Mulch block if it's within a 1-height difference from the highest Grass block and the ant has sufficient health.
    - Eating Mulch directly below if it's the best option available.
    - Digging up Grass blocks directly below if no better Mulch blocks are available.
    - Seeking out higher Grass or Mulch blocks within a 2-block height if no immediate higher blocks are available.
- The queen ant searches for the lowest non-Mulch block within a nearby radius for placing nest blocks.
- She also seeks out the highest concentration of pheromones within a nearby radius to guide her movement.
- Health transfers from ants to the queen occur within a proximity of 2 blocks.
  


