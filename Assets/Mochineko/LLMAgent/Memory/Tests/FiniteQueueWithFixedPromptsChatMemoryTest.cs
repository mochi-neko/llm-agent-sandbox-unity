#nullable enable
using System.Collections.Generic;
using Mochineko.ChatGPT_API;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mochineko.LLMAgent.Memory.Tests
{
    [TestFixture]
    internal sealed class FiniteQueueWithFixedPromptsChatMemoryTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public void MemoriesShouldBeUpToCapacityWithFixedPrompts()
        {
            var memory = new FiniteQueueWithFixedPromptsChatMemory(3);
            memory.ShouldBeSameContentsAs(new List<Message>());
            
            memory.AddMessage(new Message(Role.System, "prompt"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
            });
            
            memory.AddMessage(new Message(Role.User, "a"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "a"),
            });
            
            memory.AddMessage(new Message(Role.Assistant, "b"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "a"),
                new Message(Role.Assistant, "b"),
            });
            
            // System messages are not counted as capacity
            memory.AddMessage(new Message(Role.User, "c"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "a"),
                new Message(Role.Assistant, "b"),
                new Message(Role.User, "c"),
            });
            
            memory.AddMessage(new Message(Role.Assistant, "d"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.Assistant, "b"),
                new Message(Role.User, "c"),
                new Message(Role.Assistant, "d"),
            });
            
            memory.AddMessage(new Message(Role.User, "e"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "c"),
                new Message(Role.Assistant, "d"),
                new Message(Role.User, "e"),
            });
            
            memory.ClearMessagesWithoutPrompts();
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
            });
            
            memory.AddMessage(new Message(Role.User, "f"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "f"),
            });
            
            memory.AddMessage(new Message(Role.System, "second prompt"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.System, "second prompt"),
                new Message(Role.User, "f"),
            });
            
            memory.AddMessage(new Message(Role.Assistant, "g"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.System, "second prompt"),
                new Message(Role.User, "f"),
                new Message(Role.Assistant, "g"),
            });

            memory.ClearAllMessages();
            memory.ShouldBeSameContentsAs(new List<Message>());
        }
        
        [Test]
        [RequiresPlayMode(false)]
        public void ShouldBeSameBehaviourWhenHasNoPrompts()
        {
            var memory = new FiniteQueueWithFixedPromptsChatMemory(3);
            memory.ShouldBeSameContentsAs(new List<Message>());
            
            memory.AddMessage(new Message(Role.User, "a"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "a"),
            });
            
            memory.AddMessage(new Message(Role.Assistant, "b"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "a"),
                new Message(Role.Assistant, "b"),
            });
            
            memory.AddMessage(new Message(Role.User, "c"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "a"),
                new Message(Role.Assistant, "b"),
                new Message(Role.User, "c"),
            });
            
            // Up to capacity
            memory.AddMessage(new Message(Role.Assistant, "d"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.Assistant, "b"),
                new Message(Role.User, "c"),
                new Message(Role.Assistant, "d"),
            });
            
            memory.AddMessage(new Message(Role.User, "e"));
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "c"),
                new Message(Role.Assistant, "d"),
                new Message(Role.User, "e"),
            });

            memory.ClearAllMessages();
            memory.ShouldBeSameContentsAs(new List<Message>());
        }
    }
}