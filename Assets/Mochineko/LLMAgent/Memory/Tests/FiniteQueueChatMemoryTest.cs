#nullable enable
using System.Collections.Generic;
using Mochineko.ChatGPT_API;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mochineko.LLMAgent.Memory.Tests
{
    [TestFixture]
    internal sealed class FiniteQueueChatMemoryTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public void MemoriesShouldBeUpToCapacity()
        {
            var memory = new FiniteQueueChatMemory(3);
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
