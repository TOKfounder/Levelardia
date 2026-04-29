using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
	public static int score { get; private set; }
	float lastEnemyKillTime;
	int streakCount;
	float streakExpiryTime = 1;
	Player player;

	void Awake()
	{
		score = 0;
		lastEnemyKillTime = -streakExpiryTime;
		streakCount = 0;
	}

	void Start()
	{
		Enemy.OnDeathStatic += OnEnemyKilled;
		player = FindAnyObjectByType<Player>();
		if (player != null)
		{
			player.OnDeath += OnPlayerDeath;
		}
	}

	void OnEnemyKilled()
	{
		if (Time.time < lastEnemyKillTime + streakExpiryTime)
		{
			streakCount ++;
		} else
		{
			streakCount = 0;
		}

		lastEnemyKillTime = Time.time;

		score += 5 + (int)Mathf.Pow(2, streakCount);
	}
	
	void OnPlayerDeath()
	{
		Enemy.OnDeathStatic -= OnEnemyKilled;
		if (player != null)
		{
			player.OnDeath -= OnPlayerDeath;
		}
	}

	void OnDestroy()
	{
		Enemy.OnDeathStatic -= OnEnemyKilled;
		if (player != null)
		{
			player.OnDeath -= OnPlayerDeath;
		}
	}
}
