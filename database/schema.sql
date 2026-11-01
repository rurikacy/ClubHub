-- Run only against a new, empty database. Existing tables are dropped.
-- ----------------------------
-- Table structure for T_Active
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Active]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Active]
GO

CREATE TABLE [dbo].[T_Active] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [ClubId] int  NOT NULL,
  [AdminId] int  NOT NULL,
  [Title] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Content] nvarchar(200) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL,
  [StartTime] datetime  NOT NULL,
  [EndTime] datetime  NOT NULL,
  [JoinNum] int DEFAULT 0 NOT NULL,
  [Place] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [MinParticipants] int DEFAULT 0 NOT NULL,
  [Status] tinyint DEFAULT 0 NOT NULL,
  [CancelReason] nvarchar(200) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[T_Active] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_ActiveSign
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_ActiveSign]') AND type IN ('U'))
	DROP TABLE [dbo].[T_ActiveSign]
GO

CREATE TABLE [dbo].[T_ActiveSign] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [MemberId] int  NOT NULL,
  [ActiveId] int  NOT NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL,
  [SignType] tinyint  NOT NULL
)
GO

ALTER TABLE [dbo].[T_ActiveSign] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_Admin
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Admin]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Admin]
GO

CREATE TABLE [dbo].[T_Admin] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [StudentId] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Password] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Name] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Sex] tinyint  NOT NULL,
  [PhoneNumber] nvarchar(11) COLLATE Chinese_PRC_CI_AS  NULL,
  [AdminType] tinyint  NOT NULL,
  [ClubId] int  NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL
)
GO

ALTER TABLE [dbo].[T_Admin] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_Club
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Club]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Club]
GO

CREATE TABLE [dbo].[T_Club] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [Name] nvarchar(20) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Description] nvarchar(200) COLLATE Chinese_PRC_CI_AS  NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL
)
GO

ALTER TABLE [dbo].[T_Club] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_Duty
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Duty]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Duty]
GO

CREATE TABLE [dbo].[T_Duty] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [ClubId] int  NOT NULL,
  [Name] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Description] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [MemberNum] int DEFAULT 0 NOT NULL
)
GO

ALTER TABLE [dbo].[T_Duty] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_DutyMember
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_DutyMember]') AND type IN ('U'))
	DROP TABLE [dbo].[T_DutyMember]
GO

CREATE TABLE [dbo].[T_DutyMember] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [DutyId] int  NOT NULL,
  [MemberId] int  NOT NULL,
  [Role] tinyint  NOT NULL
)
GO

ALTER TABLE [dbo].[T_DutyMember] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_Enroll
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Enroll]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Enroll]
GO

CREATE TABLE [dbo].[T_Enroll] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [ClubId] int  NOT NULL,
  [MemberId] int  NOT NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL,
  [HandType] tinyint  NULL,
  [HandInfo] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NULL,
  [HandDate] datetime  NULL
)
GO

ALTER TABLE [dbo].[T_Enroll] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_Member
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Member]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Member]
GO

CREATE TABLE [dbo].[T_Member] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [StudentId] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Password] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Name] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Sex] tinyint  NOT NULL,
  [PhoneNumber] nvarchar(11) COLLATE Chinese_PRC_CI_AS  NULL,
  [ClubId] int  NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL
)
GO

ALTER TABLE [dbo].[T_Member] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_Message
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Message]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Message]
GO

CREATE TABLE [dbo].[T_Message] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [ClubId] int  NOT NULL,
  [AdminId] int  NOT NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL,
  [Title] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Content] nvarchar(500) COLLATE Chinese_PRC_CI_AS  NOT NULL
)
GO

ALTER TABLE [dbo].[T_Message] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_Module
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Module]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Module]
GO

