using System.Collections;
using UnityEngine;
using YG;

public class Spawner : MonoBehaviour
{
	public Wave[] waves;
	public Enemy enemy;

	LivingEntity playerEntity;
	Transform playerT;

	Wave currentWave;
	int currentWaveNumber;

	int enemiesRemainingToSpawn;
	int enemiesRemainingAlive;
	float nextSpawnTime;

	MapGenerator map;

	float timeBetweenCampingChecks = 2;
	float campThresholdDistance = 1.5f;
	float nextCampCheckTime;
	Vector3 campPositionOld;
	bool isCamping;

	bool isDisabled;

	public event System.Action<int> OnNewWave;

	void Start()
	{
		playerEntity = FindFirstObjectByType<Player>();
		playerT = playerEntity.transform;

		nextCampCheckTime = timeBetweenCampingChecks + Time.time;
		campPositionOld = playerT.position;
		playerEntity.OnDeath += OnPlayerDeath;

		map = FindFirstObjectByType<MapGenerator>();
		if (map == null)
		{
			Debug.LogError("MapGenerator не найден!");
			enabled = false;
			return;
		}
		NextWave();
	}

	void Update()
	{
		if (!isDisabled)
		{
			if (Time.time > nextCampCheckTime)
			{
				nextCampCheckTime = Time.time + timeBetweenCampingChecks;

				isCamping = Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance;
				campPositionOld = playerT.position;
			}
			
			if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
			{
				enemiesRemainingToSpawn --;
				nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

				StartCoroutine(SpawnEnemy());
			}
		}
	}

	IEnumerator SpawnEnemy()
	{
		float spawnDelay = 1;
		float tileFlashSpeed = 4;

		Transform spawnTile = map.GetRandomOpenTile();
		if (map == null || spawnTile == null) yield break;
		if (isCamping)
		{
			spawnTile = map.GetTileFromPosition(playerT.position);
		}
		Material tileMat = spawnTile.GetComponent<Renderer>().material;
		Color initialColour = Color.white;
		Color flashColour = Color.red;
		float spawnTimer = 0;

		while (spawnTimer < spawnDelay)
		{
			tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

			spawnTimer += Time.deltaTime;
			yield return null;
		}

		Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity);
		spawnedEnemy.OnDeath += OnEnemyDeath;
		spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColour);
	}

	void OnPlayerDeath()
	{
		isDisabled = true;
		YG2.saves.currentWave = 1;
	}

	void OnEnemyDeath()
	{
		enemiesRemainingAlive --;

		if (enemiesRemainingAlive == 0)
		{
			NextWave();
		}
	}

	void ResetPlayerPosition()
	{
		playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
	}

	void NextWave()
	{
		if (YG2.saves.currentWave > 0)
		{
			AudioManager.instance.PlaySound2D("Level Complete");
		}
		
		if (!YG2.saves.isStartGame)
		{
			if (YG2.saves.currentWave < 5)
				YG2.saves.currentWave ++;
			else
			{
				YG2.saves.currentWave = 1;
			}
		}
		else
			YG2.saves.isStartGame = false;
		YG2.SaveProgress();

		
		if (YG2.saves.currentWave - 1 < waves.Length)
		{
			currentWave = waves[YG2.saves.currentWave - 1];

			enemiesRemainingToSpawn = currentWave.enemyCount;
			enemiesRemainingAlive = enemiesRemainingToSpawn;

			if (OnNewWave != null)
			{
				OnNewWave(YG2.saves.currentWave);
			}
			ResetPlayerPosition();
		}
	}

	[System.Serializable]
	public class Wave
	{
		public bool infinite;
		public int enemyCount;
		public float timeBetweenSpawns;

		public float moveSpeed;
		public int hitsToKillPlayer;
		public float enemyHealth;
		public Color skinColour;
	}
	
}
