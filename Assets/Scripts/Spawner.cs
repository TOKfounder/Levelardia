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
	LevelCatalog.LevelDefinition currentLevelDefinition;

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
		YG2.saves.currentWave = LevelCatalog.ClampLevel(YG2.saves.currentWave);
		LoadLevel(YG2.saves.currentWave);
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
		YG2.saves.isStartGame = true;
		YG2.SaveProgress();
	}

	void OnEnemyDeath()
	{
		enemiesRemainingAlive --;

		if (enemiesRemainingAlive == 0)
		{
			AdvanceToNextLevel();
		}
	}

	void ResetPlayerPosition()
	{
		playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
	}

	void AdvanceToNextLevel()
	{
		AudioManager.instance.PlaySound2D("Level Complete");
		YG2.saves.isStartGame = false;
		YG2.saves.currentWave = LevelCatalog.GetNextLevel(YG2.saves.currentWave);
		YG2.SaveProgress();
		LoadLevel(YG2.saves.currentWave);
	}

	void LoadLevel(int levelNumber)
	{
		currentLevelDefinition = LevelCatalog.GetLevel(levelNumber);
		currentWave = currentLevelDefinition.wave;

		enemiesRemainingToSpawn = currentWave.enemyCount;
		enemiesRemainingAlive = enemiesRemainingToSpawn;
		nextSpawnTime = Time.time;

		if (OnNewWave != null)
		{
			OnNewWave(currentLevelDefinition.levelNumber);
		}
		ResetPlayerPosition();
		campPositionOld = playerT.position;
		nextCampCheckTime = Time.time + timeBetweenCampingChecks;
		isCamping = false;
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
