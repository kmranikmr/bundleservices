/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using AutoMapper.Configuration;
using Common.Services.Infrastructure.MappingProfiles;

namespace Common.WebApiCore.Setup
{
    public static class AutoMapperConfig
    {
        public static void Configure(MapperConfigurationExpression config)
        {
            config.AllowNullCollections = false;

            config.AddProfile<UserProfile>();
        }
    }
}
