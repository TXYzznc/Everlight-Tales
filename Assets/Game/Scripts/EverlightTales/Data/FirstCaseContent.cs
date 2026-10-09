namespace Everlight.Tales.Data
{
    /// <summary>首案既有故事的短对话与精确身份。图片由正式Sprite目录解析。</summary>
    public static class FirstCaseContent
    {
        public const string CaseId = "L-01";
        public const string EventId = "L-01-01";
        public const string SceneId = "dance";
        public const string Player = "玩家";
        public const string Shen = "沈遥";

        public static readonly PrologueStepConfig[] OpeningSteps =
        {
            new PrologueStepConfig("旁白", "夜雨落在旧城区的铁皮屋顶上。", 2.5f),
            new PrologueStepConfig(Player, "我回来了。长明修理铺的灯还亮着。", 3f),
            new PrologueStepConfig("周衡", "先熟悉工作台，再一起接下邻里的维修委托。", 3f),
        };

        public static DialogueGraph Request()
        {
            var graph = new DialogueGraph("request");
            var request = new DialogueNode("request", new DialogueLine(Shen, "节拍器和音乐停下了，我的脚却还在补完下一组动作。"));
            request.Choices.Add(new DialogueChoice("去舞蹈教室查看", "reply"));
            graph.Add(request);
            graph.Add(new DialogueNode("reply", new DialogueLine(Player, "先确认红鞋和动作之间的联系，别强行抓住她。")));
            return graph;
        }

        public static DialogueGraph InvestigationComplete()
        {
            var graph = new DialogueGraph("confirmed");
            var node = new DialogueNode("confirmed", new DialogueLine(Player, "音乐停止后，红鞋仍在带动身体。先导流、分离，再修好封存匣。"));
            node.Choices.Add(new DialogueChoice("记下作业目标", "response"));
            graph.Add(node);
            graph.Add(new DialogueNode("response", new DialogueLine(Shen, "让我停下来。鞋还在动，也请把它单独封存。")));
            return graph;
        }

        public static DialogueGraph Revisit()
        {
            var graph = new DialogueGraph("thanks");
            var node = new DialogueNode("thanks", new DialogueLine(Shen, "今天终于能按自己的节奏走路了。谢谢你，这份报酬请收下。"));
            node.Choices.Add(new DialogueChoice("领取回访报酬", "reply")); graph.Add(node);
            graph.Add(new DialogueNode("reply", new DialogueLine(Player, "红鞋已经封存。如果还有异常，就来长明修理铺找我。")));
            return graph;
        }
    }
}
