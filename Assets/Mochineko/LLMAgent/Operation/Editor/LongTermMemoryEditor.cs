#nullable enable
using System.Collections.Generic;
using System.Linq;
using Mochineko.ChatGPT_API;
using Mochineko.LLMAgent.Summarization;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Mochineko.LLMAgent.Operation.Editor
{
    internal sealed class LongTermMemoryEditor : EditorWindow
    {
        [MenuItem("Mochineko/LLMAgent/LongTermMemory")]
        private static void Open()
        {
            GetWindow<LongTermMemoryEditor>("LongTermMemory");
        }

        private DemoOperator? demoOperator;
        private Vector2 totalScrollPosition;
        private Vector2 promptsScrollPosition;
        private Vector2 shortTermMemoriesScrollPosition;
        private Vector2 bufferMemoriesScrollPosition;
        private Vector2 summaryScrollPosition;

        private void OnGUI()
        {
            demoOperator = EditorGUILayout.ObjectField(
                    "DemoOperator",
                    demoOperator,
                    typeof(DemoOperator),
                    true)
                as DemoOperator;

            EditorGUILayout.Space();

            if (demoOperator == null)
            {
                EditorGUILayout.LabelField("Please specify demo operator...");
                return;
            }

            EditorGUILayout.Space();

            var memory = demoOperator.Memory;
            if (memory == null)
            {
                EditorGUILayout.LabelField("Please start demo operator...");
                return;
            }

            EditorGUILayout.Space();

            using var totalScroll = new EditorGUILayout.ScrollViewScope(totalScrollPosition, GUI.skin.box);
            totalScrollPosition = totalScroll.scrollPosition;

            EditorGUILayout.LabelField($"Total tokens:{memory.TotalMemoriesTokenLength}");

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Prompts:");
            if (GUILayout.Button("Copy to clipboard"))
            {
                CopyConversationJsonToClipboard(memory.Prompts);
            }
            using (var scope = new EditorGUILayout.ScrollViewScope(promptsScrollPosition, GUI.skin.box))
            {
                promptsScrollPosition = scope.scrollPosition;

                EditorGUILayout.LabelField($"Tokens:{memory.PromptsTokenLength}");
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    foreach (var prompt in memory.Prompts)
                    {
                        EditorGUILayout.TextArea($"{prompt.Content}");
                    }
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Short Term Memory:");
            if (GUILayout.Button("Copy to clipboard"))
            {
                CopyConversationJsonToClipboard(memory.ShortTermMemories);
            }
            using (var scope = new EditorGUILayout.ScrollViewScope(shortTermMemoriesScrollPosition, GUI.skin.box))
            {
                shortTermMemoriesScrollPosition = scope.scrollPosition;

                EditorGUILayout.LabelField($"Tokens:{memory.ShortTermMemoriesTokenLength}");
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    foreach (var message in memory.ShortTermMemories)
                    {
                        EditorGUILayout.TextArea($"{message.Role} > {message.Content}");
                    }
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Buffer Memory:");
            if (GUILayout.Button("Copy to clipboard"))
            {
                CopyConversationJsonToClipboard(memory.BufferMemories);
            }
            using (var scope = new EditorGUILayout.ScrollViewScope(bufferMemoriesScrollPosition, GUI.skin.box))
            {
                bufferMemoriesScrollPosition = scope.scrollPosition;

                EditorGUILayout.LabelField($"Tokens:{memory.BufferMemoriesTokenLength}");
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    foreach (var message in memory.BufferMemories)
                    {
                        EditorGUILayout.TextArea($"{message.Role} > {message.Content}");
                    }
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Summary:");
            if (GUILayout.Button("Copy to clipboard"))
            {
                CopyToClipboard(memory.Summary.Content);
            }
            using (var scope = new EditorGUILayout.ScrollViewScope(summaryScrollPosition, GUI.skin.box))
            {
                summaryScrollPosition = scope.scrollPosition;
                
                EditorGUILayout.LabelField($"Tokens:{memory.SummaryTokenLength}");
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.TextArea($"{memory.Summary.Content}");
                }
            }
        }

        private static void CopyConversationJsonToClipboard(IEnumerable<Message> messages)
        {
            var conversations = new ConversationCollection(messages.ToList());
            var json = JsonConvert.SerializeObject(conversations);

            EditorGUIUtility.systemCopyBuffer = json;
            
            Debug.Log($"Copy json to clipboard:{json}");
        }

        private static void CopyToClipboard(string text)
        {
            EditorGUIUtility.systemCopyBuffer = text;
            
            Debug.Log($"Copy text to clipboard:{text}");
        }
    }
}