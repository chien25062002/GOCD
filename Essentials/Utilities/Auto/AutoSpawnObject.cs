using UnityEngine;

namespace GOCD.Framework
{
    public class AutoSpawnObject : MonoBehaviour
    {
        [SerializeField] GameObject _prefab;
        [SerializeField] float _spawnTime = 4f;
        [SerializeField] bool _spawnAtStart = true;

         GameObject _currentItem;

         float _timer;

         void Awake()
         {
             _timer = _spawnTime;
             
             if (_spawnAtStart)
             {
                 SpawnTheItem();
             }
         }

         void FixedUpdate()
        {
            if (!_currentItem)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    _timer = _spawnTime;
                    SpawnTheItem();
                }
            }
        }

        public void SpawnTheItem()
        {
            if (_currentItem)
            {
                Destroy(_currentItem);
            }

            if (_prefab)
            {
                _currentItem = Instantiate(_prefab, transform.position, transform.rotation);
            }
            else
            {
                Debug.LogWarning("Please add a item to spawn.");
            }
        }
    }
}
