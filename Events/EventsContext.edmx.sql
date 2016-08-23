
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 05/18/2015 20:02:02
-- Generated from EDMX file: C:\Users\Bence\documents\visual studio 2013\Projects\Events\Events\EventsContext.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Events];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_EventEventSequence]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Events] DROP CONSTRAINT [FK_EventEventSequence];
GO
IF OBJECT_ID(N'[dbo].[FK_EventLecturer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Lecturers] DROP CONSTRAINT [FK_EventLecturer];
GO
IF OBJECT_ID(N'[dbo].[FK_ParticipantEvent]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Attendees] DROP CONSTRAINT [FK_ParticipantEvent];
GO
IF OBJECT_ID(N'[dbo].[FK_SeqSponsorEventSequence]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SeqSponsors] DROP CONSTRAINT [FK_SeqSponsorEventSequence];
GO
IF OBJECT_ID(N'[dbo].[FK_SponsorEvent]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Sponsors] DROP CONSTRAINT [FK_SponsorEvent];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Attendees]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Attendees];
GO
IF OBJECT_ID(N'[dbo].[Events]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Events];
GO
IF OBJECT_ID(N'[dbo].[EventSequences]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EventSequences];
GO
IF OBJECT_ID(N'[dbo].[Lecturers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Lecturers];
GO
IF OBJECT_ID(N'[dbo].[SeqSponsors]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SeqSponsors];
GO
IF OBJECT_ID(N'[dbo].[Sponsors]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Sponsors];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Events'
CREATE TABLE [dbo].[Events] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(255)  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [Time] datetime  NOT NULL,
    [Place] nvarchar(50)  NOT NULL,
    [EventSequenceId] int  NULL,
    [AttendeeLimit] smallint  NULL,
    [TimeLength] smallint  NOT NULL,
    [Documents] nvarchar(max)  NULL,
    [AttendRequirements] nvarchar(255)  NOT NULL,
    [IsLocked] bit  NOT NULL,
    [IncludeSponsors] bit  NOT NULL,
    [AttendMessage] nvarchar(max)  NULL
);
GO

-- Creating table 'EventSequences'
CREATE TABLE [dbo].[EventSequences] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(255)  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [StartDate] datetime  NULL,
    [EndDate] datetime  NULL,
    [WeekTimes] nvarchar(50)  NULL
);
GO

-- Creating table 'Lecturers'
CREATE TABLE [dbo].[Lecturers] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(50)  NULL,
    [Email] nvarchar(50)  NULL,
    [Avatar] nvarchar(255)  NULL,
    [Url] nvarchar(255)  NULL,
    [CompanyRank] nvarchar(255)  NULL,
    [Introduction] nvarchar(max)  NULL,
    [EventId] int  NOT NULL
);
GO

-- Creating table 'Attendees'
CREATE TABLE [dbo].[Attendees] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [EventId] int  NOT NULL,
    [Name] nvarchar(255)  NULL,
    [Company] nvarchar(255)  NULL,
    [Email] nvarchar(255)  NULL,
    [Other] nvarchar(max)  NULL
);
GO

-- Creating table 'Sponsors'
CREATE TABLE [dbo].[Sponsors] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [EventId] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Url] nvarchar(max)  NOT NULL,
    [Icon] nvarchar(max)  NULL
);
GO

-- Creating table 'SeqSponsors'
CREATE TABLE [dbo].[SeqSponsors] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [EventSequenceId] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Url] nvarchar(max)  NOT NULL,
    [Icon] nvarchar(max)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Events'
ALTER TABLE [dbo].[Events]
ADD CONSTRAINT [PK_Events]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'EventSequences'
ALTER TABLE [dbo].[EventSequences]
ADD CONSTRAINT [PK_EventSequences]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Lecturers'
ALTER TABLE [dbo].[Lecturers]
ADD CONSTRAINT [PK_Lecturers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Attendees'
ALTER TABLE [dbo].[Attendees]
ADD CONSTRAINT [PK_Attendees]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Sponsors'
ALTER TABLE [dbo].[Sponsors]
ADD CONSTRAINT [PK_Sponsors]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SeqSponsors'
ALTER TABLE [dbo].[SeqSponsors]
ADD CONSTRAINT [PK_SeqSponsors]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [EventSequenceId] in table 'Events'
ALTER TABLE [dbo].[Events]
ADD CONSTRAINT [FK_EventEventSequence]
    FOREIGN KEY ([EventSequenceId])
    REFERENCES [dbo].[EventSequences]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EventEventSequence'
CREATE INDEX [IX_FK_EventEventSequence]
ON [dbo].[Events]
    ([EventSequenceId]);
GO

-- Creating foreign key on [EventId] in table 'Attendees'
ALTER TABLE [dbo].[Attendees]
ADD CONSTRAINT [FK_ParticipantEvent]
    FOREIGN KEY ([EventId])
    REFERENCES [dbo].[Events]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ParticipantEvent'
CREATE INDEX [IX_FK_ParticipantEvent]
ON [dbo].[Attendees]
    ([EventId]);
GO

-- Creating foreign key on [EventId] in table 'Sponsors'
ALTER TABLE [dbo].[Sponsors]
ADD CONSTRAINT [FK_SponsorEvent]
    FOREIGN KEY ([EventId])
    REFERENCES [dbo].[Events]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SponsorEvent'
CREATE INDEX [IX_FK_SponsorEvent]
ON [dbo].[Sponsors]
    ([EventId]);
GO

-- Creating foreign key on [EventSequenceId] in table 'SeqSponsors'
ALTER TABLE [dbo].[SeqSponsors]
ADD CONSTRAINT [FK_SeqSponsorEventSequence]
    FOREIGN KEY ([EventSequenceId])
    REFERENCES [dbo].[EventSequences]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SeqSponsorEventSequence'
CREATE INDEX [IX_FK_SeqSponsorEventSequence]
ON [dbo].[SeqSponsors]
    ([EventSequenceId]);
GO

-- Creating foreign key on [EventId] in table 'Lecturers'
ALTER TABLE [dbo].[Lecturers]
ADD CONSTRAINT [FK_EventLecturer]
    FOREIGN KEY ([EventId])
    REFERENCES [dbo].[Events]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EventLecturer'
CREATE INDEX [IX_FK_EventLecturer]
ON [dbo].[Lecturers]
    ([EventId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------