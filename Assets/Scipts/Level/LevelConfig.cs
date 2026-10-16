using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Config")]
public class LevelConfig : ScriptableObject
{
    [field: SerializeField] public float Length { get; private set; } = 150f;
    [field: SerializeField] public int EnemyCount { get; private set; } = 40;
    [field: SerializeField] public int CoinsPerKill { get; private set; } = 5;
    [field: SerializeField] public float FirstEnemyDistance { get; private set; } = 25f;
    [field: SerializeField] public float EnemyMinX { get; private set; } = -4f;
    [field: SerializeField] public float EnemyMaxX { get; private set; } = 4f;
    [field: SerializeField] public float CarSpeed { get; private set; } = 8f;
    [field: SerializeField] public float CarAcceleration { get; private set; } = 10f;
}