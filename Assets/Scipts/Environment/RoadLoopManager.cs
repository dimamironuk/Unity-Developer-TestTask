using UnityEngine;
using Zenject;

public class RoadLoopManager : MonoBehaviour
{
    [SerializeField] private Transform[] _tiles;
    [SerializeField] private float _tileLength = 75f;

    private CarController _car;

    [Inject]
    public void Construct(CarController car) => _car = car;

    private void LateUpdate()
    {
        float carZ = _car.transform.position.z;
        float loopLength = _tileLength * _tiles.Length;

        foreach (Transform tile in _tiles)
        {
            if (carZ - tile.position.z > _tileLength * 1.5f)
            {
                tile.position += Vector3.forward * loopLength;
            }
        }
    }
}