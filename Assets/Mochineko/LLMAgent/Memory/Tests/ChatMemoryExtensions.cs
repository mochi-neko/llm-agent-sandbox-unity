#nullable enable
using System.Collections.Generic;
using FluentAssertions;
using Mochineko.ChatGPT_API;

namespace Mochineko.LLMAgent.Memory.Tests
{
    internal static class ChatMemoryExtensions
    {
        internal static void ShouldBeSameContentsAs(this IChatMemory memory, IReadOnlyList<Message> expected)
        {
            var messages = memory.Messages;

            messages.Count.Should().Be(expected.Count);

            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                var contentMessage = expected[i];

                message.Role.Should().Be(contentMessage.Role);
                message.RoleString.Should().Be(contentMessage.RoleString);
                message.Content.Should().Be(contentMessage.Content);
            }
        }
    }
}