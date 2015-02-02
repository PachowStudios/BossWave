using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PowerupSpawner : MonoBehaviour 
{
	private static PowerupSpawner instance;

	public bool spawnPowerups;
	public List<Powerup> powerups;
	public List<Microchip> microchips;
	public List<Gun> guns;

	public float minPowerupTime = 15f;
	public float maxPowerupTime = 25f;

	private float powerupTimer = 0f;
	private float powerupTime;

	private List<GameObject> spawners;

	public static PowerupSpawner Instance
	{
		get { return instance; }
	}

	private float NewPowerupTime
	{
		get
		{
			return Random.Range(minPowerupTime, maxPowerupTime);
		}
	}

	private Powerup RandomPowerup
	{
		get
		{
			if (powerups.Count > 0)
			{
				return powerups[Random.Range(0, powerups.Count)];
			}
			else
			{
				return null;
			}
		}
	}

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

	private void Awake()
	{
		instance = this;

		spawners = GameObject.FindGameObjectsWithTag("PowerupSpawner").ToList();
		powerupTime = NewPowerupTime;
	}

	private void FixedUpdate()
	{
		if (spawnPowerups && !PlayerControl.Instance.Dead && 
			powerups.Count > 0 && spawners.Count > 0)
		{
			powerupTimer += Time.deltaTime;

			if (powerupTimer >= powerupTime)
			{
				Instantiate(RandomPowerup, RandomSpawnPoint, Quaternion.identity);

				powerupTimer = 0f;
				powerupTime = NewPowerupTime;
			}
		}
	}

	public void SpawnMicrochip(Vector3 position, Microchip.Size size = Microchip.Size.Small, bool randomVelocity = true)
	{
		Microchip microchipToSpawn = microchips[(int)size];

		Microchip microchipInstance = Instantiate(microchipToSpawn, position, Quaternion.identity) as Microchip;

		if (randomVelocity)
		{
			microchipInstance.GetComponent<Scatter>().DoScatter();
		}
	}
}
