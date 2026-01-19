using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = null;
    public bool IsPerforming { get; private set; } = false;

    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();
    private static Dictionary<Delegate, Action<GameAction>> wrapperLookup = new();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // FIXED: Do NOT clear static dictionaries (performers/subs) here.
        // This was causing persistent systems like CurrencySystem to lose their registration.
        // preSubs.Clear();
        // postSubs.Clear();
        // performers.Clear();
        // wrapperLookup.Clear();
        
        IsPerforming = false;
        reactions = null;
    }

    public void Perform(GameAction action, System.Action OnPerformFinished = null)
    {
        if (IsPerforming) return;
        IsPerforming = true;
        StartCoroutine(Flow(action, () =>
        {
            IsPerforming = false;
            OnPerformFinished?.Invoke();
        }));
    }

    public void AddReaction(GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }

    private void Start()
    {
        // FORCE INITIALIZATION of persistent systems that need to register performers.
        // If testing in a scene where CurrencySystem isn't referenced, it won't wake up 
        // and won't register GiveGoldGA, causing "No performer found".
        var ensureCurrency = CurrencySystem.Instance;
        var ensureArmor = ArmorSystem.Instance;
        
        // Add other critical systems here if needed
    }

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        Debug.Log($"[ActionSystem] Processing {action.GetType().Name}");

        reactions = action.PreReactions;
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        yield return PerformPerformer(action);
        yield return PerformReactions();

        reactions = action.PostReactions;
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        OnFlowFinished?.Invoke();
    }

    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType();
        if (performers.ContainsKey(type))
        {
            Debug.Log($"[ActionSystem] Invoking performer for {type.Name}");
            yield return performers[type](action);
        }
        else
        {
            Debug.LogWarning($"[ActionSystem] No performer found for {type.Name}!");
            Debug.Log($"[ActionSystem] Registered performers: {string.Join(", ", performers.Keys)}");
            
            // Allow debugging of empty actions
            if (action.PreReactions.Count == 0 && action.PerformReactions.Count == 0 && action.PostReactions.Count == 0)
            {
               // Just silent if it's truly empty?
            }
        }
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();
        if (subs.ContainsKey(type))
        {
            var subsCopy = new List<Action<GameAction>>(subs[type]);
            foreach (var sub in subsCopy)
            {
                sub(action);
            }
        }
    }

    private IEnumerator PerformReactions()
    {
        if (reactions != null && reactions.Count > 0)
        {
            var reactionsCopy = new List<GameAction>(reactions);
            foreach (var reaction in reactionsCopy)
            {
                // NOTE: This recursion technically overwrites the global 'reactions' field,
                // but this represents the legacy behavior that worked for the project.
                yield return Flow(reaction);
            }
        }
    }

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);
        
        if (performers.ContainsKey(type)) performers[type] = wrappedPerformer;
        else performers.Add(type, wrappedPerformer);
        
        Debug.Log($"[ActionSystem] Attached performer for {type.Name}");
    }

    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type))
        {
            performers.Remove(type);
        }
    }

    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        if (wrapperLookup.ContainsKey(reaction)) return;

        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        
        Action<GameAction> wrappedReaction = (GameAction action) => reaction((T)action);
        wrapperLookup.Add(reaction, wrappedReaction);

        if (!subs.ContainsKey(typeof(T)))
        {
            subs.Add(typeof(T), new List<Action<GameAction>>());
        }
        
        subs[typeof(T)].Add(wrappedReaction);
    }

    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        if (wrapperLookup.TryGetValue(reaction, out Action<GameAction> wrappedReaction))
        {
            Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;

            if (subs.ContainsKey(typeof(T)))
            {
                subs[typeof(T)].Remove(wrappedReaction);
            }

            wrapperLookup.Remove(reaction);
        }
    }
}