CREATE TABLE [dbo].[T_Module] (
  [id] int  NOT NULL,
  [Name] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [Url] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [Icon] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [ParentId] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [Description] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [Type] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[T_Module] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Records of T_Module
-- ----------------------------
INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1', N'用户管理', NULL, NULL, N'0', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'3', N'成员管理', N'/ClubHub/Member/Index', NULL, N'1', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'4', N'成员活动管理', N'/ClubHub/Active/Index', NULL, N'1', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'101', N'社团管理 ', NULL, NULL, N'0', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'4', N'所有用户', N'/ClubHub/User/Index', NULL, N'1', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'103', N'部门管理', N'/ClubHub/Duty/Index', NULL, N'101', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'102', N'社团管理', N'/ClubHub/Club/Index', NULL, N'101', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1015', N'成员工作台', NULL, NULL, N'0', NULL, N'2')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1016', N'活动中心', N'/ClubHub/Active/MemberIndex', NULL, N'1015', NULL, N'2')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1018', N'社团公告', N'/ClubHub/Message/MembertIndex', NULL, N'1015', NULL, N'2')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2017', N'社团工作台', NULL, NULL, N'0', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2018', N'成员管理', N'/ClubHub/Member/Index', NULL, N'2017', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2023', N'社团信息', N'/ClubHub/Club/Admin/Index', NULL, N'2017', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2020', N'活动管理', N'/ClubHub/Active/Index', NULL, N'2017', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2021', N'公告管理', N'/ClubHub/Message/Index', NULL, N'2017', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2', N'社长管理', N'/ClubHub/Admin/index', NULL, N'1', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2019', N'部门管理', N'/ClubHub/Duty/Index', NULL, N'2017', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2022', N'报名管理', N'/ClubHub/Enroll/Approve/Index', NULL, N'2017', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'202', N'个人信息', N'/ClubHub/SpAdmin/Info', NULL, N'201', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1023', N'个人信息', N'/ClubHub/Member/Info', NULL, N'1015', NULL, N'2')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2024', N'个人信息', N'/ClubHub/Admin/Info', NULL, N'2017', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1021', N'社团广场', N'/ClubHub/Member/ClubInfo', NULL, N'1015', NULL, N'2')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1019', N'我的活动', N'/ClubHub/Member/MyActive', NULL, N'1015', NULL, N'2')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1022', N'我的问卷', N'/ClubHub/Member/Question', NULL, N'1015', NULL, N'2')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2025', N'问卷管理', N'/ClubHub/Admin/Question', NULL, N'2017', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'105', N'问卷管理', N'/ClubHub/SpAdmin/Question', NULL, N'101', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'201', N'个人信息', NULL, NULL, N'0', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1020', N'社团报名', N'/ClubHub/Enroll/Index', NULL, N'1015', NULL, N'2')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'104', N'报名管理', N'/ClubHub/Enroll/Approve/Index', NULL, N'101', NULL, N'0')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'1024', N'讨论中心', N'/ClubHub/Member/Talk', NULL, N'1015', NULL, N'2')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'2026', N'讨论中心', N'/ClubHub/Admin/Talk', NULL, N'2017', NULL, N'1')
GO

INSERT INTO [dbo].[T_Module] ([id], [Name], [Url], [Icon], [ParentId], [Description], [Type]) VALUES (N'106', N'讨论中心管理', N'/ClubHub/SpAdmin/Talk', NULL, N'101', NULL, N'0')
GO


-- ----------------------------
-- Table structure for T_Question
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Question]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Question]
GO

CREATE TABLE [dbo].[T_Question] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [ClubId] int  NOT NULL,
  [AdminId] int  NOT NULL,
  [Title] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Description] nvarchar(500) COLLATE Chinese_PRC_CI_AS  NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL,
  [Status] tinyint DEFAULT 1 NOT NULL
)
GO

ALTER TABLE [dbo].[T_Question] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_QuestionAnswer
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_QuestionAnswer]') AND type IN ('U'))
	DROP TABLE [dbo].[T_QuestionAnswer]
GO

CREATE TABLE [dbo].[T_QuestionAnswer] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [QuestionId] int  NOT NULL,
  [ItemId] int  NOT NULL,
  [MemberId] int  NOT NULL,
  [Answer] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL
)
GO

ALTER TABLE [dbo].[T_QuestionAnswer] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_QuestionItem
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_QuestionItem]') AND type IN ('U'))
	DROP TABLE [dbo].[T_QuestionItem]
GO

