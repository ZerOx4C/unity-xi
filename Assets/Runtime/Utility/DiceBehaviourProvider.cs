using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Behaviour;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Runtime.Utility
{
    public class DiceBehaviourProvider : IDisposable
    {
        private readonly Stack<DiceBehaviour> _allBehaviours = new();
        private readonly Instantiator.Config<DiceBehaviour> _behaviourInstantiator;
        private readonly Stack<DiceBehaviour> _idleBehaviours = new();
        private readonly Transform _root;

        [Inject]
        public DiceBehaviourProvider(DiceBehaviour diceBehaviourPrefab)
        {
            _root = new GameObject("Dices").transform;

            _behaviourInstantiator = Instantiator.Create(diceBehaviourPrefab)
                .SetParent(_root);
        }

        public void Dispose()
        {
            _idleBehaviours.Clear();

            if (_root)
            {
                Object.Destroy(_root.gameObject);
            }
        }

        public async UniTask WarmupAsync(int count, CancellationToken cancellation)
        {
            var diceBehaviours = await _behaviourInstantiator
                .SetTransforms(count, Vector3.zero, Quaternion.identity)
                .InstantiateAsync(cancellation).All;

            foreach (var diceBehaviour in diceBehaviours)
            {
                diceBehaviour.gameObject.SetActive(false);
                _idleBehaviours.Push(diceBehaviour);
                _allBehaviours.Push(diceBehaviour);
            }
        }

        public DiceBehaviour Obtain()
        {
            if (!_idleBehaviours.TryPop(out var diceBehaviour))
            {
                return _behaviourInstantiator.Instantiate().First;
            }

            diceBehaviour.gameObject.SetActive(true);
            return diceBehaviour;
        }

        public void Release(DiceBehaviour diceBehaviour)
        {
            diceBehaviour.gameObject.SetActive(false);
            _idleBehaviours.Push(diceBehaviour);
        }

        public void ReleaseAll()
        {
            foreach (var diceBehaviour in _allBehaviours.Except(_idleBehaviours))
            {
                Release(diceBehaviour);
            }
        }
    }
}
