using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public const string CarHealthId = "CarHealth";

    [Header("Scene References")]
    [SerializeField] private LevelConfig[] _levels;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private CarController _carController;
    [SerializeField] private TurretController _turretController;
    [SerializeField] private CameraController _cameraFollow;
    [SerializeField] private Health _carHealth;

    [Header("Prefabs")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private EnemyController _enemyPrefab;

    [Header("Pool")]
    [SerializeField] private int _bulletPoolSize = 40;

    public override void InstallBindings()
    {
        LevelConfig config = _levels[LevelProgress.Index % _levels.Length];

        Container.BindInstance(config);
        Container.BindInstance(_uiManager);
        Container.BindInstance(_carController);
        Container.BindInstance(_turretController);
        Container.BindInstance(_cameraFollow);
        Container.BindInstance(_carHealth).WithId(CarHealthId);

        Container.Bind<IInputService>().To<InputService>().AsSingle();
        Container.Bind<EnemySpawner>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameManager>().AsSingle();

        Container.BindMemoryPool<Bullet, Bullet.Pool>()
            .WithInitialSize(_bulletPoolSize)
            .FromComponentInNewPrefab(_bulletPrefab)
            .UnderTransformGroup("BulletPool");

        Container.BindMemoryPool<EnemyController, EnemyController.Pool>()
            .WithInitialSize(config.EnemyCount)
            .FromComponentInNewPrefab(_enemyPrefab)
            .UnderTransformGroup("EnemyPool");
    }
}