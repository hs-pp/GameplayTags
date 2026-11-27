using System.Collections.Generic;
using DatastoresDX.Runtime;
using DatastoresDX.Runtime.DataCollections;
using UnityEngine;

namespace GameplayTags.Runtime
{
    // Folders aren't tags, so a folder would break the parent chain IsChildTagOf walks.
    [DataCollection(true, true, "Gameplay Tags", allowFolders: false)]
    public class GameplayTagCollection : DataCollection, ISerializationCallbackReceiver
    {
        private Dictionary<Uid, GameplayTagTreeNode> m_tagTreeNodes;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        /// <summary>
        /// Any deserialize (editor apply, undo, reimport) can add, remove or move tags, so rebuild the tree on next use.
        /// The asset outlives Play Mode now that there's no domain reload so nothing else would reset it.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_tagTreeNodes = null;
        }

        private Dictionary<Uid, GameplayTagTreeNode> GenerateTreeNodes()
        {
            Dictionary<Uid, GameplayTagTreeNode> treeNodes = new();
            for (int i = 0; i < m_elements.Count; i++)
            {
                GameplayTagData tag = m_elements[i] as GameplayTagData;
                if (tag == null)
                {
                    continue;
                }
                TreeViewMeta meta = m_treeViewMetas[i];
                if (!treeNodes.ContainsKey(tag.Id))
                {
                    treeNodes.Add(tag.Id, new GameplayTagTreeNode());
                }

                // A child listed earlier may have already added this node as its parent, without a tag.
                GameplayTagTreeNode node = treeNodes[tag.Id];
                node.Tag = tag;
                if (!treeNodes.ContainsKey(meta.ParentId))
                {
                    treeNodes.Add(meta.ParentId, new GameplayTagTreeNode());
                }

                node.Parent = treeNodes[meta.ParentId];

                if (node.Parent.Children == null)
                {
                    node.Parent.Children = new List<GameplayTagTreeNode>();
                }

                node.Parent.Children.Add(node);
            }

            return treeNodes;
        }
        
        private bool IsChildTagOf_Internal(GameplayTagData child, GameplayTagData parent)
        {
            if (m_tagTreeNodes == null)
            {
                m_tagTreeNodes = GenerateTreeNodes();
            }

            if (child == null || parent == null)
            {
                return false;
            }

            if (!m_tagTreeNodes.ContainsKey(child.Id) || !m_tagTreeNodes.ContainsKey(parent.Id))
            {
                return false;
            }

            if (child.Id.Equals(parent.Id))
            {
                return false;
            }
            
            GameplayTagTreeNode childNode = m_tagTreeNodes[child.Id];
            while (childNode.Parent != null)
            {
                childNode = childNode.Parent;
                if (childNode.Tag == null)
                {
                    return false;
                }
                if (childNode.Tag.Id.Equals(parent.Id))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsChildTagOf(GameplayTag child, GameplayTag parent)
        {
            List<GameplayTagCollection> allCollections = Datastores.GetDataCollectionsOfType<GameplayTagCollection>();
            foreach (GameplayTagCollection collection in allCollections)
            {
                if (collection.IsChildTagOf_Internal(child.GetData(), parent.GetData()))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class GameplayTagTreeNode
    {
        public GameplayTagData Tag;
        public GameplayTagTreeNode Parent;
        public List<GameplayTagTreeNode> Children;
    }
}