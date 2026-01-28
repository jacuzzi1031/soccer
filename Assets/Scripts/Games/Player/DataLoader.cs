
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
public class DataLoader : MonoBehaviour
{
    public static DataLoader Instance{get; private set;}
    [HideInInspector] public List<string> countries = new List<string> { "DEFAULT" };
    public Dictionary<string, List<PlayerResource>> squads = new Dictionary<string, List<PlayerResource>>();

    void Awake()
    {   
        Instance = this;
        LoadJson();
    }

    private void LoadJson()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "squads.json");

        if (!File.Exists(jsonPath))
        {
            Debug.LogError("could not find or load squads.json");
            return;
        }

        string jsonText = File.ReadAllText(jsonPath);

        List<SquadJson> teams = JsonConvert.DeserializeObject<List<SquadJson>>(jsonText);
        foreach (var team in teams)
        {
            string countryName = team.country;
            countries.Add(countryName);

            if (!squads.ContainsKey(countryName))
                squads[countryName] = new List<PlayerResource>();

            foreach (var player in team.players)
            {
                PlayerResource pr = new PlayerResource(
                    player.name,
                    (PlayerView.SkinColor)player.skin,
                    (Role)player.role,
                    player.speed,
                    player.power
                );

                squads[countryName].Add(pr);
            }

            Debug.Assert(team.players.Count == 6, $"{countryName} should have 6 players!");
        }
    }

    public List<PlayerResource> GetSquad(string country)
    {
        if (squads.ContainsKey(country))
            return squads[country];

        return new List<PlayerResource>();
    }

    public List<string> GetCountries()
    {
        return countries;
    }
}
[System.Serializable]
public class SquadJson
{
    public string country;
    public List<PlayerJson> players;
}

[System.Serializable]
public class PlayerJson
{
    public string name;
    public int skin;
    public int role;
    public float speed;
    public float power;
}
