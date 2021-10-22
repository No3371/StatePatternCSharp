namespace BAStudio.StatePattern.Example
{
    public abstract partial class Movement
    {
        struct JumpParameter : IStateParameter<Movement>
        {
            public float JumpMultiplier { get; set; }
        }
    }

}