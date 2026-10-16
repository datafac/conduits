using Shouldly;
using System;
using System.Threading.Tasks;
using Testing.ChatHub;
using Xunit;

namespace DataFac.Conduits.UnitTests;

public class ChatHubTests
{
    [Fact]
    public async Task SendMessage()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var client = new ChatHubClient(new ChatHubServer(new ChatHub()));
        MessageId id = new MessageId(Guid.NewGuid());

        // act
        var result = await client.SendMessage(new SendMessageCmd() { MessageId = id, Source = new Address("Alan"), Target = new Address("Belle"), Content = "Hello?" }, ct);

        // assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task GetAll()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var client = new ChatHubClient(new ChatHubServer(new ChatHub()));
        MessageId id = new MessageId(Guid.NewGuid());
        var allan = new Address("Allan");
        var belle = new Address("Belle");
        await client.SendMessage(new SendMessageCmd() { MessageId = id, Source = allan, Target = belle, Content = "Hello?" }, ct);

        // act
        var result = await client.GetAll(belle, ct);

        // assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(1);
        result[0].Source.ShouldBe(allan);
        result[0].Target.ShouldBe(belle);
        result[0].MessageId.ShouldBe(id);
        result[0].Content.ShouldBe("Hello?");
    }

    [Fact]
    public async Task AckMessage()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        await using var client = new ChatHubClient(new ChatHubServer(new ChatHub()));
        MessageId id = new MessageId(Guid.NewGuid());
        var allan = new Address("Allan");
        var belle = new Address("Belle");
        await client.SendMessage(new SendMessageCmd() { MessageId = id, Source = allan, Target = belle, Content = "Hello?" }, ct);
        var getall1 = await client.GetAll(belle, ct);
        getall1.ShouldNotBeNull();
        getall1.Length.ShouldBe(1);
        getall1[0].MessageId.ShouldBe(id);

        // act
        bool acked = await client.AckMessage(belle, id, ct);

        // assert
        acked.ShouldBeTrue();
        var getall2 = await client.GetAll(belle, ct);
        getall2.ShouldNotBeNull();
        getall2.Length.ShouldBe(0);
    }

}
