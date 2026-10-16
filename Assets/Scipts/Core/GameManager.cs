using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GameManager : IInitializable, ITickable, IDisposable
{
    private enum State
    {
        WaitingStart,
        Playing,
        Paused,
        Won,
        Lost
    }

    private const float RestartDelay = 0.8f;

    private readonly LevelConfig _config;
    private readonly UIManager _ui;
    private readonly CarController _car;
    private readonly TurretController _turret;
    private readonly EnemySpawner _spawner;
    private readonly IInputService _input;
    private readonly Health _carHealth;

    private State _state;
    private float _endTime;

    public GameManager(
        LevelConfig config,
        UIManager ui,
        CarController car,
        TurretController turret,
        EnemySpawner spawner,
        IInputService input,
        [Inject(Id = GameInstaller.CarHealthId)] Health carHealth)
    {
        _config = config;
        _ui = ui;
        _car = car;
        _turret = turret;
        _spawner = spawner;
        _input = input;
        _carHealth = carHealth;
    }

    public void Initialize()
    {
        _spawner.SpawnEnemies();
        _spawner.EnemyKilled += HandleEnemyKilled;
        _carHealth.OnDied += HandleCarDied;
        _ui.PauseClicked += HandlePause;
        _ui.ResumeClicked += HandleResume;
        _ui.RestartClicked += Restart;
        _ui.NextLevelClicked += NextLevel; 

        _turret.SetCanShoot(false);
        _ui.ShowStart(LevelProgress.Number, Wallet.Coins);
    }

    public void Dispose()
    {
        Time.timeScale = 1f;

        _spawner.EnemyKilled -= HandleEnemyKilled;
        _carHealth.OnDied -= HandleCarDied;
        _ui.PauseClicked -= HandlePause;
        _ui.ResumeClicked -= HandleResume;
        _ui.RestartClicked -= Restart;
        _ui.NextLevelClicked -= NextLevel; 
    }

    public void Tick()
    {
        switch (_state)
        {
            case State.WaitingStart:
                if (_input.WasTapped) StartGame();
                break;
            case State.Playing:
                UpdateProgress();
                break;
            case State.Won:
            case State.Lost:
                if (_input.WasTapped && Time.unscaledTime - _endTime >= RestartDelay)
                {
                    if (_state == State.Won) NextLevel();
                    else Restart();
                }
                break;
        }
    }

    private void StartGame()
    {
        _state = State.Playing;
        _car.StartMoving();
        _turret.SetCanShoot(true);
        _ui.ShowGameplay();
    }

    private void UpdateProgress()
    {
        float progress = Mathf.Clamp01(_car.DistanceTraveled / _config.Length);
        _ui.UpdateProgress(progress);

        if (progress >= 1f)
        {
            Finish(State.Won);
        }
    }

    private void HandlePause()
    {
        if (_state == State.Playing) SetPaused(true);
    }

    private void HandleResume()
    {
        if (_state == State.Paused) SetPaused(false);
    }

    private void SetPaused(bool isPaused)
    {
        _state = isPaused ? State.Paused : State.Playing;
        Time.timeScale = isPaused ? 0f : 1f;
        _turret.SetCanShoot(!isPaused);
        _ui.SetPaused(isPaused);
    }

    private void HandleEnemyKilled()
    {
        if (_state != State.Playing) return;

        Wallet.Add(_config.CoinsPerKill);
        _ui.UpdateCoins(Wallet.Coins);
    }

    private void HandleCarDied()
    {
        if (_state == State.Playing)
        {
            Finish(State.Lost);
        }
    }

    private void Finish(State result)
    {
        _state = result;
        _endTime = Time.unscaledTime;
        _car.StopMoving();
        _turret.SetCanShoot(false);
        _ui.ShowResult(result == State.Won);

        if (result == State.Won)
        {
            LevelProgress.Advance(); 
        }
    }

    private void NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}