CREATE TABLE [dbo].[T_QuestionItem] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [QuestionId] int  NOT NULL,
  [Content] nvarchar(500) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [ItemType] tinyint  NOT NULL,
  [Options] nvarchar(2000) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [SortOrder] int DEFAULT 0 NOT NULL
)
GO

ALTER TABLE [dbo].[T_QuestionItem] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Table structure for T_Talk
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_Talk]') AND type IN ('U'))
	DROP TABLE [dbo].[T_Talk]
GO

CREATE TABLE [dbo].[T_Talk] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [ClubId] int  NOT NULL,
  [SenderId] int  NOT NULL,
  [SenderType] tinyint  NOT NULL,
  [Content] nvarchar(1000) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [SendTime] datetime DEFAULT getdate() NOT NULL,
  [Status] tinyint DEFAULT 0 NOT NULL
)
GO

ALTER TABLE [dbo].[T_Talk] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'消息ID',
'SCHEMA', N'dbo',
'TABLE', N'T_Talk',
'COLUMN', N'id'
GO

EXEC sp_addextendedproperty
'MS_Description', N'所属社团ID',
'SCHEMA', N'dbo',
'TABLE', N'T_Talk',
'COLUMN', N'ClubId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'发送人ID',
'SCHEMA', N'dbo',
'TABLE', N'T_Talk',
'COLUMN', N'SenderId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'发送人类型: 0=成员, 1=管理员, 2=超管',
'SCHEMA', N'dbo',
'TABLE', N'T_Talk',
'COLUMN', N'SenderType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'消息内容',
'SCHEMA', N'dbo',
'TABLE', N'T_Talk',
'COLUMN', N'Content'
GO

EXEC sp_addextendedproperty
'MS_Description', N'发送时间',
'SCHEMA', N'dbo',
'TABLE', N'T_Talk',
'COLUMN', N'SendTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'状态: 0=正常, 1=已删除',
'SCHEMA', N'dbo',
'TABLE', N'T_Talk',
'COLUMN', N'Status'
GO

EXEC sp_addextendedproperty
'MS_Description', N'讨论中心消息表',
'SCHEMA', N'dbo',
'TABLE', N'T_Talk'
GO


-- ----------------------------
-- Table structure for T_User
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[T_User]') AND type IN ('U'))
	DROP TABLE [dbo].[T_User]
GO

CREATE TABLE [dbo].[T_User] (
  [id] int  IDENTITY(1,1) NOT NULL,
  [StudentId] nvarchar(10) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Password] nvarchar(100) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Name] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Sex] tinyint  NOT NULL,
  [PhoneNumber] nvarchar(11) COLLATE Chinese_PRC_CI_AS  NULL,
  [UserType] tinyint  NOT NULL,
  [EnterData] datetime DEFAULT getdate() NOT NULL
)
GO

ALTER TABLE [dbo].[T_User] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Auto increment value for T_Active
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_Active]', RESEED, 14)
GO


-- ----------------------------
-- Primary Key structure for table T_Active
-- ----------------------------
ALTER TABLE [dbo].[T_Active] ADD CONSTRAINT [PK__T_Active__3213E83FF139CA5B] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_ActiveSign
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_ActiveSign]', RESEED, 12)
GO


-- ----------------------------
-- Checks structure for table T_ActiveSign
-- ----------------------------
ALTER TABLE [dbo].[T_ActiveSign] ADD CONSTRAINT [CK_T_ActiveSign_SignType] CHECK ([SignType]=(3) OR [SignType]=(2) OR [SignType]=(1) OR [SignType]=(0))
GO


-- ----------------------------
-- Primary Key structure for table T_ActiveSign
-- ----------------------------
ALTER TABLE [dbo].[T_ActiveSign] ADD CONSTRAINT [PK__T_Active__3213E83FEEBA212B] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_Admin
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_Admin]', RESEED, 9)
GO


-- ----------------------------
-- Indexes structure for table T_Admin
-- ----------------------------
CREATE UNIQUE NONCLUSTERED INDEX [IX_T_Admin_StudentId]
ON [dbo].[T_Admin] (
  [StudentId] ASC
)
GO


