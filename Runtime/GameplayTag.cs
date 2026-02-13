using System;
using DatastoresDX.Runtime;
using UnityEngine;

namespace GameplayTags.Runtime
{
    [Serializable]
    public class GameplayTag
    {
        [SerializeField]
        private Uid m_tagId = Uid.Invalid;

        public Uid TagId => m_tagId;
        public bool IsInvalid => m_tagId.IsInvalid();
        
        public GameplayTag() { }
        public GameplayTag(Uid tagId)
        {
            m_tagId = tagId;
        }

        public GameplayTagData GetData()
        {
            return (GameplayTagData)Datastores.GetElement(m_tagId);
        }

        public bool IsChildOf(GameplayTag other)
        {
            return GameplayTagCollection.IsChildTagOf(this, other);
        }

        public override bool Equals(object obj)
        {
            if (obj is GameplayTag other)
            {
                return m_tagId.Equals(other.TagId);
            }
            else if (obj is GameplayTagData otherTag)
            {
                return m_tagId.Equals(otherTag.Id);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_tagId.GetHashCode();
        }

#if UNITY_EDITOR
        public static string TagId_VarName = "m_tagId";
#endif
    }
}