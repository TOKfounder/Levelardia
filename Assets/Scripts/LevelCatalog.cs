using UnityEngine;

public static class LevelCatalog
{
	public const int TotalLevels = 100;
	const int LevelsPerAct = 20;
	const int ActCount = 5;

	enum LevelBeat { Breather, Standard, Pressure, Milestone }
	enum ArenaPattern { OpenGround, SideLanes, Crossroads, TightGrid, KillBox, LongRun }

	struct ActTheme
	{
		public string name;
		public Color foregroundA;
		public Color foregroundB;
		public Color backgroundA;
		public Color backgroundB;
	}

	struct ArenaTuning
	{
		public string label;
		public int widthOffset;
		public int heightOffset;
		public float obstacleOffset;
		public float minHeightBonus;
		public float maxHeightBonus;
	}

	public struct LevelDefinition
	{
		public int levelNumber;
		public string bannerTitle;
		public string bannerDescription;
		public Spawner.Wave wave;
		public MapGenerator.Map map;
	}

	static readonly ActTheme[] ActThemes = {
		new ActTheme {
			name = "Warm Up",
			foregroundA = new Color(0.18f, 0.72f, 0.98f),
			foregroundB = new Color(0.95f, 0.78f, 0.24f),
			backgroundA = new Color(0.08f, 0.16f, 0.45f),
			backgroundB = new Color(0.33f, 0.08f, 0.08f)
		},
		new ActTheme {
			name = "Momentum",
			foregroundA = new Color(0.10f, 0.92f, 0.50f),
			foregroundB = new Color(0.62f, 0.22f, 0.98f),
			backgroundA = new Color(0.03f, 0.24f, 0.18f),
			backgroundB = new Color(0.21f, 0.05f, 0.33f)
		},
		new ActTheme {
			name = "Pressure",
			foregroundA = new Color(0.98f, 0.42f, 0.08f),
			foregroundB = new Color(0.88f, 0.12f, 0.16f),
			backgroundA = new Color(0.26f, 0.07f, 0.04f),
			backgroundB = new Color(0.34f, 0.06f, 0.14f)
		},
		new ActTheme {
			name = "Mastery",
			foregroundA = new Color(0.25f, 0.62f, 1.00f),
			foregroundB = new Color(0.96f, 0.20f, 0.78f),
			backgroundA = new Color(0.07f, 0.09f, 0.32f),
			backgroundB = new Color(0.28f, 0.05f, 0.27f)
		},
		new ActTheme {
			name = "Final Stretch",
			foregroundA = new Color(0.92f, 0.92f, 0.95f),
			foregroundB = new Color(0.95f, 0.18f, 0.18f),
			backgroundA = new Color(0.10f, 0.10f, 0.16f),
			backgroundB = new Color(0.02f, 0.02f, 0.05f)
		}
	};

	static readonly ArenaTuning[] ArenaTunings = {
		new ArenaTuning { label = "Open Ground", widthOffset = 2, heightOffset = 1, obstacleOffset = -0.08f, minHeightBonus = -0.10f, maxHeightBonus = 0.10f },
		new ArenaTuning { label = "Side Lanes", widthOffset = 3, heightOffset = 0, obstacleOffset = -0.03f, minHeightBonus = 0.05f, maxHeightBonus = 0.20f },
		new ArenaTuning { label = "Crossroads", widthOffset = 1, heightOffset = 1, obstacleOffset = 0.02f, minHeightBonus = 0.10f, maxHeightBonus = 0.30f },
		new ArenaTuning { label = "Tight Grid", widthOffset = 0, heightOffset = 0, obstacleOffset = 0.08f, minHeightBonus = 0.15f, maxHeightBonus = 0.40f },
		new ArenaTuning { label = "Kill Box", widthOffset = -1, heightOffset = -1, obstacleOffset = 0.10f, minHeightBonus = 0.20f, maxHeightBonus = 0.50f },
		new ArenaTuning { label = "Long Run", widthOffset = 4, heightOffset = -1, obstacleOffset = -0.06f, minHeightBonus = -0.05f, maxHeightBonus = 0.20f }
	};

