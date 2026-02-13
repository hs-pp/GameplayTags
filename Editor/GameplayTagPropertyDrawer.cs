using System.Collections.Generic;
using System.Text;
using DatastoresDX.Editor;
using DatastoresDX.Editor.DataCollections;
using DatastoresDX.Runtime;
using DatastoresDX.Runtime.DataCollections;
using GameplayTags.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace GameplayTags.Editor
{
    [CustomPropertyDrawer(typeof(GameplayTag))]
    public class GameplayTagPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new GameplayTagField(property, new GameplayTagDrawer(property));
        }
    }

    public class GameplayTagField : BaseField<GameplayTag>
    {
        private GameplayTagDrawer m_drawer;
        public GameplayTagField(SerializedProperty gameplayTagSP, GameplayTagDrawer visualInput) : base(gameplayTagSP.displayName, visualInput)
        {
            AddToClassList(alignedFieldUssClassName);
            m_drawer = visualInput;
            
            this.TrackPropertyValue(gameplayTagSP, property =>
            {
                m_drawer.RefreshElement();
            });
        }
    }

    public class GameplayTagDrawer : VisualElement
    {
        private static string VIEW_UXML = "GameplayTags/GameplayTagDrawer";
        private static string UNSET_FIELD_VIEW_TAG = "unset-field-view";
        private static string SET_FIELD_VIEW_TAG = "set-field-view";
        private static string TAG_LABEL_TAG = "tag-label";
        
        private VisualElement m_unsetFieldView;
        private VisualElement m_setFieldView;
        private Label m_tagLabel;
        
        private SerializedProperty m_gameplayTagSP;

        public GameplayTagDrawer(SerializedProperty gameplayTagSP)
        {
            m_gameplayTagSP = gameplayTagSP;

            CreateLayout();
            LoadProperty();
        }

        private void CreateLayout()
        {
            var visualTree = Resources.Load<VisualTreeAsset>(VIEW_UXML);
            visualTree.CloneTree(this);
            
            m_unsetFieldView = this.Q(UNSET_FIELD_VIEW_TAG);
            m_setFieldView = this.Q(SET_FIELD_VIEW_TAG);
            m_tagLabel = this.Q<Label>(TAG_LABEL_TAG);

            this.AddManipulator(new Clickable(() =>
            {
                PopupWindow.Show(worldBound, new DataReferenceSelectorPopup(typeof(GameplayTagData), SetGameplayTag));
            }));
        }

        private void LoadProperty()
        {
            SerializedProperty idSP = m_gameplayTagSP.FindPropertyRelative(GameplayTag.TagId_VarName);
            Uid tagId = Uid.FromSerializedProperty(idSP);
            WorkflowElementKey elementKey = DatastoresEditorCore.GetDataElementKey(tagId, true);

            IDataElement element = elementKey.GetElement();
            if (element == null && !tagId.IsInvalid()) // TODO: also check to make sure this is a GameplayTag id.
            {
                // Tag doesnt exist!!
                Debug.LogError("[GameplayTags] GameplayTagSingle ref is broken! Do something about it!!");
            }
            if (element == null)
            {
                m_unsetFieldView.style.display = DisplayStyle.Flex;
                m_setFieldView.style.display = DisplayStyle.None;
            }
            else
            {
                DataCollectionWorkflow workflow = elementKey.GetWorkflow() as DataCollectionWorkflow;
                m_tagLabel.text = GetPathString((workflow.GetElementById(tagId) as DataCollectionElementWrapper).ElementSP);
                
                this.Unbind();
                this.TrackSerializedObjectValue(workflow.DataCollectionSO, prop => { RefreshElement(); });
                
                m_unsetFieldView.style.display = DisplayStyle.None;
                m_setFieldView.style.display = DisplayStyle.Flex;
            }
        }
        
        private string GetPathString(SerializedProperty elementSP)
        {
            List<SerializedProperty> path = DataCollection.GetFullPath(elementSP);
            StringBuilder str = new StringBuilder();
            foreach (SerializedProperty property in path)
            {
                str.Append(property.FindPropertyRelative(DataCollectionElement.DisplayName_VarName).stringValue);
                if (property.propertyPath != elementSP.propertyPath)
                {
                    str.Append(".");
                }
            }

            return str.ToString();
        }

        private void SetGameplayTag(Uid elementId)
        {
            SerializedProperty idSP = m_gameplayTagSP.FindPropertyRelative(GameplayTag.TagId_VarName);
            if (elementId.IsInvalid())
            {
                Uid.ToSerializedProperty(idSP, elementId);
                idSP.serializedObject.ApplyModifiedProperties();
                return;
            }
            
            Uid.ToSerializedProperty(idSP, elementId);
            idSP.serializedObject.ApplyModifiedProperties();
        }
        
        public void RefreshElement()
        {
            LoadProperty();
        }
    }
}