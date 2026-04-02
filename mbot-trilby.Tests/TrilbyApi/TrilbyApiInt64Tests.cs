using System;
using System.Linq;
using System.Reflection;
using TrilbyApi.Api;
using TrilbyApi.Client;
using TrilbyApi.Model;
using Xunit;

namespace mbottrilby.Tests.TrilbyApi
{
    public sealed class TrilbyApiInt64Tests
    {
        [Theory]
        [InlineData(nameof(IDefaultApi.GetCurrentIntroAsync), 2, "guildId", typeof(string))]
        [InlineData(nameof(IDefaultApi.GetRecentClipStatsAsync), 5, "guildId", typeof(string))]
        [InlineData(nameof(IDefaultApi.GetTopClipStatsAsync), 6, "guildId", typeof(string))]
        [InlineData(nameof(IDefaultApi.ListClipsAsync), 3, "guildId", typeof(string))]
        [InlineData(nameof(IDefaultApi.RemoveTagClipAsync), 4, "guildId", typeof(string))]
        [InlineData(nameof(IDefaultApi.ListTagClipsAsync), 3, "guildId", typeof(string))]
        [InlineData(nameof(IDefaultApi.ListTagsAsync), 3, "guildId", typeof(string))]
        [InlineData(nameof(IDefaultApi.RefreshTrilbySessionAsync), 2, "refreshSessionRequest", typeof(RefreshSessionRequest))]
        public void Generated_Api_Methods_Use_String_Snowflake_Params(
            string methodName,
            int parameterCount,
            string parameterName,
            Type expectedType)
        {
            System.Reflection.MethodInfo method = typeof(IDefaultApi)
                .GetMethods()
                .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == parameterCount);
            System.Reflection.ParameterInfo parameter = method.GetParameters().Single(candidate => candidate.Name == parameterName);

            Assert.Equal(expectedType, parameter.ParameterType);
        }

        [Theory]
        [InlineData(typeof(AuthenticatedTrilbyGuildResponse), nameof(AuthenticatedTrilbyGuildResponse.GuildId), typeof(string))]
        [InlineData(typeof(GetCurrentIntroResponse), nameof(GetCurrentIntroResponse.GuildId), typeof(string))]
        [InlineData(typeof(GetCurrentIntroResponse), nameof(GetCurrentIntroResponse.RequesterUserId), typeof(string))]
        [InlineData(typeof(ListClipsResponse), nameof(ListClipsResponse.GuildId), typeof(string))]
        [InlineData(typeof(ListTagClipsResponse), nameof(ListTagClipsResponse.GuildId), typeof(string))]
        [InlineData(typeof(ListTagsResponse), nameof(ListTagsResponse.GuildId), typeof(string))]
        [InlineData(typeof(RecentClipStatsResponse), nameof(RecentClipStatsResponse.GuildId), typeof(string))]
        [InlineData(typeof(RecentClipStatsResponse), nameof(RecentClipStatsResponse.RequesterUserId), typeof(string))]
        [InlineData(typeof(SessionResponse), nameof(SessionResponse.UserId), typeof(string))]
        [InlineData(typeof(SessionResponse), nameof(SessionResponse.DefaultGuildId), typeof(string))]
        [InlineData(typeof(SessionSummaryResponse), nameof(SessionSummaryResponse.UserId), typeof(string))]
        [InlineData(typeof(SessionSummaryResponse), nameof(SessionSummaryResponse.DefaultGuildId), typeof(string))]
        [InlineData(typeof(TopClipStatsResponse), nameof(TopClipStatsResponse.GuildId), typeof(string))]
        [InlineData(typeof(TopClipStatsResponse), nameof(TopClipStatsResponse.RequesterUserId), typeof(string))]
        public void Generated_Models_Use_String_Snowflake_Properties(
            Type modelType,
            string propertyName,
            Type expectedType)
        {
            System.Reflection.PropertyInfo property = modelType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

            Assert.NotNull(property);
            Assert.Equal(expectedType, property!.PropertyType);
        }
    }
}
