using System;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityGameFramework.Runtime;

namespace Everlight.Tales.UI
{
    public sealed class DialoguePageData
    {
        public readonly DialogueGraph Graph;
        public readonly Action Completed;
        public readonly WorldSession Owner;
        public DialoguePageData(DialogueGraph graph, Action completed, WorldSession owner)
        { Graph = graph; Completed = completed; Owner = owner; }
    }

    public sealed partial class DialoguePageForm
    {
        private const string DialogueDataKey = "DialoguePage.Data";
        private DialoguePageData _flow;
        private DialogueService _dialogue;
        private bool _completionRequested;

        public static int Open(DialoguePageData data)
        {
            UIParams args = UIParams.Create();
            args.Set(DialogueDataKey, data);
            return GF.UI.OpenUIForm(UIViews.DialoguePage, args);
        }

        private void OpenDialogueData()
        {
            _flow = null; _dialogue = null; _completionRequested = false;
            if (Params.TryGet<VarObject>(DialogueDataKey, out VarObject value) && value.Value is DialoguePageData data)
            {
                _flow = data;
                _dialogue = new DialogueService(data.Graph);
                RefreshDialogue();
            }
        }

        private void RefreshDialogue()
        {
            DialogueNode node = _dialogue.Current;
            if (node == null) { CompleteDialogue(); return; }
            SetDialogue(node.Line.Speaker, node.Line.Text);
            foreach (var button in m_Buttons) { button.onClick.RemoveAllListeners(); button.gameObject.SetActive(false); }
            int choices = node.IsEnd ? 1 : node.Choices.Count;
            if (choices > m_Buttons.Length - 1)
                throw new InvalidOperationException("DialoguePage 对话选项超过预制体容量。");
            for (int i = 0; i < choices; i++)
            {
                int choice = i;
                m_Buttons[i].gameObject.SetActive(true);
                m_ButtonLabels[i].text = node.IsEnd ? "确认" : node.Choices[i].Label;
                m_Buttons[i].onClick.AddListener(() =>
                {
                    if (_completionRequested || _dialogue == null) return;
                    if (_dialogue.Current.IsEnd) CompleteDialogue();
                    else if (_dialogue.Choose(choice)) RefreshDialogue();
                });
            }
            int cancel = m_Buttons.Length - 1;
            m_Buttons[cancel].gameObject.SetActive(true);
            m_ButtonLabels[cancel].text = "暂时离开";
            m_Buttons[cancel].onClick.AddListener(OnClickClose);
        }

        private void CompleteDialogue()
        {
            if (_completionRequested) return;
            _completionRequested = true;
            OnClickClose();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            DialoguePageData completed = !isShutdown && _completionRequested ? _flow : null;
            _flow = null; _dialogue = null; _completionRequested = false;
            if (m_Buttons != null) foreach (var button in m_Buttons) if (button != null) button.onClick.RemoveAllListeners();
            SetSpeaker(null);
            base.OnClose(isShutdown, userData);
            if (completed != null && completed.Owner == WorldSession.Current) completed.Completed?.Invoke();
        }
    }
}