-- ----------------------------
-- Triggers structure for table T_Admin
-- ----------------------------
CREATE TRIGGER [dbo].[trg_T_Admin_Delete]
ON [dbo].[T_Admin]
WITH EXECUTE AS CALLER
FOR DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DELETE U
    FROM dbo.T_User AS U
    JOIN deleted AS d ON U.StudentId = d.StudentId;
END;
GO

CREATE TRIGGER [dbo].[trg_T_Admin_Insert]
ON [dbo].[T_Admin]
WITH EXECUTE AS CALLER
FOR INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.T_User (StudentId, Password, Name, Sex, PhoneNumber, UserType, EnterData)
    SELECT
        i.StudentId,
        i.Password,
        i.Name,
        i.Sex,
        i.PhoneNumber,
        CASE WHEN i.AdminType = 0 THEN 0 ELSE 1 END AS UserType,
        i.EnterData
    FROM inserted AS i;
END;
GO

CREATE TRIGGER [dbo].[trg_T_Admin_Update]
ON [dbo].[T_Admin]
WITH EXECUTE AS CALLER
FOR UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE U
    SET
        U.StudentId  = i.StudentId,
        U.Password   = i.Password,
        U.Name       = i.Name,
        U.Sex        = i.Sex,
        U.PhoneNumber= i.PhoneNumber,
        U.UserType   = CASE WHEN i.AdminType = 0 THEN 0 ELSE 1 END,
        U.EnterData  = i.EnterData
    FROM dbo.T_User AS U
    JOIN deleted AS d ON U.StudentId = d.StudentId
    JOIN inserted AS i ON i.id = d.id;
END;
GO


-- ----------------------------
-- Checks structure for table T_Admin
-- ----------------------------
ALTER TABLE [dbo].[T_Admin] ADD CONSTRAINT [CK_T_Admin_Sex] CHECK ([Sex]=(2) OR [Sex]=(1) OR [Sex]=(0))
GO

ALTER TABLE [dbo].[T_Admin] ADD CONSTRAINT [CK_T_Admin_AdminType] CHECK ([AdminType]=(1) OR [AdminType]=(0))
GO

ALTER TABLE [dbo].[T_Admin] ADD CONSTRAINT [CK_T_Admin_AdminType_Club] CHECK ([AdminType]=(0) AND [ClubId] IS NULL OR [AdminType]=(1) AND [ClubId] IS NOT NULL)
GO


-- ----------------------------
-- Primary Key structure for table T_Admin
-- ----------------------------
ALTER TABLE [dbo].[T_Admin] ADD CONSTRAINT [PK__T_Admin__3213E83FED6EA18C] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_Club
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_Club]', RESEED, 42)
GO


-- ----------------------------
-- Primary Key structure for table T_Club
-- ----------------------------
ALTER TABLE [dbo].[T_Club] ADD CONSTRAINT [PK__T_Club__3213E83F0CD60A97] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_Duty
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_Duty]', RESEED, 6)
GO


-- ----------------------------
-- Primary Key structure for table T_Duty
-- ----------------------------
ALTER TABLE [dbo].[T_Duty] ADD CONSTRAINT [PK__T_Duty__3213E83F5AB88A12] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_DutyMember
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_DutyMember]', RESEED, 5)
GO


-- ----------------------------
-- Checks structure for table T_DutyMember
-- ----------------------------
ALTER TABLE [dbo].[T_DutyMember] ADD CONSTRAINT [CK_T_DutyMember_Role] CHECK ([Role]=(1) OR [Role]=(0))
GO


-- ----------------------------
-- Primary Key structure for table T_DutyMember
-- ----------------------------
ALTER TABLE [dbo].[T_DutyMember] ADD CONSTRAINT [PK__T_DutyMe__3213E83F180C3A17] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_Enroll
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_Enroll]', RESEED, 6)
GO


-- ----------------------------
-- Checks structure for table T_Enroll
-- ----------------------------
ALTER TABLE [dbo].[T_Enroll] ADD CONSTRAINT [CK_T_Enroll_HandType] CHECK ([HandType]=(1) OR [HandType]=(0))
GO


-- ----------------------------
-- Primary Key structure for table T_Enroll
-- ----------------------------
ALTER TABLE [dbo].[T_Enroll] ADD CONSTRAINT [PK__T_Enroll__3213E83F8E104F4F] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_Member
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_Member]', RESEED, 22)
GO


