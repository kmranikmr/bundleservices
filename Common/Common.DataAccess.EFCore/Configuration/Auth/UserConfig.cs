/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.DataAccess.EFCore.Configuration.System;
using Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.DataAccess.EFCore.Configuration.Auth
{
    public class UserConfig : BaseEntityConfig<User>
    {
        public UserConfig() : base("Users") { }

        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.Property(obj => obj.FirstName);
            builder.Property(obj => obj.LastName);
            builder.Property(obj => obj.UserName).IsRequired();
            builder.Property(obj => obj.Password);
            builder.Property(obj => obj.Email).IsRequired();
            builder.Property(obj => obj.Age);

            builder.Property(obj => obj.AddressCity).HasColumnName("City");
            builder.Property(obj => obj.AddressStreet).HasColumnName("Street");
            builder.Property(obj => obj.AddressZipCode).HasColumnName("ZipCode");
            builder.Property(obj => obj.AddressLat).HasColumnName("Lat");
            builder.Property(obj => obj.AddressLng).HasColumnName("Lng");

            builder
                .HasOne(obj => obj.Photo)
                .WithOne(obj => obj.User)
                .HasForeignKey<UserPhoto>(obj => obj.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(obj => obj.UserRoles)
                .WithOne()
                .HasForeignKey(obj => obj.UserId)
                .IsRequired();

            builder
                .HasMany(obj => obj.Claims)
                .WithOne()
                .HasForeignKey(obj => obj.UserId)
                .IsRequired();
        }
    }
}
