using System.Collections.Generic;

public interface IHaveTargets
{
    List<CombatantView> Targets { get; }
}