-- ----------------------------
-- Indexes structure for table T_Member
-- ----------------------------
CREATE UNIQUE NONCLUSTERED INDEX [IX_T_Member_StudentId]
ON [dbo].[T_Member] (
  [StudentId] ASC
)
GO


-- ----------------------------
-- Triggers structure for table T_Member
-- ----------------------------
CREATE TRIGGER [dbo].[trg_T_Member_Delete]
ON [dbo].[T_Member]
WITH EXECUTE AS CALLER
FOR DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DELETE U
    FROM dbo.T_User AS U
    JOIN deleted AS d ON U.StudentId = d.StudentId;
END;
GO

CREATE TRIGGER [dbo].[trg_T_Member_Insert]
ON [dbo].[T_Member]
WITH EXECUTE AS CALLER
FOR INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.T_User (StudentId, Password, Name, Sex, PhoneNumber, UserType, EnterData)
    SELECT 
        i.StudentId,
        i.Password,
        i.Name,
        i.Sex,
        i.PhoneNumber,
        2 AS UserType,      -- 2 = 成员
        i.EnterData         -- 保持与成员表一致
    FROM inserted AS i;
END;
GO

CREATE TRIGGER [dbo].[trg_T_Member_Update]
ON [dbo].[T_Member]
WITH EXECUTE AS CALLER
FOR UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- 按成员原来的 StudentId 在 T_User 中更新数据
    UPDATE U
    SET 
        U.StudentId  = i.StudentId,
        U.Password   = i.Password,
        U.Name       = i.Name,
        U.Sex        = i.Sex,
        U.PhoneNumber= i.PhoneNumber,
        U.UserType   = 2,          -- 成员
        U.EnterData  = i.EnterData
    FROM dbo.T_User AS U
    JOIN deleted AS d ON U.StudentId = d.StudentId
    JOIN inserted AS i ON i.id = d.id;  -- 同一条成员记录
END;
GO


-- ----------------------------
-- Checks structure for table T_Member
-- ----------------------------
ALTER TABLE [dbo].[T_Member] ADD CONSTRAINT [CK_T_Member_Sex] CHECK ([Sex]=(2) OR [Sex]=(1) OR [Sex]=(0))
GO


-- ----------------------------
-- Primary Key structure for table T_Member
-- ----------------------------
ALTER TABLE [dbo].[T_Member] ADD CONSTRAINT [PK__T_Member__3213E83F5FCEF740] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_Message
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_Message]', RESEED, 2)
GO


-- ----------------------------
-- Primary Key structure for table T_Message
-- ----------------------------
ALTER TABLE [dbo].[T_Message] ADD CONSTRAINT [PK__T_Messag__3213E83FC360FE5A] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_Question
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_Question]', RESEED, 3)
GO


