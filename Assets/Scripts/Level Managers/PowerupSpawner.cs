using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PowerupSpawner : MonoBehaviour
{
	#region Fields
	private static PowerupSpawner instance;

	[System.Serializable]
	public struct Wave
	{
		public float startTime;
		public List<Powerup> possiblePowerups;
	}

	public bool spawnPowerups;
	public List<Wave> waves;
	public List<Microchip> microchips;
	public List<Gun> guns;

	private int currentWave = 0;
	private float waveTimer = 0f;

	private List<GameObject> spawners;
	#endregion

	#region Public Properties
	public static PowerupSpawner Instance
	{
		get { return instance; }
	}
	#endregion

	#region Internal Properties
	private Vector3 RandomSpawnPoint
	{
		get
		{
			if (spawners.Count > 0)
			{
				int spawner = Random.Range(0, spawners.Count);
				Vector3 startPoint = spawners[spawner].transform.FindChild("Powerup Start").position;
				Vector3 endPoint = spawners[spawner].transform.FindChild("Powerup End").position;

				return new Vector3(Random.Range(startPoint.x, endPoint.x),
								   Random.Range(startPoint.y, endPoint.y),
								   startPoint.z);
			}
			else
			{
				return Vector3.up;
			}
		}
	}
	#endregion

	#region MonoBehaviour
	private void Awake()
	{
		instance = this;

		spawners = GameObject.FindGameObjectsWithTag("PowerupSpawner").ToList();
	}

	private void Update()
	{
		waveTimer = LevelManager.Instance.MusicTime;

		if (spawnPowerups && !PlayerControl.Instance.Dead &&
			currentWave < waves.Count && waveTimer >= waves[currentWave].startTime)
		{
			int powerupToSpawn = Random.Range(0, waves[currentWave].possiblePowerups.Count);
			Instantiate(waves[currentWave].possiblePowerups[powerupToSpawn], RandomSpawnPoint, Quaternion.identity);
			currentWave++;
		}
	}
	#endregion

	#region Public Methods
	public void SpawnMicrochip(Vector3 position, Microchip.Size size = Microchip.Size.Small, bool randomVelocity = true)
	{
		Microchip microchipToSpawn = microchips[(int)size];

		Microchip microchipInstance = Instantiate(microchipToSpawn, position, Quaternion.identity) as Microchip;

		if (randomVelocity)
		{
			microchipInstance.GetComponent<Scatter>().DoScatter();
		}
	}
	#endregion
}
