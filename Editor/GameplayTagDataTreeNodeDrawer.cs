using DatastoresDX.Editor;
using DatastoresDX.Editor.DataCollections;
using DatastoresDX.Runtime.DataCollections;
using GameplayTags.Runtime;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameplayTags.Editor
{
    [ElementTreeNodeDrawer(typeof(GameplayTagData))]
    public class GameplayTagDataTreeNodeDrawer : ElementTreeNodeDrawer
    {
        private Label m_label;

        public GameplayTagDataTreeNodeDrawer()
        {
            m_label = new Label();
            m_label.style.flexGrow = 1;
            m_label.style.unityTextAlign = TextAnchor.MiddleLeft;
            Add(m_label);
        }

        public override void SetElement(WorkflowElementKey elementKey)
        {
            DataCollectionElementWrapper wrapper = elementKey.GetElement() as DataCollectionElementWrapper;
            m_label.BindProperty(wrapper.ElementSP.FindPropertyRelative(DataCollectionElement.DisplayName_VarName));
        }
    }
}