using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTreeGraph.Core
{
    public interface ISkillContext
    {
        GameObject PlayerRoot { get; }
        T GetSystem<T>() where T : class;
    }

    public class SkillContext : ISkillContext
    {
        public GameObject PlayerRoot { get; private set; }
        private readonly Dictionary<Type, object> _systems = new();

        public SkillContext(GameObject player)
        {
            PlayerRoot = player;

            // Cache System here. for example :
            // _systems[typeof(PlayerStats)] = player.GetComponent<PlayerStats>();
            // _systems[typeof(IInventory)] = player.GetComponent<IInventory>();
            // _systems[typeof(IAbilityController)] = player.GetComponent<IAbilityController>();
            // Etc.
        }

        public T GetSystem<T>() where T : class
        {
            if (_systems.TryGetValue(typeof(T), out var system))
                return system as T;

            Debug.LogError($"System {typeof(T).Name} is missing from the Player!");
            return null;
        }
    }
}