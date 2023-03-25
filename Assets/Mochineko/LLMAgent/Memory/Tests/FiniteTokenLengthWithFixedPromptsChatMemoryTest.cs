#nullable enable
using System.Collections.Generic;
using FluentAssertions;
using Mochineko.ChatGPT_API;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mochineko.LLMAgent.Memory.Tests
{
    [TestFixture]
    internal sealed class FiniteTokenLengthWithFixedPromptsChatMemoryTest
    {
        [Test]
        [RequiresPlayMode(false)]
        public void MemoriesTokenLengthShouldBeUpToCapacityWithFixedPrompts()
        {
            var memory = new FiniteTokenLengthQueueWithFixedPromptsChatMemory(10);
            memory.TokenLengthWithoutPrompts.Should().Be(0);
            memory.MemoriesTokenLength.Should().Be(0);
            memory.ShouldBeSameContentsAs(new List<Message>());
            
            memory.AddMessage(new Message(Role.System, "prompt"));
            memory.TokenLengthWithoutPrompts.Should().Be(0);
            memory.MemoriesTokenLength.Should().Be(1);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
            });
            
            memory.AddMessage(new Message(Role.User, "a"));
            memory.TokenLengthWithoutPrompts.Should().Be(1);
            memory.MemoriesTokenLength.Should().Be(2);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "a"),
            });

            memory.AddMessage(new Message(Role.User, "b"));
            memory.TokenLengthWithoutPrompts.Should().Be(2);
            memory.MemoriesTokenLength.Should().Be(3);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "a"),
                new Message(Role.User, "b"),
            });

            memory.AddMessage(new Message(Role.User, "c d e f g h i j"));
            memory.TokenLengthWithoutPrompts.Should().Be(10);
            memory.MemoriesTokenLength.Should().Be(11);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "a"),
                new Message(Role.User, "b"),
                new Message(Role.User, "c d e f g h i j"),
            });
            
            memory.AddMessage(new Message(Role.User, "k"));
            memory.TokenLengthWithoutPrompts.Should().Be(10);
            memory.MemoriesTokenLength.Should().Be(11);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "b"),
                new Message(Role.User, "c d e f g h i j"),
                new Message(Role.User, "k"),
            });
            
            memory.AddMessage(new Message(Role.User, "l m n o p"));
            memory.TokenLengthWithoutPrompts.Should().Be(6);
            memory.MemoriesTokenLength.Should().Be(7);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "k"),
                new Message(Role.User, "l m n o p"),
            });
            
            memory.AddMessage(new Message(Role.User, "q"));
            memory.TokenLengthWithoutPrompts.Should().Be(7);
            memory.MemoriesTokenLength.Should().Be(8);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "k"),
                new Message(Role.User, "l m n o p"),
                new Message(Role.User, "q"),
            });
            
            memory.AddMessage(new Message(Role.User, "r"));
            memory.TokenLengthWithoutPrompts.Should().Be(8);
            memory.MemoriesTokenLength.Should().Be(9);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "k"),
                new Message(Role.User, "l m n o p"),
                new Message(Role.User, "q"),
                new Message(Role.User, "r"),
            });
            
            memory.AddMessage(new Message(Role.User, "1 2 3 4 5 "));
            memory.TokenLengthWithoutPrompts.Should().Be(10);
            memory.MemoriesTokenLength.Should().Be(11);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
                new Message(Role.User, "1 2 3 4 5 "),
            });
            
            memory.ClearMessagesWithoutPrompts();
            memory.TokenLengthWithoutPrompts.Should().Be(0);
            memory.MemoriesTokenLength.Should().Be(1);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.System, "prompt"),
            });
            
            memory.ClearAllMessages();
            memory.TokenLengthWithoutPrompts.Should().Be(0);
            memory.MemoriesTokenLength.Should().Be(0);
            memory.ShouldBeSameContentsAs(new List<Message>());
        }
        
        [Test]
        [RequiresPlayMode(false)]
        public void ShouldBeSameBehaviourWhenHasNoPrompts()
        {
            var memory = new FiniteTokenLengthQueueWithFixedPromptsChatMemory(10);
            memory.TokenLengthWithoutPrompts.Should().Be(0);
            memory.ShouldBeSameContentsAs(new List<Message>());
            
            memory.AddMessage(new Message(Role.User, "a"));
            memory.TokenLengthWithoutPrompts.Should().Be(1);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "a"),
            });

            memory.AddMessage(new Message(Role.User, "b"));
            memory.TokenLengthWithoutPrompts.Should().Be(2);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "a"),
                new Message(Role.User, "b"),
            });

            memory.AddMessage(new Message(Role.User, "c d e f g h i j"));
            memory.TokenLengthWithoutPrompts.Should().Be(10);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "a"),
                new Message(Role.User, "b"),
                new Message(Role.User, "c d e f g h i j"),
            });
            
            memory.AddMessage(new Message(Role.User, "k"));
            memory.TokenLengthWithoutPrompts.Should().Be(10);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "b"),
                new Message(Role.User, "c d e f g h i j"),
                new Message(Role.User, "k"),
            });
            
            memory.AddMessage(new Message(Role.User, "l m n o p"));
            memory.TokenLengthWithoutPrompts.Should().Be(6);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "k"),
                new Message(Role.User, "l m n o p"),
            });
            
            memory.AddMessage(new Message(Role.User, "q"));
            memory.TokenLengthWithoutPrompts.Should().Be(7);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "k"),
                new Message(Role.User, "l m n o p"),
                new Message(Role.User, "q"),
            });
            
            memory.AddMessage(new Message(Role.User, "r"));
            memory.TokenLengthWithoutPrompts.Should().Be(8);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "k"),
                new Message(Role.User, "l m n o p"),
                new Message(Role.User, "q"),
                new Message(Role.User, "r"),
            });
            
            memory.AddMessage(new Message(Role.User, "1 2 3 4 5 "));
            memory.TokenLengthWithoutPrompts.Should().Be(10);
            memory.ShouldBeSameContentsAs(new[]
            {
                new Message(Role.User, "1 2 3 4 5 "),
            });
            
            memory.ClearAllMessages();
            memory.TokenLengthWithoutPrompts.Should().Be(0);
            memory.ShouldBeSameContentsAs(new List<Message>());
        }
    }
}