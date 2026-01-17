using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; // Necesario para MonoBehaviour y OnDestroy

public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = null;
    public bool IsPerforming { get; private set; } = false;

    // Listas de suscriptores
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

    // --- NUEVO: Diccionario para rastrear las envolturas (wrappers) ---
    // Esto conecta tu función original 'reaction' con la 'wrapper' genérica para poder borrarla después.
    private static Dictionary<Delegate, Action<GameAction>> wrapperLookup = new();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // Limpieza vital para evitar errores al reiniciar la escena o el juego
        preSubs.Clear();
        postSubs.Clear();
        performers.Clear();
        wrapperLookup.Clear();
        IsPerforming = false;
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

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
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
            yield return performers[type](action);
        }
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();
        if (subs.ContainsKey(type))
        {
            // Importante: Usamos una copia de la lista para iterar.
            // Si una reacción se desuscribe a sí misma durante la ejecución,
            // iterar sobre la lista original daría error.
            var subsCopy = new List<Action<GameAction>>(subs[type]);
            foreach (var sub in subsCopy)
            {
                sub(action);
            }
        }
    }

    private IEnumerator PerformReactions()
    {
        // Igual que arriba, iterar sobre una lista que puede cambiar es peligroso.
        // Si reactions es null, no hacemos nada.
        if (reactions != null && reactions.Count > 0)
        {
            // Procesamos las reacciones en orden (FIFO)
            // Nota: Si una reacción añade nuevas reacciones a ESTA misma lista,
            // necesitarías un bucle while o for inverso. Por ahora lo dejo como estaba.
            var reactionsCopy = new List<GameAction>(reactions);
            foreach (var reaction in reactionsCopy)
            {
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
    }

    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type)) performers.Remove(type);
    }

    // --- MÉTODOS DE SUSCRIPCIÓN CORREGIDOS ---

    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        // Evitar doble suscripción
        if (wrapperLookup.ContainsKey(reaction)) return;

        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        
        // Creamos el wrapper y lo guardamos en el diccionario de lookup
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
        // 1. Buscamos si existe un wrapper para esta función original
        if (wrapperLookup.TryGetValue(reaction, out Action<GameAction> wrappedReaction))
        {
            Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;

            // 2. Si existe la lista de suscriptores para ese tipo, borramos el wrapper
            if (subs.ContainsKey(typeof(T)))
            {
                subs[typeof(T)].Remove(wrappedReaction);
            }

            // 3. Borramos la referencia del lookup para mantener limpio el diccionario
            wrapperLookup.Remove(reaction);
        }
    }
}