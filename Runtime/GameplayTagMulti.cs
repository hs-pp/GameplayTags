using System;
using System.Collections.Generic;
using DatastoresDX.Runtime;
using UnityEngine;

namespace GameplayTags.Runtime
{
    [Serializable]
    public class GameplayTagMulti
    {
        [SerializeField]
        private List<Uid> m_tags = new();
        
        public void AddTag(GameplayTag tag)
        {
            m_tags.Add(tag.TagId);
        }
        
        public void RemoveTag(GameplayTag tag)
        {
            m_tags.Remove(tag.TagId);
        }
        
        public bool ContainsTag(GameplayTag tag)
        {
            return m_tags.Contains(tag.TagId);
        }

        public bool IsSubsetOf(GameplayTagMulti other)
        {
            foreach (Uid tagId in m_tags)
            {
                if (!other.m_tags.Contains(tagId))
                {
                    return false;
                }
            }

            return true;
        }
        
        public bool IsIdenticalTo(GameplayTagMulti other)
        {
            return (m_tags.Count == other.m_tags.Count) && IsSubsetOf(other);
        }

        public List<GameplayTag> GetTags()
        {
            List<GameplayTag> tags = new();
            foreach (Uid tagId in m_tags)
            {
                tags.Add(new GameplayTag(tagId));
            }

            return tags;
        }
    }
}