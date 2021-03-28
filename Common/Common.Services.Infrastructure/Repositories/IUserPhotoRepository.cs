﻿/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.Entities;
using System.Threading.Tasks;

namespace Common.Services.Infrastructure
{
    public interface IUserPhotoRepository
    {
        Task<UserPhoto> Get(int id, ContextSession session);
    }
}
