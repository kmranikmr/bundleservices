/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

namespace Common.Utils
{
    public static class Mapper
    {
        public static TResult MapTo<TResult>(this object source)
            where TResult : class
        {
            return AutoMapper.Mapper.Map<TResult>(source);
        }
    }
}