	public static int ClampLevel(int levelNumber)
	{
		return Mathf.Clamp(levelNumber, 1, TotalLevels);
	}

	public static int GetNextLevel(int levelNumber)
	{
		int level = ClampLevel(levelNumber);
		return (level < TotalLevels) ? level + 1 : 1;
	}

	public static LevelDefinition GetLevel(int levelNumber)
	{
		int level = ClampLevel(levelNumber);
		int actIndex = Mathf.Min((level - 1) / LevelsPerAct, ActCount - 1);
		int levelInAct = (level - 1) % LevelsPerAct;
		float actProgress = levelInAct / (float)(LevelsPerAct - 1);
		float globalProgress = (level - 1) / (float)(TotalLevels - 1);

		LevelBeat beat = GetBeat(level);
		ArenaPattern arenaPattern = GetArenaPattern(level, beat);
		ActTheme theme = ActThemes[actIndex];
		ArenaTuning arena = ArenaTunings[(int)arenaPattern];

		Spawner.Wave wave = BuildWave(level, actIndex, levelInAct, globalProgress, beat, theme);
		MapGenerator.Map map = BuildMap(level, actIndex, actProgress, beat, arena, theme);

		string bannerTitle = string.Format("- Level {0}/{1} -", level, TotalLevels);
		string bannerDescription = string.Format("{0} | {1}", GetBeatLabel(beat), BuildThreatLabel(arena.label, wave));

		ApplyOverrides(level, ref wave, ref map, ref bannerDescription);

		return new LevelDefinition {
			levelNumber = level,
			bannerTitle = bannerTitle,
			bannerDescription = bannerDescription,
			wave = wave,
			map = map
		};
	}

	static LevelBeat GetBeat(int level)
	{
		if (level == 1)
		{
			return LevelBeat.Breather;
		}

		switch ((level - 1) % 10)
		{
			case 0:
			case 5:
				return LevelBeat.Breather;
			case 4:
			case 7:
			case 8:
				return LevelBeat.Pressure;
			case 9:
				return LevelBeat.Milestone;
			default:
				return LevelBeat.Standard;
		}
	}

	static ArenaPattern GetArenaPattern(int level, LevelBeat beat)
	{
		if (beat == LevelBeat.Breather)
		{
			return (level % 2 == 0) ? ArenaPattern.LongRun : ArenaPattern.OpenGround;
		}

		if (beat == LevelBeat.Milestone)
		{
			switch ((level / 10) % 5)
			{
				case 0:
					return ArenaPattern.Crossroads;
				case 1:
					return ArenaPattern.SideLanes;
				case 2:
					return ArenaPattern.TightGrid;
				case 3:
					return ArenaPattern.KillBox;
				default:
					return ArenaPattern.OpenGround;
			}
		}

		ArenaPattern[] cycle = {
			ArenaPattern.OpenGround,
			ArenaPattern.SideLanes,
			ArenaPattern.Crossroads,
			ArenaPattern.TightGrid,
			ArenaPattern.LongRun
		};

		if (beat == LevelBeat.Pressure)
		{
			ArenaPattern[] pressureCycle = {
				ArenaPattern.Crossroads,
				ArenaPattern.TightGrid,
				ArenaPattern.KillBox,
				ArenaPattern.SideLanes
			};
			return pressureCycle[(level / 2) % pressureCycle.Length];
		}

		return cycle[level % cycle.Length];
	}

