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
        [InlineData(nameof(IDefaultApi.GetCurrentIntroAsync), 2, "guildId", typeof(long))]
        [InlineData(nameof(IDefaultApi.GetRecentClipStatsAsync), 5, "guildId", typeof(long))]
        [InlineData(nameof(IDefaultApi.GetTopClipStatsAsync), 6, "guildId", typeof(long))]
        [InlineData(nameof(IDefaultApi.ListClipsAsync), 3, "guildId", typeof(long))]
        [InlineData(nameof(IDefaultApi.RemoveTagClipAsync), 4, "guildId", typeof(long))]
        [InlineData(nameof(IDefaultApi.ListTagClipsAsync), 3, "guildId", typeof(long))]
        [InlineData(nameof(IDefaultApi.ListTagsAsync), 3, "guildId", typeof(long))]
        [InlineData(nameof(IDefaultApi.RefreshTrilbySessionAsync), 2, "refreshSessionRequest", typeof(RefreshSessionRequest))]
        public void Generated_Api_Methods_Keep_Snowflake_Params_As_Long(
            string methodName,
            int parameterCount,
            string parameterName,
            Type expectedType)
        {
            var method = typeof(IDefaultApi)
                .GetMethods()
                .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == parameterCount);
            var parameter = method.GetParameters().Single(candidate => candidate.Name == parameterName);

            Assert.Equal(expectedType, parameter.ParameterType);
        }

        [Theory]
        [InlineData(typeof(GetCurrentIntroResponse), nameof(GetCurrentIntroResponse.GuildId), typeof(long))]
        [InlineData(typeof(GetCurrentIntroResponse), nameof(GetCurrentIntroResponse.RequesterUserId), typeof(long))]
        [InlineData(typeof(ListClipsResponse), nameof(ListClipsResponse.GuildId), typeof(long))]
        [InlineData(typeof(ListTagClipsResponse), nameof(ListTagClipsResponse.GuildId), typeof(long))]
        [InlineData(typeof(ListTagsResponse), nameof(ListTagsResponse.GuildId), typeof(long))]
        [InlineData(typeof(RecentClipStatsResponse), nameof(RecentClipStatsResponse.GuildId), typeof(long))]
        [InlineData(typeof(RecentClipStatsResponse), nameof(RecentClipStatsResponse.RequesterUserId), typeof(long?))]
        [InlineData(typeof(TopClipStatsResponse), nameof(TopClipStatsResponse.GuildId), typeof(long))]
        [InlineData(typeof(TopClipStatsResponse), nameof(TopClipStatsResponse.RequesterUserId), typeof(long?))]
        public void Generated_Models_Keep_Snowflake_Properties_As_Long(
            Type modelType,
            string propertyName,
            Type expectedType)
        {
            var property = modelType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

            Assert.NotNull(property);
            Assert.Equal(expectedType, property!.PropertyType);
        }
    }
}
