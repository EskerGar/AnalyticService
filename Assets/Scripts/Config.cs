using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "Config")]
public class Config : ScriptableObject
{
	[SerializeField] private string _serverUrl;

	public string ServerUrl => _serverUrl;
}