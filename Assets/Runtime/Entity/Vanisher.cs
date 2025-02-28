using System.Collections.Generic;
using System.Linq;

namespace Runtime.Entity
{
    public class Vanisher
    {
        private readonly Field _field;

        public Vanisher(Field field)
        {
            _field = field;
        }

        public void Evaluate(Dice targetDice)
        {
            var targetValue = targetDice.Value.CurrentValue;
            if (targetValue == 1)
            {
                EvaluateOne(targetDice);
                return;
            }

            var matchedDices = new List<Dice>();
            var knownDices = new HashSet<Dice>();
            var diceStack = new Stack<Dice>();
            diceStack.Push(targetDice);

            while (diceStack.TryPop(out var currentDice))
            {
                knownDices.Add(currentDice);

                if (currentDice.Value.CurrentValue != targetValue)
                {
                    continue;
                }

                matchedDices.Add(currentDice);

                _field.GetNeighborDices(currentDice.Position.Value, out var neighborDices);
                foreach (var dice in neighborDices.Except(knownDices))
                {
                    diceStack.Push(dice);
                }
            }

            if (matchedDices.Count < targetValue)
            {
                return;
            }

            foreach (var dice in matchedDices)
            {
                if (dice.Vanishing.CurrentValue)
                {
                    dice.RewindVanish();
                }
                else
                {
                    dice.BeginVanish();
                }
            }

            // TODO: コンボ情報を構築する
        }

        private void EvaluateOne(Dice targetDice)
        {
            _field.GetNeighborDices(targetDice.Position.Value, out var neighborDices);
            if (!neighborDices.Any(d => d.Vanishing.CurrentValue))
            {
                return;
            }

            foreach (var dice in _field.Dices.Where(d => d.Value.CurrentValue == 1))
            {
                if (!dice.Vanishing.CurrentValue)
                {
                    // TODO: 沈まずに消えるようにする
                    dice.BeginVanish();
                }
            }
        }
    }
}