	static Spawner.Wave BuildWave(int level, int actIndex, int levelInAct, float globalProgress, LevelBeat beat, ActTheme theme)
	{
		float targetDuration = 27f + actIndex * 1.8f + levelInAct * 0.1f + GetBeatDurationOffset(beat);
		targetDuration = Mathf.Clamp(targetDuration, 25f, 39f);

		float spawnInterval = Mathf.Lerp(0.95f, 0.48f, globalProgress) + GetBeatSpawnOffset(beat);
		spawnInterval = Mathf.Clamp(spawnInterval, 0.42f, 1.08f);

		float endBuffer = Mathf.Lerp(5.5f, 8.5f, globalProgress) + GetBeatEndBufferOffset(beat);
		int enemyCount = Mathf.Clamp(Mathf.RoundToInt((targetDuration - endBuffer) / spawnInterval), 18, 80);

		float moveSpeed = Mathf.Lerp(2.2f, 4.4f, globalProgress) + GetBeatMoveSpeedOffset(beat);
		moveSpeed = Mathf.Clamp(moveSpeed, 2.0f, 4.9f);

		float enemyHealth = 1f + actIndex * 0.55f + levelInAct * 0.03f + GetBeatHealthOffset(beat);
		enemyHealth = Mathf.Clamp(enemyHealth, 1f, 4.4f);

		int hitsToKillPlayer = GetHitsToKillPlayer(level, beat);

		Color skinColour = Color.Lerp(theme.foregroundA, theme.foregroundB, (levelInAct + 1f) / LevelsPerAct);

		return new Spawner.Wave {
			infinite = false,
			enemyCount = enemyCount,
			timeBetweenSpawns = spawnInterval,
			moveSpeed = moveSpeed,
			hitsToKillPlayer = hitsToKillPlayer,
			enemyHealth = enemyHealth,
			skinColour = skinColour
		};
	}

	static MapGenerator.Map BuildMap(int level, int actIndex, float actProgress, LevelBeat beat, ArenaTuning arena, ActTheme theme)
	{
		int width = Mathf.Clamp(11 + actIndex * 2 + arena.widthOffset, 9, 26);
		int height = Mathf.Clamp(9 + actIndex + arena.heightOffset, 8, 18);

		float obstaclePercent = 0.23f + actIndex * 0.02f + arena.obstacleOffset + GetBeatObstacleOffset(beat);
		obstaclePercent = Mathf.Clamp(obstaclePercent, 0.14f, 0.42f);

		float minObstacleHeight = 0.95f + actIndex * 0.18f + arena.minHeightBonus;
		float maxObstacleHeight = minObstacleHeight + 1.25f + actIndex * 0.18f + arena.maxHeightBonus;

		Color foreground = Color.Lerp(theme.foregroundA, theme.foregroundB, actProgress);
		Color background = Color.Lerp(theme.backgroundA, theme.backgroundB, actProgress);

		return new MapGenerator.Map {
			mapSize = new MapGenerator.Coord(width, height),
			obstaclePercent = obstaclePercent,
			seed = 1000 + level * 17 + Mathf.RoundToInt(arena.obstacleOffset * 1000f),
			minObstacleHeight = minObstacleHeight,
			maxObstacleHeight = maxObstacleHeight,
			foregroundColour = foreground,
			backgroundColour = background
		};
	}

	static void ApplyOverrides(int level, ref Spawner.Wave wave, ref MapGenerator.Map map, ref string bannerDescription)
	{
		switch (level)
		{
			case 1:
				wave.enemyCount = 18;
				wave.timeBetweenSpawns = 1.02f;
				wave.moveSpeed = 2.1f;
				wave.enemyHealth = 1f;
				map.mapSize = new MapGenerator.Coord(11, 9);
				map.obstaclePercent = 0.18f;
				bannerDescription = "Warm-up | Open Ground | 18 enemies";
				break;
			case 10:
				wave.enemyCount += 2;
				map.mapSize = new MapGenerator.Coord(13, 10);
				bannerDescription = "Milestone | Dash Test | " + wave.enemyCount + " enemies";
				break;
			case 25:
				wave.timeBetweenSpawns = Mathf.Max(0.56f, wave.timeBetweenSpawns - 0.04f);
				bannerDescription = "Pressure | Fast Swarm | " + wave.enemyCount + " enemies";
				break;
			case 40:
				map.obstaclePercent = Mathf.Clamp(map.obstaclePercent + 0.03f, 0.14f, 0.42f);
				bannerDescription = "Milestone | Corner Pressure | " + wave.enemyCount + " enemies";
				break;
			case 50:
				wave.moveSpeed += 0.15f;
				bannerDescription = "Milestone | Midpoint Rush | " + wave.enemyCount + " enemies";
				break;
			case 60:
				wave.enemyHealth += 0.35f;
				map.mapSize = new MapGenerator.Coord(18, 12);
				bannerDescription = "Milestone | No Shelter | " + wave.enemyCount + " enemies";
				break;
			case 75:
				wave.enemyHealth += 0.45f;
				map.obstaclePercent = Mathf.Clamp(map.obstaclePercent + 0.04f, 0.14f, 0.42f);
				bannerDescription = "Pressure | Lockdown | " + wave.enemyCount + " enemies";
				break;
			case 90:
				wave.moveSpeed += 0.2f;
				wave.hitsToKillPlayer = 2;
				bannerDescription = "Milestone | Red Alert | " + wave.enemyCount + " enemies";
				break;
			case 100:
				wave.enemyCount = Mathf.Max(wave.enemyCount, 68);
				wave.timeBetweenSpawns = 0.42f;
				wave.moveSpeed = 4.75f;
				wave.enemyHealth = 4.4f;
				wave.hitsToKillPlayer = 2;
				map.mapSize = new MapGenerator.Coord(20, 14);
				map.obstaclePercent = 0.24f;
				map.seed = 9099;
				bannerDescription = "Final Test | Final Shift | " + wave.enemyCount + " enemies";
				break;
		}
	}

