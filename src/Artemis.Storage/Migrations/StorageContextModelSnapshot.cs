﻿// <auto-generated />

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Artemis.Storage.Migrations
{
    [DbContext(typeof(StorageContext))]
    internal class StorageContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

            modelBuilder.Entity("Artemis.Storage.Entities.FolderEntity", b =>
            {
                b.Property<string>("Guid")
                    .ValueGeneratedOnAdd();

                b.Property<string>("FolderEntityGuid");

                b.Property<string>("Name");

                b.Property<int>("Order");

                b.HasKey("Guid");

                b.HasIndex("FolderEntityGuid");

                b.ToTable("Folders");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.KeypointEntity", b =>
            {
                b.Property<string>("Guid")
                    .ValueGeneratedOnAdd();

                b.Property<string>("LayerSettingEntityGuid");

                b.Property<int>("Time");

                b.Property<string>("Value");

                b.HasKey("Guid");

                b.HasIndex("LayerSettingEntityGuid");

                b.ToTable("Keypoints");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.LayerEntity", b =>
            {
                b.Property<string>("Guid")
                    .ValueGeneratedOnAdd();

                b.Property<string>("FolderEntityGuid");

                b.Property<string>("Name");

                b.Property<int>("Order");

                b.HasKey("Guid");

                b.HasIndex("FolderEntityGuid");

                b.ToTable("Layers");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.LayerSettingEntity", b =>
            {
                b.Property<string>("Guid")
                    .ValueGeneratedOnAdd();

                b.Property<string>("LayerEntityGuid");

                b.Property<string>("Name");

                b.Property<string>("Value");

                b.HasKey("Guid");

                b.HasIndex("LayerEntityGuid");

                b.ToTable("LayerSettings");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.LedEntity", b =>
            {
                b.Property<string>("Guid")
                    .ValueGeneratedOnAdd();

                b.Property<string>("LayerGuid");

                b.Property<int>("LayerId");

                b.Property<string>("LedName");

                b.Property<string>("LimitedToDevice");

                b.HasKey("Guid");

                b.HasIndex("LayerGuid");

                b.ToTable("Leds");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.ProfileEntity", b =>
            {
                b.Property<string>("Guid")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Name");

                b.Property<Guid>("PluginGuid");

                b.Property<string>("RootFolderGuid");

                b.Property<int>("RootFolderId");

                b.HasKey("Guid");

                b.HasIndex("RootFolderGuid");

                b.ToTable("Profiles");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.SettingEntity", b =>
            {
                b.Property<string>("Name")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Value");

                b.HasKey("Name");

                b.ToTable("Settings");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.FolderEntity", b =>
            {
                b.HasOne("Artemis.Storage.Entities.FolderEntity")
                    .WithMany("Folders")
                    .HasForeignKey("FolderEntityGuid");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.KeypointEntity", b =>
            {
                b.HasOne("Artemis.Storage.Entities.LayerSettingEntity")
                    .WithMany("Keypoints")
                    .HasForeignKey("LayerSettingEntityGuid");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.LayerEntity", b =>
            {
                b.HasOne("Artemis.Storage.Entities.FolderEntity")
                    .WithMany("Layers")
                    .HasForeignKey("FolderEntityGuid");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.LayerSettingEntity", b =>
            {
                b.HasOne("Artemis.Storage.Entities.LayerEntity")
                    .WithMany("Settings")
                    .HasForeignKey("LayerEntityGuid");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.LedEntity", b =>
            {
                b.HasOne("Artemis.Storage.Entities.LayerEntity", "Layer")
                    .WithMany("Leds")
                    .HasForeignKey("LayerGuid");
            });

            modelBuilder.Entity("Artemis.Storage.Entities.ProfileEntity", b =>
            {
                b.HasOne("Artemis.Storage.Entities.FolderEntity", "RootFolder")
                    .WithMany()
                    .HasForeignKey("RootFolderGuid");
            });
#pragma warning restore 612, 618
        }
    }
}