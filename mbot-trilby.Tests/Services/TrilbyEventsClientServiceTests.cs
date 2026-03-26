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

        [Fact]
        public void ParseEvent_ParsesClipTaggedEnvelope()
        {
            var json = """
            {
              "event_type": "clip_tagged",
              "guild_id": 123,
              "payload": {
                "tag_name": "test",
                "clip_trigger": "hello"
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var clipTaggedEvent = Assert.IsType<TrilbyEventsClientService.ClipTaggedEvent>(parsed);
            Assert.Equal(123, clipTaggedEvent.GuildId);
            Assert.Equal("test", clipTaggedEvent.TagName);
            Assert.Equal("hello", clipTaggedEvent.ClipTrigger);
        }

        [Fact]
        public void ParseEvent_ParsesClipPlayCountChangedEnvelope()
        {
            var json = """
            {
              "event_type": "clip_play_count_changed",
              "guild_id": 123,
              "payload": {
                "trigger": "hello",
                "mode": "direct",
                "requester_user_id": 456,
                "requester_display_name": "Vic",
                "played_at_utc": "2026-03-25T20:00:00Z"
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var clipPlayCountChangedEvent = Assert.IsType<TrilbyEventsClientService.ClipPlayCountChangedEvent>(parsed);
            Assert.Equal(123, clipPlayCountChangedEvent.GuildId);
            Assert.Equal("hello", clipPlayCountChangedEvent.Trigger);
            Assert.Equal("direct", clipPlayCountChangedEvent.Mode);
            Assert.Equal(456, clipPlayCountChangedEvent.RequesterUserId);
            Assert.Equal("Vic", clipPlayCountChangedEvent.RequesterDisplayName);
            Assert.False(clipPlayCountChangedEvent.IsRandom);
        }

        [Fact]
        public void ParseEvent_ParsesClipCreatedEnvelope()
        {
            var json = """
            {
              "event_type": "clip_created",
              "guild_id": 123,
              "payload": {
                "trigger": "hello",
                "source_url": "https://example.com/video",
                "start_offset_text": "2.5s",
                "clip_length_text": "4s",
                "added_by_text": "Vic",
                "tag_names": ["test", "wow"]
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var clipCreatedEvent = Assert.IsType<TrilbyEventsClientService.ClipCreatedEvent>(parsed);
            Assert.Equal(123, clipCreatedEvent.GuildId);
            Assert.Equal("hello", clipCreatedEvent.Trigger);
            Assert.Equal("https://example.com/video", clipCreatedEvent.SourceUrl);
            Assert.Equal("2.5s", clipCreatedEvent.StartOffsetText);
            Assert.Equal("4s", clipCreatedEvent.ClipLengthText);
            Assert.Equal("Vic", clipCreatedEvent.AddedByText);
            Assert.Equal(new[] { "test", "wow" }, clipCreatedEvent.TagNames);
        }

        [Fact]
        public void ParseEvent_ParsesClipDeletedEnvelope()
        {
            var json = """
            {
              "event_type": "clip_deleted",
              "guild_id": 123,
              "payload": {
                "trigger": "hello"
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var clipDeletedEvent = Assert.IsType<TrilbyEventsClientService.ClipDeletedEvent>(parsed);
            Assert.Equal(123, clipDeletedEvent.GuildId);
            Assert.Equal("hello", clipDeletedEvent.Trigger);
        }

        [Fact]
        public void ParseEvent_ParsesClipUntaggedEnvelope()
        {
            var json = """
            {
              "event_type": "clip_untagged",
              "guild_id": 123,
              "payload": {
                "tag_name": "test",
                "clip_trigger": "hello"
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var clipUntaggedEvent = Assert.IsType<TrilbyEventsClientService.ClipUntaggedEvent>(parsed);
            Assert.Equal(123, clipUntaggedEvent.GuildId);
            Assert.Equal("test", clipUntaggedEvent.TagName);
            Assert.Equal("hello", clipUntaggedEvent.ClipTrigger);
        }

        [Fact]
        public void ParseEvent_ParsesCurrentIntroUpdatedEnvelope()
        {
            var json = """
            {
              "event_type": "current_intro_updated",
              "guild_id": 123,
              "payload": {
                "user_id": 456,
                "trigger": "intro1"
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var currentIntroUpdatedEvent = Assert.IsType<TrilbyEventsClientService.CurrentIntroUpdatedEvent>(parsed);
            Assert.Equal(123, currentIntroUpdatedEvent.GuildId);
            Assert.Equal(456, currentIntroUpdatedEvent.UserId);
            Assert.Equal("intro1", currentIntroUpdatedEvent.Trigger);
        }

        [Fact]
        public void ParseEvent_ParsesTagCreatedEnvelope()
        {
            var json = """
            {
              "event_type": "tag_created",
              "guild_id": 123,
              "payload": {
                "tag_name": "test"
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var tagCreatedEvent = Assert.IsType<TrilbyEventsClientService.TagCreatedEvent>(parsed);
            Assert.Equal(123, tagCreatedEvent.GuildId);
            Assert.Equal("test", tagCreatedEvent.TagName);
        }

        [Fact]
        public void ParseEvent_ParsesTagDeletedEnvelope()
        {
            var json = """
            {
              "event_type": "tag_deleted",
              "guild_id": 123,
              "payload": {
                "tag_name": "test"
              }
            }
            """;

            var parsed = TrilbyEventsClientService.ParseEvent(json);

            Assert.NotNull(parsed);
            var tagDeletedEvent = Assert.IsType<TrilbyEventsClientService.TagDeletedEvent>(parsed);
            Assert.Equal(123, tagDeletedEvent.GuildId);
            Assert.Equal("test", tagDeletedEvent.TagName);
        }
    }
}
