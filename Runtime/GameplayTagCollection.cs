using System.Collections.Generic;
using DatastoresDX.Runtime;
using DatastoresDX.Runtime.DataCollections;

namespace GameplayTags.Runtime
{
    [DataCollection(true, true, "Gameplay Tags")]
    public class GameplayTagCollection : DataCollection
    {
        private Dictionary<Uid, GameplayTagTreeNode> m_tagTreeNodes;
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
                    treeNodes.Add(tag.Id, new GameplayTagTreeNode() { Tag = tag });
                }

                GameplayTagTreeNode node = treeNodes[tag.Id];
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