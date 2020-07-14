﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Migrations.WorkflowDbContext
{
    [DbContext(typeof(NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts.WorkflowDbContext))]
    partial class WorkflowDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("dbo")
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("Relational:Sequence:.EntityFrameworkHiLoSequence", "'EntityFrameworkHiLoSequence', '', '1', '10', '', '', 'Int64', 'False'")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.KeyReleaseWorkflowState", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:HiLoSequenceName", "EntityFrameworkHiLoSequence")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.SequenceHiLo);

                    b.Property<bool>("Authorised")
                        .HasColumnType("bit");

                    b.Property<bool>("AuthorisedByCaregiver")
                        .HasColumnType("bit");

                    b.Property<string>("BucketId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConfirmationKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateOfSymptomsOnset")
                        .HasColumnType("datetime2");

                    b.Property<string>("LabConfirmationId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PollToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ValidUntil")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("KeyReleaseWorkflowState");
                });

            modelBuilder.Entity("NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.TemporaryExposureKeyEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:HiLoSequenceName", "EntityFrameworkHiLoSequence")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.SequenceHiLo);

                    b.Property<byte[]>("KeyData")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<long>("OwnerId")
                        .HasColumnType("bigint");

                    b.Property<int>("PublishingState")
                        .HasColumnType("int");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RollingPeriod")
                        .HasColumnType("int");

                    b.Property<int>("RollingStartNumber")
                        .HasColumnType("int");

                    b.Property<int>("TransmissionRiskLevel")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("TemporaryExposureKeys");
                });

            modelBuilder.Entity("NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.TemporaryExposureKeyEntity", b =>
                {
                    b.HasOne("NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.KeyReleaseWorkflowState", "Owner")
                        .WithMany("Keys")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
