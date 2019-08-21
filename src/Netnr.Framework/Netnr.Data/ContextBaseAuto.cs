using Microsoft.EntityFrameworkCore;
using Netnr.Domain;

namespace Netnr.Data
{
    public partial class ContextBase : DbContext
    {
        public virtual DbSet<DocSet> DocSet { get; set; }
        public virtual DbSet<DocSetDetail> DocSetDetail { get; set; }
        public virtual DbSet<Draw> Draw { get; set; }
        public virtual DbSet<GiftRecord> GiftRecord { get; set; }
        public virtual DbSet<GiftRecordDetail> GiftRecordDetail { get; set; }
        public virtual DbSet<Gist> Gist { get; set; }
        public virtual DbSet<GistSync> GistSync { get; set; }
        public virtual DbSet<KeyValueSynonym> KeyValueSynonym { get; set; }
        public virtual DbSet<KeyValues> KeyValues { get; set; }
        public virtual DbSet<Notepad> Notepad { get; set; }
        public virtual DbSet<Run> Run { get; set; }
        public virtual DbSet<Tags> Tags { get; set; }
        public virtual DbSet<UserConnection> UserConnection { get; set; }
        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<UserMessage> UserMessage { get; set; }
        public virtual DbSet<UserReply> UserReply { get; set; }
        public virtual DbSet<UserWriting> UserWriting { get; set; }
        public virtual DbSet<UserWritingTags> UserWritingTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<DocSet>(entity =>
            {
                entity.HasKey(e => e.DsCode)
                    .HasName("DocSet_DsCode_PK");

                entity.HasIndex(e => e.Uid)
                    .HasName("DocSet_Uid");

                entity.Property(e => e.DsCode)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.DsCreateTime).HasColumnType("datetime");

                entity.Property(e => e.DsName).HasMaxLength(50);

                entity.Property(e => e.DsRemark).HasMaxLength(200);

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DocSetDetail>(entity =>
            {
                entity.HasKey(e => e.DsdId)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.DsCode)
                    .HasName("DocSetDetail_DsCode")
                    .ForSqlServerIsClustered();

                entity.Property(e => e.DsdId)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.DsCode).HasMaxLength(50);

                entity.Property(e => e.DsdCreateTime).HasColumnType("datetime");

                entity.Property(e => e.DsdPid).HasMaxLength(50);

                entity.Property(e => e.DsdTitle).HasMaxLength(50);

                entity.Property(e => e.DsdUpdateTime).HasColumnType("datetime");

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Draw>(entity =>
            {
                entity.HasKey(e => e.DrId)
                    .HasName("Draw_DrId_PK");

                entity.HasIndex(e => e.Uid)
                    .HasName("Draw_Uid");

                entity.Property(e => e.DrId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DrCategory).HasMaxLength(20);

                entity.Property(e => e.DrCreateTime).HasColumnType("datetime");

                entity.Property(e => e.DrName).HasMaxLength(50);

                entity.Property(e => e.DrRemark).HasMaxLength(200);

                entity.Property(e => e.DrType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<GiftRecord>(entity =>
            {
                entity.HasKey(e => e.GrId)
                    .HasName("GiftRecord_GrId_PK");

                entity.HasIndex(e => e.Uid)
                    .HasName("GiftRecord_Uid");

                entity.Property(e => e.GrId)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.GrActionTime).HasColumnType("datetime");

                entity.Property(e => e.GrCreateTime).HasColumnType("datetime");

                entity.Property(e => e.GrDescription).HasMaxLength(255);

                entity.Property(e => e.GrName1).HasMaxLength(50);

                entity.Property(e => e.GrName2).HasMaxLength(50);

                entity.Property(e => e.GrName3).HasMaxLength(50);

                entity.Property(e => e.GrName4).HasMaxLength(50);

                entity.Property(e => e.GrRemark).HasMaxLength(255);

                entity.Property(e => e.GrTheme).HasMaxLength(200);

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Uid).HasMaxLength(50);
            });

            modelBuilder.Entity<GiftRecordDetail>(entity =>
            {
                entity.HasKey(e => e.GrdId)
                    .HasName("GiftRecordDetail_GrdId_PK");

                entity.HasIndex(e => e.GrId)
                    .HasName("GiftRecordDetail_Gid");

                entity.Property(e => e.GrdId)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.GrId).HasMaxLength(50);

                entity.Property(e => e.GrdCash).HasColumnType("money");

                entity.Property(e => e.GrdCreateTime).HasColumnType("datetime");

                entity.Property(e => e.GrdGiverName).HasMaxLength(50);

                entity.Property(e => e.GrdGoods).HasMaxLength(255);

                entity.Property(e => e.GrdRemark).HasMaxLength(255);

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Gist>(entity =>
            {
                entity.HasIndex(e => e.GistCode)
                    .HasName("Gist_GistCode");

                entity.HasIndex(e => e.Uid)
                    .HasName("Gist_Uid");

                entity.Property(e => e.GistId)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.GistCode).HasMaxLength(50);

                entity.Property(e => e.GistCreateTime).HasColumnType("datetime");

                entity.Property(e => e.GistFilename).HasMaxLength(50);

                entity.Property(e => e.GistLanguage).HasMaxLength(50);

                entity.Property(e => e.GistRemark).HasMaxLength(200);

                entity.Property(e => e.GistTags).HasMaxLength(200);

                entity.Property(e => e.GistTheme).HasMaxLength(50);

                entity.Property(e => e.GistUpdateTime).HasColumnType("datetime");

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<GistSync>(entity =>
            {
                entity.HasKey(e => e.GistCode)
                    .HasName("GistSync_GitCode_PK");

                entity.Property(e => e.GistCode)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.GistFilename).HasMaxLength(50);

                entity.Property(e => e.GsGitHubId).HasMaxLength(50);

                entity.Property(e => e.GsGitHubTime).HasColumnType("datetime");

                entity.Property(e => e.GsGiteeId).HasMaxLength(50);

                entity.Property(e => e.GsGiteeTime).HasColumnType("datetime");

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<KeyValueSynonym>(entity =>
            {
                entity.HasKey(e => e.KsId)
                    .HasName("KeyValueSynonym_KsId_PK");

                entity.HasIndex(e => e.KeyName)
                    .HasName("KeyValueSynonym_KeyName");

                entity.HasIndex(e => e.KsName)
                    .HasName("KeyValueSynonym_KsName");

                entity.Property(e => e.KsId)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.KeyName).HasMaxLength(255);

                entity.Property(e => e.KsName).HasMaxLength(255);

                entity.Property(e => e.Spare1).HasMaxLength(50);

                entity.Property(e => e.Spare2).HasMaxLength(50);

                entity.Property(e => e.Spare3).HasMaxLength(50);
            });

            modelBuilder.Entity<KeyValues>(entity =>
            {
                entity.HasKey(e => e.KeyId)
                    .HasName("KeyValues_KeyId_PK");

                entity.HasIndex(e => e.KeyName)
                    .HasName("KeyValues_KeyName");

                entity.Property(e => e.KeyId)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.KeyName).HasMaxLength(255);

                entity.Property(e => e.KeyRemark).HasMaxLength(50);

                entity.Property(e => e.KeyType).HasMaxLength(50);

                entity.Property(e => e.Spare1).HasMaxLength(50);

                entity.Property(e => e.Spare2).HasMaxLength(50);

                entity.Property(e => e.Spare3).HasMaxLength(50);
            });

            modelBuilder.Entity<Notepad>(entity =>
            {
                entity.HasKey(e => e.NoteId)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.Uid)
                    .HasName("Notepad_Uid")
                    .ForSqlServerIsClustered();

                entity.Property(e => e.NoteCreateTime).HasColumnType("datetime");

                entity.Property(e => e.NoteTitle)
                    .HasColumnName("NoteTItle")
                    .HasMaxLength(100);

                entity.Property(e => e.NoteUpdateTime).HasColumnType("datetime");

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Run>(entity =>
            {
                entity.HasIndex(e => e.RunCode)
                    .HasName("Run_RunCode");

                entity.HasIndex(e => e.Uid)
                    .HasName("Run_Uid");

                entity.Property(e => e.RunId)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.RunCode).HasMaxLength(50);

                entity.Property(e => e.RunCreateTime).HasColumnType("datetime");

                entity.Property(e => e.RunRemark).HasMaxLength(200);

                entity.Property(e => e.RunTags).HasMaxLength(200);

                entity.Property(e => e.RunTheme).HasMaxLength(50);

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Tags>(entity =>
            {
                entity.HasKey(e => e.TagId)
                    .HasName("Tags_TagId_PK");

                entity.HasIndex(e => new { e.TagOwner, e.TagPid, e.TagOrder })
                    .HasName("Tags_TagOwner_TagPid_TagOrder");

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TagCode).HasMaxLength(20);

                entity.Property(e => e.TagName).HasMaxLength(50);

                entity.Property(e => e.TagIcon).HasMaxLength(200);
            });

            modelBuilder.Entity<UserConnection>(entity =>
            {
                entity.HasKey(e => e.UconnId)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.Uid)
                    .HasName("UserConnection_Uid")
                    .ForSqlServerIsClustered();

                entity.Property(e => e.UconnId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UconnCreateTime).HasColumnType("datetime");

                entity.Property(e => e.UconnTargetType)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.UconnTargetId)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("UserInfo_UserId_PK");

                entity.HasIndex(e => e.UserName)
                    .HasName("UserInfo_UserName")
                    .IsUnique();

                entity.Property(e => e.Nickname).HasMaxLength(50);

                entity.Property(e => e.OpenId1).HasMaxLength(50);

                entity.Property(e => e.OpenId2).HasMaxLength(50);

                entity.Property(e => e.OpenId3).HasMaxLength(50);

                entity.Property(e => e.OpenId4).HasMaxLength(50);

                entity.Property(e => e.OpenId5).HasMaxLength(50);

                entity.Property(e => e.OpenId6).HasMaxLength(50);

                entity.Property(e => e.OpenId7).HasMaxLength(50);

                entity.Property(e => e.OpenId8).HasMaxLength(50);

                entity.Property(e => e.OpenId9).HasMaxLength(50);

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserBirthday).HasColumnType("datetime");

                entity.Property(e => e.UserCreateTime).HasColumnType("datetime");

                entity.Property(e => e.UserLoginTime).HasColumnType("datetime");

                entity.Property(e => e.UserMail).HasMaxLength(50);

                entity.Property(e => e.UserName).HasMaxLength(50);

                entity.Property(e => e.UserPhone).HasMaxLength(20);

                entity.Property(e => e.UserPhoto).HasMaxLength(200);

                entity.Property(e => e.UserPwd).HasMaxLength(50);

                entity.Property(e => e.UserSay).HasMaxLength(200);

                entity.Property(e => e.UserSign).HasMaxLength(30);

                entity.Property(e => e.UserUrl).HasMaxLength(100);
            });

            modelBuilder.Entity<UserMessage>(entity =>
            {
                entity.HasKey(e => e.UmId)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.UmType, e.Uid, e.UmCreateTime })
                    .HasName("UserMessage_TypeAndUid")
                    .ForSqlServerIsClustered();

                entity.Property(e => e.UmId)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.Spare1)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UmCreateTime).HasColumnType("datetime");

