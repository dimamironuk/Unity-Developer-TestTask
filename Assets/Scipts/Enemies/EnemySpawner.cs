using System;
using UnityEngine;

public class EnemySpawner
{
    private readonly EnemyController.Pool _pool;
    private readonly CarController _car;
    private readonly LevelConfig _config;

    public event Action EnemyKilled;

    public int TotalCount => _config.EnemyCount;

    public EnemySpawner(EnemyController.Pool pool, CarController car, LevelConfig config)
    {
        _pool = pool;
        _car = car;
        _config = config;
    }

    public void SpawnEnemies()
    {
        Vector3 carPosition = _car.transform.position;
        Quaternion rotation = Quaternion.LookRotation(Vector3.back);

        float startZ = carPosition.z + _config.FirstEnemyDistance;
        float segment = (_config.Length - _config.FirstEnemyDistance) / _config.EnemyCount;

        for (int i = 0; i < _config.EnemyCount; i++)
        {
            float x = carPosition.x + UnityEngine.Random.Range(_config.EnemyMinX, _config.EnemyMaxX);
            float z = startZ + segment * (i + UnityEngine.Random.value);

            EnemyController enemy = _pool.Spawn(new Vector3(x, carPosition.y, z), rotation);
            enemy.Died += () => EnemyKilled?.Invoke();
        }
    }
}