using UnityEngine;
using System;
using System.Collections.Generic;
using SerializeReferenceEditor;

[Serializable]
public class MapEventChoice
{
    public string ChoiceLabel;
    [SerializeField] public Sprite ChoiceIcon;
    [TextArea] public string ResultDescription;
    [SerializeReference, SR] public List<MapEventOutcome> Outcomes = new();
}

[CreateAssetMenu(fileName = "NewMapEvent", menuName = "Data/Map/Event")]
public class MapEventData : ScriptableObject
{
    public string EventTitle;
    [TextArea] public string EventDescription;
    public Sprite Illustration;
    public List<MapEventChoice> Choices = new();
}
