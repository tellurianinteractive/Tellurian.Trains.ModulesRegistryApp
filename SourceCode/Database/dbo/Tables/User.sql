CREATE TABLE [dbo].[User] (
    [Id]                               INT                IDENTITY (1, 1) NOT NULL,
    [ObjectId]                         UNIQUEIDENTIFIER   NOT NULL,
    [EmailAddress]                     NVARCHAR (255)     NOT NULL,
    [RegistrationTime]                 DATETIMEOFFSET (7) CONSTRAINT [DF_User_RegistrationTime] DEFAULT (getdate()) NOT NULL,
    [LastSignInTime]                   DATETIMEOFFSET (7) NULL,
    [LastEmailConfirmationTime]        DATETIMEOFFSET (7) NULL,
    [LastTermsOfUseAcceptTime]         DATETIMEOFFSET (7) NULL,
    [HashedPassword]                   NVARCHAR (255)     NULL,
    [AdministratorAreaOfResposibility] NVARCHAR (50)      NULL,
    [IsGlobalAdministrator]            BIT                DEFAULT ((0)) NOT NULL,
    [IsCountryAdministrator]           BIT                DEFAULT ((0)) NOT NULL,
    [IsReadOnly]                       BIT                DEFAULT ((0)) NOT NULL,
    [IsDemo]                           BIT                DEFAULT ((0)) NOT NULL,
    [IsApiAccessPermitted]             BIT                DEFAULT ((0)) NOT NULL,
    [PasswordResetAttempts]            INT                DEFAULT ((0)) NOT NULL,
    [FailedLoginAttempts]              INT                DEFAULT ((0)) NOT NULL,
    [MayUploadSkpDrawing]              BIT                DEFAULT ((0)) NOT NULL, 
    [MayManageWiFreds]                 BIT                DEFAULT ((0)) NOT NULL,     
    
    [DeletedTimestamp] DATETIMEOFFSET NULL, 
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO


