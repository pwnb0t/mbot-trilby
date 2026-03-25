using mbottrilby.Services;
using Xunit;

namespace mbottrilby.Tests.Services
{
    public sealed class TrilbyEventsClientServiceTests
    {
        [Fact]
        public void ParseEvent_ParsesClipPlayedEnvelope()
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

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var clipPlayedEvent = Assert.IsType<TrilbyEventsClientService.ClipPlayedEvent>(parsed);
            Assert.Equal(123, clipPlayedEvent.GuildId);
            Assert.Equal("hello", clipPlayedEvent.Trigger);
            Assert.Equal("random", clipPlayedEvent.Mode);
            Assert.Equal(456, clipPlayedEvent.RequesterUserId);
            Assert.Equal("Vic", clipPlayedEvent.RequesterDisplayName);
            Assert.Equal("2026-03-25T20:00:00Z", clipPlayedEvent.PlayedAtUtc);
            Assert.True(clipPlayedEvent.IsRandom);
        }

        [Fact]
        public void ParseEvent_FallsBackToUserIdWhenNameMissing()
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

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var clipPlayedEvent = Assert.IsType<TrilbyEventsClientService.ClipPlayedEvent>(parsed);
            Assert.Equal("456", clipPlayedEvent.RequesterDisplayName);
        }

        [Fact]
        public void ParseEvent_ReturnsNullForUnknownEventType()
        {
            var json = """
            {
              "event_type": "unknown_event",
              "guild_id": 123,
              "payload": {}
            }
            """;

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.Null(parsed);
        }
    }
}
