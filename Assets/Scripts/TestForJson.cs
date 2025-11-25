using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

class TestForJson: MonoBehaviour
{
    void OnEnable()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "squads.json");
        string jsonText = File.ReadAllText(filePath);

        // 解析 JSON
        JArray countries = JArray.Parse(jsonText);

        foreach (JObject country in countries)
        {
            JArray players = (JArray)country["players"];
            foreach (JObject player in players)
            {
                // 获取原始值
                float speed = player["speed"].Value<float>();
                float power = player["power"].Value<float>();

                // 除以 18 并保留 1 位小数
                player["speed"] = (float)Math.Round(speed / 18f, 1);
                player["power"] = (float)Math.Round(power / 18f, 1);
            }
        }

        // 保存回文件
        File.WriteAllText(filePath, countries.ToString(Formatting.Indented));

        Console.WriteLine("修改完成！");
    }
}