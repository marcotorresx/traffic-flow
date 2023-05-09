using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class AgentData
{
    public string id, direction;
    public float x, y, z;
    public bool green, arrived;
    public int destination;

    public AgentData(string id, string direction, float x, float y, float z, bool green = false, bool arrived = false, int destination = 0)
    {
        this.id = id;
        this.direction = direction;
        this.x = x;
        this.y = y;
        this.z = z;
        this.green = green;
        this.arrived = arrived;
        this.destination = destination;
    }
} 

[Serializable]
public class AgentsData
{
    public List<AgentData> positions;
    public AgentsData() => this.positions = new List<AgentData>();
}

public class AgentController : MonoBehaviour
{
    string serverUrl = "http://localhost:8585";
    string sendConfigEndpoint = "/init";
    string updateEndpoint = "/update";
    string getAgentsEndpoint = "/getCars";
    string getObstaclesEndpoint = "/getTrafficLights";
    AgentsData carsData, trafficLightsData;
    Dictionary<string, GameObject> agents;
    Dictionary<string, GameObject> trafficLights;
    Dictionary<string, Vector3> prevPositions, currPositions;
    Dictionary<string, int> tlRotations;
    Dictionary<int, GameObject> destinationCarColors;

    bool updated = false, started = false;

    public GameObject trafficLightPrefabGreen, trafficLightPrefabRed;
    public GameObject carOrange, carRed, carYellow, carBlue, carPurple, carPink, carGreen;
    public float timeToUpdate = 5.0f;
    private float timer, dt;

    void Start()
    {
        carsData = new AgentsData();
        trafficLightsData = new AgentsData();

        prevPositions = new Dictionary<string, Vector3>();
        currPositions = new Dictionary<string, Vector3>();

        agents = new Dictionary<string, GameObject>();
        trafficLights = new Dictionary<string, GameObject>();

        tlRotations = new Dictionary<string, int>();
        tlRotations["U"] = -90;
        tlRotations["A"] = 90;
        tlRotations["R"] = 0;
        tlRotations["L"] = 180;

        destinationCarColors = new Dictionary<int, GameObject>();
        destinationCarColors[0] = carOrange;
        destinationCarColors[1] = carRed;
        destinationCarColors[2] = carYellow;
        destinationCarColors[3] = carBlue;
        destinationCarColors[4] = carPurple;
        destinationCarColors[5] = carPink;
        destinationCarColors[6] = carGreen;

        
        timer = timeToUpdate;

        StartCoroutine(SendConfiguration());
    }

    private void Update() 
    {
        if(timer < 0)
        {
            timer = timeToUpdate;
            updated = false;
            StartCoroutine(UpdateSimulation());
        }

        if (updated)
        {
            timer -= Time.deltaTime;
            dt = 1.0f - (timer / timeToUpdate);

            foreach(var agent in currPositions)
            {
                Vector3 currentPosition = agent.Value;
                Vector3 previousPosition = prevPositions[agent.Key];

                Vector3 interpolated = Vector3.Lerp(previousPosition, currentPosition, dt);
                Vector3 direction = currentPosition - interpolated;

                agents[agent.Key].transform.localPosition = interpolated;
                if(direction != Vector3.zero) agents[agent.Key].transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
 
    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + updateEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            StartCoroutine(GetCarsData());
            StartCoroutine(GetTrafficLightsData());
        }
    }

    IEnumerator SendConfiguration()
    {
        WWWForm form = new WWWForm();

        UnityWebRequest www = UnityWebRequest.Post(serverUrl + sendConfigEndpoint, form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Configuration upload complete!");
            Debug.Log("Getting Agents positions");
            StartCoroutine(GetCarsData());
            StartCoroutine(GetTrafficLightsData());
        }
    }

    IEnumerator GetCarsData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getAgentsEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else  
        {
            carsData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);
            Debug.Log("Cars: " + agents.Count); // Number of cars in the grid

            foreach(AgentData agent in carsData.positions)
            {
                if (agent.arrived){
                    if (agents.TryGetValue(agent.id, out _)) {
                        GameObject agentToRemove = agents[agent.id];
                        Destroy(agentToRemove);
                        agents.Remove(agent.id); 
                        prevPositions.Remove(agent.id); 
                        currPositions.Remove(agent.id); 
                    }
                } else { 
                    Vector3 newAgentPosition = new Vector3(agent.x, agent.y, agent.z - 1);

                    if(!agents.TryGetValue(agent.id, out _))
                    { 
                        prevPositions[agent.id] = newAgentPosition;
                        agents[agent.id] = Instantiate(
                            destinationCarColors[agent.destination], 
                            newAgentPosition, 
                            Quaternion.identity
                        );
                    }
                    else
                    {
                        Vector3 currentPosition = new Vector3();
                        if(currPositions.TryGetValue(agent.id, out currentPosition))
                            prevPositions[agent.id] = currentPosition;
                        currPositions[agent.id] = newAgentPosition;
                    }
                }

            }

            updated = true;
            if(!started) started = true;
        }
    }

    IEnumerator GetTrafficLightsData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getObstaclesEndpoint);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);  
        else 
        {
            trafficLightsData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            foreach(AgentData trafficLight in trafficLightsData.positions) 
            {
                if(!trafficLights.TryGetValue(trafficLight.id, out _))
                {
                    trafficLights[trafficLight.id] = Instantiate( 
                        trafficLight.green ? trafficLightPrefabGreen : trafficLightPrefabRed, 
                        new Vector3(trafficLight.x, trafficLight.y, trafficLight.z - 1), Quaternion.Euler(0, tlRotations[trafficLight.direction], 0)
                    );
                } else 
                {
                    Destroy(trafficLights[trafficLight.id]);
                    trafficLights[trafficLight.id] = Instantiate( 
                        trafficLight.green ? trafficLightPrefabGreen : trafficLightPrefabRed, 
                        new Vector3(trafficLight.x, trafficLight.y, trafficLight.z - 1), Quaternion.Euler(0, tlRotations[trafficLight.direction], 0)
                    );
                }
            }
        }
    }
}
