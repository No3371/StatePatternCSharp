using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BA_Studio.StatePattern
{

	// StateController<T> is used to control any T object.
	// Define and customize your own State<T>.
	// It does not update states by itself, calls Update()/FixedUpdate() when you need.
	// You have to keep the stateController instance of your T, this allows you to easily attach this state pattern to any class.
	public class StateMachine<T> where T : class {

		public bool UnityDebugLog = false;

		public T Target { get; internal set; }

		public State<T> CurrentState { get; internal set; }

		int pauseCount = 0;

		public StateMachine (T owner) {
			// if (!startStateType.IsSubclassOf(typeof(IState<T>))) throw new System.Exception(startStateType.Name + " is not a valid state type for this object!");
			this.Target = owner;
		}

		
		public void Update ()
		{
			if (pauseCount > 0) 
			{
				pauseCount -= 1;
				return;
			}
			if (CurrentState == null) throw new System.Exception("CurrentState is null. Did you set a state after instantiate this controller?");
			if (CurrentState.AllowUpdate) CurrentState.Update();
		}

		public void ChangeState (State<T> nextState, bool instantUpdate = false)
		{
			if (UnityDebugLog) Debug.Log(Target.GetType() + " is changing to: " + nextState.GetType().Name);
			CurrentState?.OnLeaving();
			CurrentState = nextState;
			CurrentState?.OnEntered();
			if (instantUpdate) CurrentState?.Update();
		}

		public void PauseUpdateForFrames (int count = 1)
		{
			pauseCount = count;
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

		/// Shared Objects is objects shared among states.
		/// You should use this to keep objects accessible to every states of the T instance. 
		public T2 GetSharedObject<T2> (string ID, bool strictType = true, bool returnNewDefaultIfTypeError = false)
		{
			if (strictType)
			{
				if (SharedMemory[ID].Item1 == typeof(T2)) return (T2)SharedMemory[ID].Item2;
				else if (returnNewDefaultIfTypeError) return default(T2);
				else throw new System.Exception("StateController<" + typeof(T2).ToString() + ">(" + CurrentState.ToString() + ") Requesting wrong type of shared object.");
			}
			else if (SharedMemory[ID].Item2 is T2) return (T2)SharedMemory[ID].Item2;
			else if (returnNewDefaultIfTypeError) return default(T2);
			else throw new System.Exception("StateController<" + typeof(T2).ToString() + ">(" + CurrentState.ToString() + ") Requesting wrong type of shared object.");
		}

		/// Shared Objects is objects shared among states.
		/// You should use this to keep objects accessible to every states of the T instance. 
		public void SetSharedObject<T2> (string ID, T2 obj)
		{
			if (!SharedMemory.ContainsKey(ID)) SharedMemory.Add(ID, new System.Tuple<System.Type, object>(typeof(T2), obj));
			else SharedMemory[ID] = new System.Tuple<System.Type, object>(typeof(T2), obj);
		}

		public void UnrefSharedObject (string ID)
		{
			SharedMemory.Remove(ID);
		}
	}


}