namespace BAStudio.StatePattern.Example.Game
{
    public partial class Game
    {
        public partial class InMainMenu : StateMachine<Game>.State,
                                          IEventReceiverState<Game, LoadGameRequest>,
                                          IEventReceiverState<Game, InMainMenu.Interaction>
        {
            public override void OnEntered(StateMachine<Game> machine, StateMachine<Game>.State previous, Game context, object parameter = null)
            {
                // A fake MainMenu. In realworld, is usually a UI object created. For example, in Unity, a MainMenu prefab should be loaded before we proceed.
                // SingletonLocator.SingletonLocator<MainMenu>.Instance = new MainMenu();
                // SingletonLocator.SingletonLocator<MainMenu>.Instance.Show();
            }

            public override void OnLeaving(StateMachine<Game> machine, StateMachine<Game>.State next, Game context, object parameter = null) {}

            /// <summary>
            /// In MainMenu, we only wait for user selecting main menu buttons.
            /// </summary>
            public override void Update(StateMachine<Game> machine, Game context) {}

            public void ReceiveEvent(StateMachine<Game> machine, Game context, LoadGameRequest ev)
            {
                machine.ChangeState<LoadGame>(ev);
            }

            public void ReceiveEvent(StateMachine<Game> machine, Game context, InMainMenu.Interaction ev)
            {
                switch (ev)
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