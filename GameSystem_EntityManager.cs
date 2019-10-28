using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntitySystem;

namespace GameSystem
{
    public class GameSystem_EntityManager : MonoBehaviour
    {
        #region Pooling Singleton
        public static GameSystem_EntityManager Instance;

        private void Awake()
        {
            Instance = this;
        }
        #endregion

        [System.Serializable]
        internal class Spawner
        {
            [SerializeField]
            internal string identity; //Type of spawner.

            [SerializeField]
            internal int maximumSpawn;

            [SerializeField]
            internal int currentSpawned;

            [SerializeField]
            internal bool isEnabled;

            [Space]
            [SerializeField]
            internal float minSpawnTimeRange;
            [SerializeField]
            internal float maxSpawnTimeRange;
            internal float timeTillNextSpawn;
        }
        
        


        [SerializeField]
        private int maximumEntities;
        [SerializeField]
        public int currentTotalEntities;


        float currentTime;

        [SerializeField]
        internal bool canSpawnEntity;

        //Declaring our modules
        [SerializeField]
        internal List<Spawner> spawners;

        [SerializeField]
        internal List<Transform> orbitSpawnPoints;
        Transform spawnPoint;


        private float currentTimeTillNextSpawn;


        #region PLAYER MANAGEMENT
        [SerializeField]
        internal int totalPlayers = 1; //Default is 1. Multiplayer will set this value for the host.
        [SerializeField]
        internal List<EntitySystem_Core> players; //Default player is [0].
        [SerializeField]
        internal float collectivePlayerLevel;
        [SerializeField]
        internal float collectivePlayerExperience;
        [SerializeField]
        internal float experienceNeededToLevel;
        

        internal int deadPlayers;
        [SerializeField]
        private string endOfGameSceneName;
        private bool gameOver;

        #endregion
        #region DROP MANAGEMENT
        [SerializeField]
        internal GameObject[] blueprintSpawnables_common; //All blueprint combinations go here for entities to reference on drop.
        [SerializeField]
        internal GameObject[] blueprintSpawnables_uncommon;
        [SerializeField]
        internal GameObject[] blueprintSpawnables_rare;
        [SerializeField]
        internal GameObject[] blueprintSpawnables_epic;
        [SerializeField]
        internal GameObject[] blueprintSpawnables_legendary;
        #endregion

        #region TARGETTING
        [SerializeField]
        internal List<EntitySystem_Core> machineEntities; //All non-player entities. 
        [SerializeField]
        internal List<EntitySystem_Core> playerEntities; //All player placed entities, excluding players. 



        #endregion

        #region HIVEMINDS AND THREAt
        internal int hivemindsDestroyed;
        internal float universalThreat; //This is used to trigger the "end game" when it's made. Players can essentially battle the 'core' hivemind once this is reached. Players may continue after beating this or end their game here.
        [SerializeField]
        internal float currentThreat;
        [SerializeField]
        internal float baseThreatNeededToTriggerHive;
        internal float currentThreatNeededToTriggerHive;

        #endregion


        void Start()
        {
            foreach (Spawner spawner in spawners)
            {
                if (spawner.isEnabled == true)
                {
                    spawner.timeTillNextSpawn = Random.Range(spawner.minSpawnTimeRange, spawner.maxSpawnTimeRange);
                }
            }
            collectivePlayerLevel = 1;
            experienceNeededToLevel = 100;
        }

