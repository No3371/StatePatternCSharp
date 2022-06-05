using System;

namespace BAStudio.StatePattern
{
    public partial class StateMachine<T>
    {
        public abstract class ObserverTransitionState<H, FROM, TO> : State, IObserver<H> where FROM : State where TO : State, new()
        {
            IDisposable handle;
            TO instancedNext;
            object parameter;

			public ObserverTransitionState(IObservable<H> observable, TO instance = null, object parameter = null)
            {
                handle = observable.Subscribe(this);
                instancedNext = instance;
                this.parameter = parameter;
            }

            public override void OnEntered(StateMachine<T> machine, State previous, T context, object parameter = null)
            {
                if (typeof(FROM) != previous.GetType()) throw new InvalidOperationException("Transition source state mismatch");

                // Reset
                isCompleted = false;
                exception = null;
                progress = default(H);
            }

            public override void OnLeaving(StateMachine<T> machine, State next, T context, object parameter = null)
            {
                handle.Dispose();
            }

            public override void Update(StateMachine<T> machine, T context)
            {
                if (isCompleted)
                {
                    if (instancedNext != null) machine.ChangeState(instancedNext, parameter);
                    else machine.ChangeState<TO>(parameter);
                }
                else if (exception != null) { /* DEBUG OUTPUT */ }
            }

            bool isCompleted;
            Exception exception;
            H progress;

            public virtual void OnCompleted()
            {
                isCompleted = true; // Because we don't know which thread it is
            }

            public virtual void OnNext(H value)
            {
                progress = value;
            }

            public virtual void OnError(Exception error)
            {
                exception = error;
            }
        }
    }
}