using System.Collections.Generic;
using Runtime.Behaviour;
using Runtime.Entity;
using UnityEngine.Assertions;
using VContainer;

namespace Runtime.Utility
{
    public class DiceBehaviourRepository
    {
        private readonly Dictionary<DiceBehaviour, Dice> _behaviourToDiceTable = new();
        private readonly Dictionary<Dice, DiceBehaviour> _diceToBehaviourTable = new();

        [Inject]
        public DiceBehaviourRepository()
        {
        }

        public void Add(Dice dice, DiceBehaviour behaviour)
        {
            Assert.IsFalse(_behaviourToDiceTable.ContainsKey(behaviour));
            Assert.IsFalse(_diceToBehaviourTable.ContainsKey(dice));

            _behaviourToDiceTable.Add(behaviour, dice);
            _diceToBehaviourTable.Add(dice, behaviour);
        }

        public bool Remove(Dice dice, out DiceBehaviour behaviour)
        {
            if (!_diceToBehaviourTable.Remove(dice, out behaviour))
            {
                return false;
            }

            _behaviourToDiceTable.Remove(behaviour);
            return true;
        }

        public bool TryResolve(DiceBehaviour behaviour, out Dice dice)
        {
            return _behaviourToDiceTable.TryGetValue(behaviour, out dice);
        }

        public bool TryResolve(Dice dice, out DiceBehaviour behaviour)
        {
            return _diceToBehaviourTable.TryGetValue(dice, out behaviour);
        }
    }
}