-- ----------------------------
-- Indexes structure for table T_Question
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_T_Question_ClubId]
ON [dbo].[T_Question] (
  [ClubId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table T_Question
-- ----------------------------
ALTER TABLE [dbo].[T_Question] ADD CONSTRAINT [PK_T_Question] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_QuestionAnswer
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_QuestionAnswer]', RESEED, 4)
GO


-- ----------------------------
-- Indexes structure for table T_QuestionAnswer
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_T_QuestionAnswer_QuestionId]
ON [dbo].[T_QuestionAnswer] (
  [QuestionId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_T_QuestionAnswer_MemberId]
ON [dbo].[T_QuestionAnswer] (
  [MemberId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table T_QuestionAnswer
-- ----------------------------
ALTER TABLE [dbo].[T_QuestionAnswer] ADD CONSTRAINT [PK_T_QuestionAnswer] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_QuestionItem
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_QuestionItem]', RESEED, 6)
GO


-- ----------------------------
-- Indexes structure for table T_QuestionItem
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_T_QuestionItem_QuestionId]
ON [dbo].[T_QuestionItem] (
  [QuestionId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table T_QuestionItem
-- ----------------------------
ALTER TABLE [dbo].[T_QuestionItem] ADD CONSTRAINT [PK_T_QuestionItem] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_Talk
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_Talk]', RESEED, 11)
GO


-- ----------------------------
-- Indexes structure for table T_Talk
-- ----------------------------
CREATE NONCLUSTERED INDEX [IX_T_Talk_ClubId]
ON [dbo].[T_Talk] (
  [ClubId] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_T_Talk_SenderId_SenderType]
ON [dbo].[T_Talk] (
  [SenderId] ASC,
  [SenderType] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_T_Talk_SendTime]
ON [dbo].[T_Talk] (
  [SendTime] DESC
)
GO

CREATE NONCLUSTERED INDEX [IX_T_Talk_ClubId_SendTime]
ON [dbo].[T_Talk] (
  [ClubId] ASC,
  [SendTime] DESC
)
GO


-- ----------------------------
-- Primary Key structure for table T_Talk
-- ----------------------------
ALTER TABLE [dbo].[T_Talk] ADD CONSTRAINT [PK_T_Talk] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Auto increment value for T_User
-- ----------------------------
DBCC CHECKIDENT ('[dbo].[T_User]', RESEED, 15)
GO


-- ----------------------------
-- Indexes structure for table T_User
-- ----------------------------
CREATE UNIQUE NONCLUSTERED INDEX [IX_T_User_StudentId]
ON [dbo].[T_User] (
  [StudentId] ASC
)
GO


-- ----------------------------
-- Checks structure for table T_User
-- ----------------------------
ALTER TABLE [dbo].[T_User] ADD CONSTRAINT [CK_T_User_Sex] CHECK ([Sex]=(2) OR [Sex]=(1) OR [Sex]=(0))
GO

ALTER TABLE [dbo].[T_User] ADD CONSTRAINT [CK_T_User_UserType] CHECK ([UserType]=(2) OR [UserType]=(1) OR [UserType]=(0))
GO


-- ----------------------------
-- Primary Key structure for table T_User
-- ----------------------------
ALTER TABLE [dbo].[T_User] ADD CONSTRAINT [PK__T_User__3213E83F67D59876] PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Foreign Keys structure for table T_Active
-- ----------------------------
ALTER TABLE [dbo].[T_Active] ADD CONSTRAINT [FK_T_Active_Club] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[T_Club] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[T_Active] ADD CONSTRAINT [FK_T_Active_Admin] FOREIGN KEY ([AdminId]) REFERENCES [dbo].[T_Admin] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table T_ActiveSign
-- ----------------------------
ALTER TABLE [dbo].[T_ActiveSign] ADD CONSTRAINT [FK_T_ActiveSign_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[T_Member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[T_ActiveSign] ADD CONSTRAINT [FK_T_ActiveSign_Active] FOREIGN KEY ([ActiveId]) REFERENCES [dbo].[T_Active] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table T_Admin
-- ----------------------------
ALTER TABLE [dbo].[T_Admin] ADD CONSTRAINT [FK_T_Admin_Club] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[T_Club] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table T_Duty
-- ----------------------------
ALTER TABLE [dbo].[T_Duty] ADD CONSTRAINT [FK_T_Duty_Club] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[T_Club] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table T_DutyMember
-- ----------------------------
ALTER TABLE [dbo].[T_DutyMember] ADD CONSTRAINT [FK_T_DutyMember_Duty] FOREIGN KEY ([DutyId]) REFERENCES [dbo].[T_Duty] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[T_DutyMember] ADD CONSTRAINT [FK_T_DutyMember_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[T_Member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table T_Enroll
-- ----------------------------
ALTER TABLE [dbo].[T_Enroll] ADD CONSTRAINT [FK_T_Enroll_Club] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[T_Club] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[T_Enroll] ADD CONSTRAINT [FK_T_Enroll_Member] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[T_Member] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table T_Member
-- ----------------------------
ALTER TABLE [dbo].[T_Member] ADD CONSTRAINT [FK_T_Member_Club] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[T_Club] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table T_Message
-- ----------------------------
ALTER TABLE [dbo].[T_Message] ADD CONSTRAINT [FK_T_Message_Club] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[T_Club] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[T_Message] ADD CONSTRAINT [FK_T_Message_Admin] FOREIGN KEY ([AdminId]) REFERENCES [dbo].[T_Admin] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

