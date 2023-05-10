# Traffic Flow Simulation
**Full video demo:** https://youtu.be/FnTqEJruj08

![Traffic Flow](https://github.com/marcotorresx/traffic-flow/assets/90577455/779e4168-d8aa-4885-b54d-cc6e859080f4)

## Introduction

This project involves the development of a vehicular traffic simulation in an urban environment. The simulation generates autonomous vehicles that strive to reach their destinations while avoiding traffic congestion. The objective of the project is to apply multi-agent system concepts to create an efficient and intelligent traffic system.

## Technologies

### Python

The agents' processing and logic are constructed through a Python API that is tasked with managing the current state of each agent at every step of the model. The API generates a new state for each agent, utilizing the specialized Mesa library, tailored specifically for multi-agent models.

### Unity

The simulation graphics were created using Unity, with the application requesting updated agent states from the API at predetermined intervals and rendering them in real-time.

## How It Works

The simulation features various agents that interact with each other to create the traffic flow.

### Cars

Cars are the main agents in the simulation. They are generated at random intervals of time in the corners of the map and assigned a random destination. Each vehicle calculates the shortest path to its destination using the A\* (A star) search algorithm.

At each step of the model, each vehicle must analyze the following elements in its environment:

- If there is a vehicle in the next cell of the route, the vehicle must wait for it to move.
- If there are blockages or congestion on the current route, the vehicle will redirect by recalculating the next nearest route to the destination.
- If the vehicle is facing a red traffic light, it must wait until the light turns green.
- The vehicle must move in the direction indicated by the street (not against the flow of traffic).

### Traffic Lights

The traffic lights change colors at fixed intervals of time, controlling the flow of vehicles to avoid collisions.

### Roads

Each cell of the road has a specific direction that must be respected by the vehicles, and they cannot travel against the flow of traffic.

## Installation

### Miniconda

Install the 3.8 version for your OS from the following link: https://docs.conda.io/en/latest/miniconda.html

### Clone Repository

Press the code button and copy the https link. Clone the repository from the terminal with the following command:

`git clone https://github.com/marcotorresx/traffic-flow.git`

### Packages

To run this project, you need miniconda or anaconda and version 3.8 of Python. To run the simulation in Unity, you must have version 2021.3.12f1 installed.

Once you install miniconda and Unity, follow these steps:

- Create a virtual environment: `conda create --name traffic-sim python=3.8`
- Activate the virtual environment from the terminal: `conda activate traffic-sim`
- Install Flask and Mesa: `pip install flask` and `pip install mesa`

After installing everything, you are ready to run the simulation.

## Running the Simulation

Navigate to the server folder by entering the following command: `cd server`

Once you are in this folder, run `python server.py` to start the Flask API. Open the Unity project and play the BuildCity scene to run the simulation.
