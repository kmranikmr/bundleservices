/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.DTO;
using Common.Entities;
using Common.Services.Infrastructure;
using Common.Utils;
using System.Threading.Tasks;

namespace Common.Services
{
    public class UserService<TUser> : BaseService, IUserService where TUser : User, new()
    {
        protected readonly IUserRepository<TUser> userRepository;
        protected readonly IUserPhotoRepository userPhotoRepository;

        public UserService(ICurrentContextProvider contextProvider, IUserRepository<TUser> userRepository, IUserPhotoRepository userPhotoRepository) : base(contextProvider)
        {
            this.userRepository = userRepository;
            this.userPhotoRepository = userPhotoRepository;
        }

        public async Task<bool> Delete(int id)
        {
            await userRepository.Delete(id, Session);
            return true;
        }

        public async Task<UserDTO> Edit(UserDTO dto)
        {
            var user = dto.MapTo<TUser>();
            await userRepository.Edit(user, Session);
            return user.MapTo<UserDTO>();
        }

        public async Task<byte[]> GetUserPhoto(int userId)
        {
            var photoContent = await userPhotoRepository.Get(userId, Session);
            return photoContent?.Image;
        }

        public async Task<UserDTO> GetById(int id)
        {
            var user = await userRepository.Get(id, Session);
            return user.MapTo<UserDTO>();
        }

        public async Task<UserDTO> GetByName(string username)
        {
            var user = await userRepository.GetByUserName(username, Session);
            return user.MapTo<UserDTO>();
        }
    }
}
