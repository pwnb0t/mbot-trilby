using mbottrilby.Services;
using Xunit;

namespace mbottrilby.Tests.Services
{
    public sealed class TrilbyEventsClientServiceTests
    {
        [Fact]
        public void ParseClipPlayedEvent_ParsesValidEnvelope()
        {
            var json = """
            {
              "event_type": "clip_played",
              "guild_id": 123,
              "occurred_at_utc": "2026-03-25T20:00:00Z",
              "payload": {
                "trigger": "hello",
                "mode": "random",
                "requester_user_id": 456,
                "requester_display_name": "Vic",
                "played_at_utc": "2026-03-25T20:00:00Z"
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseClipPlayedEvent(json);

            Assert.NotNull(parsed);
            Assert.Equal(123, parsed!.GuildId);
            Assert.Equal("hello", parsed.Trigger);
            Assert.Equal("random", parsed.Mode);
            Assert.Equal(456, parsed.RequesterUserId);
            Assert.Equal("Vic", parsed.RequesterDisplayName);
            Assert.Equal("2026-03-25T20:00:00Z", parsed.PlayedAtUtc);
            Assert.True(parsed.IsRandom);
        }

        [Fact]
        public void ParseClipPlayedEvent_FallsBackToUserIdWhenNameMissing()
        {
            var json = """
            {
              "event_type": "clip_played",
              "guild_id": 123,
              "payload": {
                "trigger": "hello",
                "mode": "direct",
                "requester_user_id": 456,
                "played_at_utc": "2026-03-25T20:00:00Z"
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseClipPlayedEvent(json);

            Assert.NotNull(parsed);
            Assert.Equal("456", parsed!.RequesterDisplayName);
        }
    }
}
