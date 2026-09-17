using System;
using UnityEngine;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Levels
{
    /// <summary>
    /// Data-driven definition for a single playable level.
    /// Follows Master Build Prompt Section 9, Section 10, and Phase 19.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData_XX", menuName = "RoyalSmash/Levels/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int _levelId = 1;
        [SerializeField] private string _levelName = "Level 1";

        [Header("Gameplay Ammunition")]
        [SerializeField] private int _ballCount = 25;
        [SerializeField] private int _baseRewardGold = 50;

        [Header("Stand Configuration")]
        [SerializeField] private StandPlacement[] _stands;

        [Header("Target Objects")]
        [SerializeField] private ObjectPlacement[] _objects;

        [Header("Milestone & Progression Events")]
        [SerializeField] private LevelEvent[] _specialEvents;

        [Header("Tutorial Details")]
        [SerializeField] private bool _isTutorialLevel = false;
        [SerializeField] private Vector3 _tutorialTargetPosition;
        [SerializeField] private float _tutorialHighlightRadius = 1f;

        public int LevelId => _levelId;
        public string LevelName => _levelName;
        public int BallCount => _ballCount;
        public int BaseRewardGold => _baseRewardGold;
        public StandPlacement[] Stands => _stands;
        public ObjectPlacement[] Objects => _objects;
        public LevelEvent[] SpecialEvents => _specialEvents;
        public bool IsTutorialLevel => _isTutorialLevel;
        public Vector3 TutorialTargetPosition => _tutorialTargetPosition;
        public float TutorialHighlightRadius => _tutorialHighlightRadius;

        public void SetData(int id, string name, int balls, int reward, StandPlacement[] stands, ObjectPlacement[] objs, LevelEvent[] events)
        {
            _levelId = id;
            _levelName = name;
            _ballCount = balls;
            _baseRewardGold = reward;
            _stands = stands;
            _objects = objs;
            _specialEvents = events;
        }
    }

    [Serializable]
    public class StandPlacement
    {
        public Vector3 Position = new Vector3(0, 1.0f, 5.0f);
        public Vector3 Scale = new Vector3(4.0f, 0.3f, 2.5f);
        public Vector3 Rotation = Vector3.zero;
    }

    [Serializable]
    public class ObjectPlacement
    {
        public ObjectType Type;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;

        public bool OverrideMass = false;
        public float CustomMass = 1.0f;
    }

    [Serializable]
    public class LevelEvent
    {
        public LevelEventType Type;
        public string Data;
    }

    public enum LevelEventType
    {
        None,
        UnlockMegaBall,         // Level 5
        UnlockInfinityBall,     // Level 10
        PromptUsername,         // Level 10
        NewContentIntroduction, // Every 5 levels
        FinalLevel              // Level 30
    }
}
