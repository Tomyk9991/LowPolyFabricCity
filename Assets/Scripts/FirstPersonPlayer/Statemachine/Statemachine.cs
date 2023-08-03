using System.Collections;
using System.Collections.Generic;

namespace FirstPersonPlayer.Statemachine
{
    public abstract class Statemachine
    {
        public abstract void OnUpdate();
        public abstract void OnDisable();
        public bool RequestExit { get; protected set; }
    }

    public class PlayerStatemachineManager : IEnumerable<Statemachine>
    {
        private readonly List<Statemachine> currentStates = new();
        public int Length => currentStates.Count;

        public Statemachine this[int index] => currentStates[index];
        
        public void AddState(Statemachine state)
        {
            currentStates.Add(state);
        }
        
        public void RemoveState(Statemachine state)
        {
            currentStates.Remove(state);
        }

        public IEnumerator<Statemachine> GetEnumerator()
        {
            return currentStates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return currentStates.GetEnumerator();
        }
    }
}