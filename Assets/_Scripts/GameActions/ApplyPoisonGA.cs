public class ApplyPoisonGA : GameAction
{
    public CombatantView Target { get; private set; }

    public ApplyPoisonGA(CombatantView target)
    {
        Target = target;
    }
}