        void Update()
        {
            if(gameOver == true)
            {
                return;
            }


            if (currentTotalEntities >= maximumEntities)
            {
                return;
            }

            CheckForLivingPlayers();

            foreach (Spawner spawner in spawners)
            {
                if (spawner.isEnabled == true)
                {
                    spawner.timeTillNextSpawn -= 1 * Time.deltaTime;
                    if (spawner.timeTillNextSpawn <= 0 && spawner.currentSpawned < spawner.maximumSpawn) //If we can spawn and haven't spawned the max of these entities, make another!
                    {
                        spawner.timeTillNextSpawn = Random.Range(spawner.minSpawnTimeRange, spawner.maxSpawnTimeRange); //Reset the spawner timer
                        SpawnEntity(GetNextSpawnPoint(), spawner.identity, spawner);
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            #region CONSTANT CHECKS
            CheckForLevelUpdate();

            #endregion
        }
        #region SPAWNING FUNCTIONS
        private Transform GetNextSpawnPoint() //Calculate our next spawnpoint at random.
        {
            int index = Random.Range(0, orbitSpawnPoints.Count);
            spawnPoint = orbitSpawnPoints[index];
            return spawnPoint;
        }

        private void SpawnEntity(Transform spawnPoint, string identity, Spawner spawner)
        {
            currentTotalEntities += 1; //Log our overall spawn count.
            spawner.currentSpawned += 1; //Log our spawner spawn count.

            GameObject entity = PoolingSystem_Entities.Instance.dictionaryOfPools[identity].Dequeue();
            // entity.GetComponent<EntitySystem_StatusAndStats>().originalSpawner = spawner; NEED TO ADD TO CORE
            entity.gameObject.transform.position = spawnPoint.position;
            entity.GetComponent<EntitySystem_Core>().spawner = spawner;
            entity.gameObject.SetActive(true);

            PoolingSystem_Entities.Instance.dictionaryOfPools[identity].Enqueue(entity);
        }

        internal void TeleportEntity(GameObject entity)
        {
            Transform spawnPoint = GetNextSpawnPoint();
            entity.transform.position = spawnPoint.position;
        }

        internal void AddSpawnPoint(Transform spawnPoint)
        {
            orbitSpawnPoints.Add(spawnPoint);
        }
        #endregion
        #region PLAYER FUNCTIONS
        private void CheckForLevelUpdate()
        {
            if (collectivePlayerExperience >= experienceNeededToLevel)
            {
                UpdateLevels();
            }
        }
        private void UpdateLevels()
        {
            collectivePlayerLevel++;

            experienceNeededToLevel = Mathf.RoundToInt((experienceNeededToLevel + (experienceNeededToLevel * Mathf.Sqrt((collectivePlayerLevel / players.Count) * 10)) / (collectivePlayerLevel / players.Count)));
            collectivePlayerExperience = 0;

            GameSystem_UIManager.Instance.text_playerLevel.text = collectivePlayerLevel.ToString();
            UpdateBaseStatsForPlayers();
        }
        private void UpdateBaseStatsForPlayers()
        {
            foreach(EntitySystem_Core core in players)
            {
                core.UpdatePlayerStats();
            }
        }
        #endregion

        #region FLUX RADIATION
        internal void AddFlux(float amount)
        {
            foreach (EntitySystem_Core player in playerEntities)
            {
                  player.player.currentFlux += amount;
            }
        }
        internal void CalculateNewFluxRequirement()
        {
          //  currentThreatNeededToTriggerHive = baseThreatNeededToTriggerHive * GameSystem_Progression.Instance.currentDifficultyRating;
        }



        #endregion

        #region THREAT AND HIVEMINDS
        internal void AddThreat(float amount)
        {
            universalThreat += amount;
            currentThreat += amount;

            if(currentThreat >= currentThreatNeededToTriggerHive)
            {
                TriggerBoss();
            }
        }

        internal void CaluclateNewThreatLimit()
        {
            //Note: The algo here is super simple but creates a VERY smooth curve. May want to swap out other incrementals with this format!
            currentThreatNeededToTriggerHive = (baseThreatNeededToTriggerHive * Mathf.Sqrt((hivemindsDestroyed * hivemindsDestroyed * hivemindsDestroyed))); //c = b * Sqrt(x^3); 1; 2.8; 5, 8, 11.?, etc.
        }

        private void TriggerBoss()
        {
            Debug.Log("Oh, look, you've triggered a boss. Cool. This'll mean something when this actually works right.");
        }


        #endregion

        #region DEATH CHECKING
        private void CheckForLivingPlayers()
        {
            if(deadPlayers >= totalPlayers || deadPlayers == totalPlayers)
            {
                gameOver = true;
                StartCoroutine(EndGame());
            }
        }

        private IEnumerator EndGame()
        {
            Debug.Log("GAME IS ENDING! ALL PLAYERS ARE DEAD!");
            yield return new WaitForSeconds(3);
            UnityEngine.SceneManagement.SceneManager.LoadScene(endOfGameSceneName);
            yield break;
        }

        #endregion

    }
}