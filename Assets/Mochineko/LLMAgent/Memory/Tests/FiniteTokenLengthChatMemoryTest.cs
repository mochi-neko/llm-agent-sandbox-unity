#nullable enable
using System.Collections.Generic;
using FluentAssertions;
using Mochineko.ChatGPT_API;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mochineko.LLMAgent.Memory.Tests
{
    [TestFixture]
    internal sealed class FiniteTokenLengthChatMemoryTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public void MemoriesTokenLengthShouldBeUpToCapacity()
        {
            var memory = new FiniteTokenLengthQueueChatMemory(10);
            memory.TokenLength.Should().Be(0);
            memory.ShouldBeSameContentsAs(new List<Message>());
            
            memory.AddMessage(new Message(Role.User, "a"));
            memory.TokenLength.Should().Be(1);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "a"),
            });

            memory.AddMessage(new Message(Role.User, "b"));
            memory.TokenLength.Should().Be(2);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "a"),
                new Message(Role.User, "b"),
            });

            memory.AddMessage(new Message(Role.User, "c d e f g h i j"));
            memory.TokenLength.Should().Be(10);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "a"),
                new Message(Role.User, "b"),
                new Message(Role.User, "c d e f g h i j"),
            });
            
            memory.AddMessage(new Message(Role.User, "k"));
            memory.TokenLength.Should().Be(10);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "b"),
                new Message(Role.User, "c d e f g h i j"),
                new Message(Role.User, "k"),
            });
            
            memory.AddMessage(new Message(Role.User, "l m n o p"));
            memory.TokenLength.Should().Be(6);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "k"),
                new Message(Role.User, "l m n o p"),
            });
            
            memory.AddMessage(new Message(Role.User, "q"));
            memory.TokenLength.Should().Be(7);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "k"),
                new Message(Role.User, "l m n o p"),
                new Message(Role.User, "q"),
            });
            
            memory.AddMessage(new Message(Role.User, "r"));
            memory.TokenLength.Should().Be(8);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "k"),
                new Message(Role.User, "l m n o p"),
                new Message(Role.User, "q"),
                new Message(Role.User, "r"),
            });
            
            memory.AddMessage(new Message(Role.User, "1 2 3 4 5 "));
            memory.TokenLength.Should().Be(10);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "1 2 3 4 5 "),
            });
            
            memory.ClearAllMessages();
            memory.TokenLength.Should().Be(0);
            memory.ShouldBeSameContentsAs(new List<Message>());
        }
    }
}