                entity.Property(e => e.UmTargetId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UmType)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserReply>(entity =>
            {
                entity.HasKey(e => e.UrId)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => new { e.UrTargetType, e.UrTargetId })
                    .HasName("UserReply_TypeAndId")
                    .ForSqlServerIsClustered();

                entity.Property(e => e.Spare1).HasMaxLength(50);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UrAnonymousLink).HasMaxLength(50);

                entity.Property(e => e.UrAnonymousMail).HasMaxLength(100);

                entity.Property(e => e.UrAnonymousName).HasMaxLength(20);

                entity.Property(e => e.UrCreateTime).HasColumnType("datetime");

                entity.Property(e => e.UrTargetId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UrTargetType)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserWriting>(entity =>
            {
                entity.HasKey(e => e.UwId)
                    .HasName("Writing_UwId_PK");

                entity.HasIndex(e => e.Uid)
                    .HasName("Writing_Uid");

                entity.Property(e => e.Spare1).HasMaxLength(50);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UwCreateTime).HasColumnType("datetime");

                entity.Property(e => e.UwUpdateTime).HasColumnType("datetime");

                entity.Property(e => e.UwLastDate).HasColumnType("datetime");

                entity.Property(e => e.UwTitle).HasMaxLength(100);
            });

            modelBuilder.Entity<UserWritingTags>(entity =>
            {
                entity.HasKey(e => e.UwtId)
                    .ForSqlServerIsClustered(false);

                entity.HasIndex(e => e.TagId)
                    .HasName("UserWritingTags_TagsId");

                entity.HasIndex(e => e.TagName)
                    .HasName("UserWritingTags_TagsName");

                entity.HasIndex(e => e.UwId)
                    .HasName("UserWritingTags_UwId")
                    .ForSqlServerIsClustered();

                entity.Property(e => e.Spare1).HasMaxLength(50);

                entity.Property(e => e.Spare2)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Spare3)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TagCode).HasMaxLength(20);

                entity.Property(e => e.TagName).HasMaxLength(50);
            });
        }
    }
}