	static string GetBeatLabel(LevelBeat beat)
	{
		switch (beat)
		{
			case LevelBeat.Breather:
				return "Breather";
			case LevelBeat.Pressure:
				return "Pressure";
			case LevelBeat.Milestone:
				return "Milestone";
			default:
				return "Standard";
		}
	}

	static string BuildThreatLabel(string arenaLabel, Spawner.Wave wave)
	{
		return string.Format("{0} | {1} enemies", arenaLabel, wave.enemyCount);
	}

	static float GetBeatDurationOffset(LevelBeat beat)
	{
		switch (beat)
		{
			case LevelBeat.Breather:
				return -2.5f;
			case LevelBeat.Pressure:
				return 2f;
			case LevelBeat.Milestone:
				return 4f;
			default:
				return 0f;
		}
	}

	static float GetBeatSpawnOffset(LevelBeat beat)
	{
		switch (beat)
		{
			case LevelBeat.Breather:
				return 0.08f;
			case LevelBeat.Pressure:
				return -0.04f;
			case LevelBeat.Milestone:
				return -0.06f;
			default:
				return 0f;
		}
	}

	static float GetBeatEndBufferOffset(LevelBeat beat)
	{
		switch (beat)
		{
			case LevelBeat.Breather:
				return -0.5f;
			case LevelBeat.Milestone:
				return 1f;
			default:
				return 0f;
		}
	}

	static float GetBeatMoveSpeedOffset(LevelBeat beat)
	{
		switch (beat)
		{
			case LevelBeat.Breather:
				return -0.15f;
			case LevelBeat.Pressure:
				return 0.2f;
			case LevelBeat.Milestone:
				return 0.1f;
			default:
				return 0f;
		}
	}

	static float GetBeatHealthOffset(LevelBeat beat)
	{
		switch (beat)
		{
			case LevelBeat.Pressure:
				return 0.2f;
			case LevelBeat.Milestone:
				return 0.5f;
			default:
				return 0f;
		}
	}

	static float GetBeatObstacleOffset(LevelBeat beat)
	{
		switch (beat)
		{
			case LevelBeat.Breather:
				return -0.04f;
			case LevelBeat.Pressure:
				return 0.02f;
			case LevelBeat.Milestone:
				return 0.01f;
			default:
				return 0f;
		}
	}

	static int GetHitsToKillPlayer(int level, LevelBeat beat)
	{
		int hitsToKillPlayer;

		if (level <= 20)
		{
			hitsToKillPlayer = 6;
		}
		else if (level <= 40)
		{
			hitsToKillPlayer = 5;
		}
		else if (level <= 60)
		{
			hitsToKillPlayer = 4;
		}
		else
		{
			hitsToKillPlayer = 3;
		}

		if (beat == LevelBeat.Breather)
		{
			hitsToKillPlayer = Mathf.Min(hitsToKillPlayer + 1, 6);
		}
		else if (beat == LevelBeat.Milestone && level >= 90)
		{
			hitsToKillPlayer = 2;
		}

		return hitsToKillPlayer;
	}
}
