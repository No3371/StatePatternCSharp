namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public partial class InMainMenu : StateMachine<Game>.State
        {
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, IStateParameter<Game> parameter = null)
            {
                // A fake MainMenu. In realworld, is usually a UI object created. For example, in Unity, a MainMenu prefab should be loaded before we proceed.
                // SingletonLocator.SingletonLocator<MainMenu>.Instance = new MainMenu();
                // SingletonLocator.SingletonLocator<MainMenu>.Instance.Show();
            }

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game context) {}

            /// <summary>
            /// In MainMenu, we only wait for user selecting main menu buttons.
            /// </summary>
            public override void Update(StateMachine<Game> machine, Game context) {}

            public override void ReceiveEvent(StateMachine<Game> machine, IStateEvent<Game> stateEvent, Game context)
            {
                switch (stateEvent)
                {
                    case Interacted e:
                    {
                        switch (e.Interaction)
                        {
                            case Interaction.NewGame:
                                machine.ChangeState<NewGame>();
                                break;
                            case Interaction.Load:
                                // SingletonLocator.SingletonLocator<MainMenu>.Instance.ShowLoadSaveUI();
                                break;
                            case Interaction.Exit:
                                context.ExitGame();
                                break;
                        }
                        break;
                    }
                    case LoadGameRequest e:
                    {
                        machine.ChangeState<LoadGame>(e);
                        break;
                    }
                }
            }

            public enum Interaction
            {
                NewGame,
                Load,
                Exit
            }
        }
    }
}