/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.DTO;
using System.Threading.Tasks;

namespace Common.Services.Infrastructure
{
    public interface IUserService
    {
        Task<UserDTO> GetById(int id);
        Task<UserDTO> GetByName(string username);
        Task<bool> Delete(int id);
        Task<UserDTO> Edit(UserDTO dto);
        Task<byte[]> GetUserPhoto(int userId);
    }
}
