using FilePoller.Entities;

namespace FilePoller.Tests.Entities;

public class JobsTests
{
    [Fact]
    public void Jobs_Id_CanBeSet()
    {
        // Arrange
        var job = new Jobs { Id = 1 };

        // Act & Assert
        Assert.Equal(1, job.Id);
    }

    [Fact]
    public void Jobs_JobId_CanBeSet()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new Jobs { JobId = jobId };

        // Act & Assert
        Assert.Equal(jobId, job.JobId);
    }

    [Fact]
    public void Jobs_StartDateTime_CanBeSet()
    {
        // Arrange
        var startTime = DateTime.Now;
        var job = new Jobs { StartDateTime = startTime };

        // Act & Assert
        Assert.Equal(startTime, job.StartDateTime);
    }

    [Fact]
    public void Jobs_StopDateTime_CanBeNull()
    {
        // Arrange
        var job = new Jobs { StopDateTime = null };

        // Act & Assert
        Assert.Null(job.StopDateTime);
    }

    [Fact]
    public void Jobs_StopDateTime_CanBeSet()
    {
        // Arrange
        var stopTime = DateTime.Now;
        var job = new Jobs { StopDateTime = stopTime };

        // Act & Assert
        Assert.Equal(stopTime, job.StopDateTime);
    }

    [Fact]
    public void Jobs_JobUser_CanBeSet()
    {
        // Arrange
        var job = new Jobs { JobUser = "testuser" };

        // Act & Assert
        Assert.Equal("testuser", job.JobUser);
    }

    [Fact]
    public void Jobs_AllPropertiesCanBeSet()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var startTime = DateTime.Now;
        var stopTime = startTime.AddHours(1);

        var job = new Jobs
        {
            Id = 1,
            JobId = jobId,
            StartDateTime = startTime,
            StopDateTime = stopTime,
            JobUser = "testuser"
        };

        // Act & Assert
        Assert.Equal(1, job.Id);
        Assert.Equal(jobId, job.JobId);
        Assert.Equal(startTime, job.StartDateTime);
        Assert.Equal(stopTime, job.StopDateTime);
        Assert.Equal("testuser", job.JobUser);
    }

    [Fact]
    public void Jobs_IsActiveWhenStopDateTimeIsNull()
    {
        // Arrange
        var job = new Jobs
        {
            Id = 1,
            JobId = Guid.NewGuid(),
            StartDateTime = DateTime.Now,
            StopDateTime = null,
            JobUser = "testuser"
        };

        // Act & Assert
        Assert.Null(job.StopDateTime);
    }
}

public class DirectoriesTests
{
    [Fact]
    public void Directories_Id_CanBeSet()
    {
        // Arrange
        var directory = new Directories { Id = 1 };

        // Act & Assert
        Assert.Equal(1, directory.Id);
    }

    [Fact]
    public void Directories_UncPath_CanBeSet()
    {
        // Arrange
        var directory = new Directories { UncPath = @"\\server\share" };

        // Act & Assert
        Assert.Equal(@"\\server\share", directory.UncPath);
    }

    [Fact]
    public void Directories_Name_CanBeSet()
    {
        // Arrange
        var directory = new Directories { Name = "downloads" };

        // Act & Assert
        Assert.Equal("downloads", directory.Name);
    }

    [Fact]
    public void Directories_Type_CanBeSet()
    {
        // Arrange
        var directory = new Directories { Type = "source" };

        // Act & Assert
        Assert.Equal("source", directory.Type);
    }

    [Fact]
    public void Directories_AllPropertiesCanBeSet()
    {
        // Arrange
        var directory = new Directories
        {
            Id = 1,
            UncPath = @"\\server\share",
            Name = "downloads",
            Type = "source"
        };

        // Act & Assert
        Assert.Equal(1, directory.Id);
        Assert.Equal(@"\\server\share", directory.UncPath);
        Assert.Equal("downloads", directory.Name);
        Assert.Equal("source", directory.Type);
    }

    [Fact]
    public void Directories_WithDifferentUncPaths()
    {
        // Arrange & Act
        var directory1 = new Directories { UncPath = @"\\server1\share" };
        var directory2 = new Directories { UncPath = @"\\server2\share" };

        // Assert
        Assert.NotEqual(directory1.UncPath, directory2.UncPath);
    }

    [Fact]
    public void Directories_WithDifferentNames()
    {
        // Arrange & Act
        var directory1 = new Directories { Name = "downloads" };
        var directory2 = new Directories { Name = "uploads" };

        // Assert
        Assert.NotEqual(directory1.Name, directory2.Name);
    }
}
