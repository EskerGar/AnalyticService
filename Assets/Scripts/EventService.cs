using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


[Serializable]
public class SerializableEventsList
{
    [SerializeField] private List<EventData> events;

    public List<EventData> Events => events;

    public SerializableEventsList(List<EventData> list)
    {
        events = list;
    }
}

[Serializable]
public class EventData
{
   [SerializeField] private string type;
   [SerializeField] private string data;

   public EventData(string type, string data)
   {
       this.type = type;
       this.data = data;
   }
}

public class EventService : MonoBehaviour
{
    [SerializeField] private float _cooldownBeforeSend;
    [SerializeField] private Config _config;
    
    private const int OkResponseCode = 200;
    private const string EventDataSavePath = "/EventsData.json";
    
    private List<EventData> _events = new List<EventData>();
    private string _serverUrl;
    private WaitForSecondsRealtime _cooldownWaitTime;

    private void Start()
    {
        _cooldownWaitTime = new WaitForSecondsRealtime(_cooldownBeforeSend);
        _serverUrl = _config.ServerUrl;

        TryLoadPlayerData(out _events);

        StartCoroutine(SendEventCoroutine());
    }

    private void OnApplicationQuit()
    {
        SaveEventsData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        SaveEventsData();
    }

    public void TrackEvent(string type, string data) 
    {
        _events.Add(new EventData(type, data));
    }

    private IEnumerator SendEventCoroutine()
    {
        while (true)
        {
            yield return _cooldownWaitTime;

            if (_events.Count > 0 && string.IsNullOrEmpty(_serverUrl) == false)
            {
                var serializableEventsList = new SerializableEventsList(_events);
                var eventDataJson = JsonUtility.ToJson(serializableEventsList);
                var request = UnityWebRequest.Post(_serverUrl, eventDataJson);
        
                yield return request.SendWebRequest();
        
                if (request.responseCode == OkResponseCode)
                {
                    _events.Clear();
                }
            }
            else if (string.IsNullOrEmpty(_serverUrl))
            {
                Debug.LogError("ServerUrl is empty");
            }
        }
    }
    
    private void SaveEventsData()
    {
        var serializableEventsList = new SerializableEventsList(_events);
        var eventDataJson = JsonUtility.ToJson(serializableEventsList);
        File.WriteAllText(Application.persistentDataPath + EventDataSavePath, eventDataJson);
    }
    
    private bool TryLoadPlayerData(out List<EventData> list)
    {
        list = default;
        var filePath = Application.persistentDataPath + EventDataSavePath;

        if (File.Exists(filePath))
        {
            var data = File.ReadAllText(filePath);
        
            var serializableEventsList = JsonUtility.FromJson<SerializableEventsList>(data);
            list = serializableEventsList.Events;
        }
        else
        {
            return false;
        }

        return true;
    }
}