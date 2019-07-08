using System.Collections;
using System.Collections.Generic;

namespace BA_Studio.StatePattern
{

	// StateController<T> is used to control any T object.
	// Define and customize your own State<T>.
	// It does not update states by itself, calls Update()/FixedUpdate() when you need.
	// You have to keep the stateController instance of your T, this allows you to easily attach this state pattern to any class.
	public class StateMachine<T> where T : class
	{

		public System.Action<string> debugLogOutput;

		public T Target { get; internal set; }

		public State<T> CurrentState { get; internal set; }

		public StateMachine (T owner)
		{
			this.Target = owner;
		}

		public System.Action<State<T>, State<T>> OnStateChanging, OnStateChanged;

		bool stateChanging;
		
		public void Update ()
		{
			if (Target == null) throw new System.NullReferenceException("Target is null. Did you destroy it?");
			if (CurrentState == null) throw new System.NullReferenceException("CurrentState is null. Did you set a state after instantiate this controller?");
			if (CurrentState.AllowUpdate) CurrentState.Update();
		}

		State<T> cache;

		public void ChangeState (State<T> nextState, bool instantUpdate = false)
		{
			cache = CurrentState;
			StateChangeStart(CurrentState, nextState);
			CurrentState = nextState;
			CurrentState?.OnEntered();
			StateChanged(cache, instantUpdate);
			cache = null;
		}

		public void ChangeState (State<T> nextState, StateParam<T> data, bool instantUpdate = false)
		{
			cache = CurrentState;
			StateChangeStart(CurrentState, nextState);
			CurrentState = nextState;
			CurrentState?.OnEntered(data);
			StateChanged(cache, instantUpdate);
			cache = null;
		}

		public void ChangeState<U>(StateParam<T> data, bool instantUpdate = false) where U : State<T>, new()
		{
			ChangeState(new U(), data, instantUpdate);
		}

		void StateChangeStart (State<T> fromState, State<T> toState)
		{			
			stateChanging = true;
			debugLogOutput?.Invoke("StateMachine<" + Target.GetType().Name + "> is switching to: " + toState.GetType().Name);
			CurrentState?.OnLeaving();
			OnStateChanging?.Invoke(fromState, toState);
		}

		void StateChanged (State<T> fromState, bool instantUpdate = false)
		{
			stateChanging = false;
			OnStateChanged?.Invoke(fromState, CurrentState);
			if (instantUpdate) CurrentState?.Update();
		}

		Dictionary<string, System.Tuple<System.Type, object>> sharedMemory;
		Dictionary<string, System.Tuple<System.Type, object>> SharedMemory
		{
			get
			{
				if (sharedMemory == null) sharedMemory = new Dictionary<string, System.Tuple<System.Type, object>>();
				return sharedMemory;
			}
		}

        public bool ContainsSharedObject (string ID)
		{
			return SharedMemory.ContainsKey(ID);
		}

		/// <param name="ID">The ID of the data.</param>
		/// <param name="checkLabeldType">Should check T2 is exactly the same T2 used when the data is set with SetSharedObject(). If false, it use *is* operator, which check if the saved object can be converted to T2, then return the data as T2.</param>
		/// <typeparam name="T2">Anything.</typeparam>
		public T2 GetSharedObject<T2> (string ID, bool checkLabeldType = true)
		{
			if (checkLabeldType)
			{
				if (SharedMemory[ID].Item1 == typeof(T2)) return (T2)SharedMemory[ID].Item2;
				else throw new System.ArgumentException ("StateController<" + Target.GetType().ToString() + ">(" + CurrentState.ToString() + ") Requesting wrong type of shared object.");
			}
			else if (SharedMemory[ID].Item2 is T2) return (T2)SharedMemory[ID].Item2;
			else throw new System.ArgumentException ("StateController<" + Target.GetType().ToString() + ">(" + CurrentState.ToString() + ") Requesting wrong type of shared object.");
		}

		public T2 GetSharedObjectOrDefault<T2>  (string ID, bool strictType = true, T2 defaultReturn = default(T2))
		{
			if (!SharedMemory.ContainsKey(ID)) return defaultReturn;
			return GetSharedObject<T2>(ID, strictType);
		}

		public T2 GetSharedObjectOrNull<T2> (string ID, bool strictType = true, bool returnNewDefaultIfTypeError = false) where T2 : class
		{
			if (!SharedMemory.ContainsKey(ID)) return null;
			return GetSharedObject<T2>(ID, strictType);
		}

		public System.Nullable<T2> GetSharedObjectOrNullable<T2> (string ID, bool strictType = true, bool returnNewDefaultIfTypeError = false) where T2 : struct
		{
			if (!SharedMemory.ContainsKey(ID)) return null;
			return GetSharedObject<T2>(ID, strictType);
		}

		/// <summary> 
		/// Shared Objects are stored in StateMachine, it's available to all State belongs to the machine.
		/// You should Get/SetSharedObject this to access data that:
		/// - Is a internal data of Context which will never be directly exposed
		/// - Is created by states' behvior, and should be used by multiple states
		/// - Accessible to all states of the T instance. 
		/// </summary>
		public void SetSharedObject<T2> (string ID, T2 obj)
		{
			if (!SharedMemory.ContainsKey(ID)) SharedMemory.Add(ID, new System.Tuple<System.Type, object>(typeof(T2), obj));
			else SharedMemory[ID] = new System.Tuple<System.Type, object>(typeof(T2), obj);
		}

		public void UnrefSharedObject (string ID)
		{
			SharedMemory.Remove(ID);
		}

		/// Only would be passed when CurrentState is designed to receive the passed type
		public void PassEvent (StateParam<T> data)
		{
			if (CurrentState == null) return;
			if (stateChanging) debugLogOutput?.Invoke("[StateMachine<" + typeof(T).Name + ">] Warning: Event passed in while ChangeState() in progress.");
			if (CurrentState.EventListener == null) return;
			else for (int i = 0; i < CurrentState.EventListener.Length; i++) if (CurrentState.EventListener[i] == data.GetType()) CurrentState.EventReceived(data); 
		}

		Dictionary<Type, State<T>> stateCache;
        public Dictionary<Type, State<T>> StateCache { get => stateCache = stateCache ?? new Dictionary<Type, State<T>>(); set => stateCache = value; }

		public U GetExclusiveState<U>(StateParam<T> data) where U : State<T>
		{
			U state ;
			if (!StateCache.ContainsKey(typeof(U)))
			{
				state = new U();
				StateCache.Add(typeof(U), state);
			}
			else
			{
				state = StateCache[typeof(U)];
			}
			state.EventReceived(data);
			return state;
			
		}
	}

}