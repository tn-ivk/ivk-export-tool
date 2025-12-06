using FluentAssertions;
using IvkExportTool.Core.Constants;
using IvkExportTool.Core.Interfaces;
using IvkExportTool.Core.Models;
using IvkExportTool.Desktop.Enums;
using IvkExportTool.Desktop.ViewModels;
using Moq;

namespace IvkExportTool.Tests.Desktop.ViewModels;

[TestFixture]
public class AutoConnectionWindowViewModelTests
{
    private Mock<IDatabaseService> _mockDatabaseService = null!;
    private Mock<IAppSettingsService> _mockSettingsService = null!;

    [SetUp]
    public void Setup()
    {
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockSettingsService = new Mock<IAppSettingsService>();

        // По умолчанию LoadConnectionAsync возвращает пустую конфигурацию
        _mockSettingsService
            .Setup(s => s.LoadConnectionAsync())
            .ReturnsAsync(new ConnectionConfig());
    }

    private AutoConnectionWindowViewModel CreateViewModel()
    {
        return new AutoConnectionWindowViewModel(
            _mockDatabaseService.Object,
            _mockSettingsService.Object);
    }

    #region Validation Tests - Host

    [Test]
    public void IsHostValid_EmptyHost_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "";

        // Act & Assert
        viewModel.IsHostValid.Should().BeFalse();
    }

    [Test]
    public void IsHostValid_WhitespaceHost_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "   ";

        // Act & Assert
        viewModel.IsHostValid.Should().BeFalse();
    }

    [Test]
    public void IsHostValid_NonEmptyHost_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";

        // Act & Assert
        viewModel.IsHostValid.Should().BeTrue();
    }

    [Test]
    public void IsHostValid_IpAddress_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "192.168.1.100";

        // Act & Assert
        viewModel.IsHostValid.Should().BeTrue();
    }

    #endregion

    #region Validation Tests - Port

    [Test]
    public void IsPortValid_EmptyPort_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Port = "";

        // Act & Assert
        viewModel.IsPortValid.Should().BeFalse();
    }

    [Test]
    public void IsPortValid_InvalidPort_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Port = "abc";

        // Act & Assert
        viewModel.IsPortValid.Should().BeFalse();
    }

    [Test]
    public void IsPortValid_PortOutOfRange_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Port = "99999";

        // Act & Assert
        viewModel.IsPortValid.Should().BeFalse();
    }

    [Test]
    public void IsPortValid_ValidPort_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Port = "3306";

        // Act & Assert
        viewModel.IsPortValid.Should().BeTrue();
    }

    #endregion

    #region CanConnect Tests

    [Test]
    public void CanConnect_ValidFields_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        // Act & Assert
        viewModel.CanConnect.Should().BeTrue();
    }

    [Test]
    public void CanConnect_InvalidHost_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "";
        viewModel.Port = "3306";

        // Act & Assert
        viewModel.CanConnect.Should().BeFalse();
    }

    [Test]
    public void CanConnect_InvalidPort_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "invalid";

        // Act & Assert
        viewModel.CanConnect.Should().BeFalse();
    }

    #endregion

    #region Back Command Tests

    [Test]
    public void Back_RaisesBackRequested()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var eventRaised = false;
        viewModel.BackRequested += (_, _) => eventRaised = true;

        // Act
        viewModel.BackCommand.Execute(null);

        // Assert
        eventRaised.Should().BeTrue();
    }

    #endregion

    #region Cancel Command Tests

    [Test]
    public void Cancel_SetsCanCancelFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        viewModel.CanCancel.Should().BeFalse();
    }

    [Test]
    public void Cancel_SetsStatusMessage()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        viewModel.StatusMessage.Should().Contain("Отменено");
        viewModel.StatusType.Should().Be(StatusMessageType.Warning);
    }

    #endregion

    #region ConnectAsync Tests

    [Test]
    public async Task ConnectAsync_FirstCredentialWorks_RaisesSuccess()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        ConnectionConfig? receivedConfig = null;
        viewModel.ConnectionSucceeded += (_, config) => receivedConfig = config;

        // Первая попытка успешна
        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockDatabaseService
            .Setup(s => s.GetDatabasesAsync(It.IsAny<ConnectionConfig>()))
            .ReturnsAsync(new List<string> { "db1", "db2" });

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        receivedConfig.Should().NotBeNull();
        receivedConfig!.Host.Should().Be("localhost");
        viewModel.StatusType.Should().Be(StatusMessageType.Success);
    }

    [Test]
    public async Task ConnectAsync_AllCredentialsFail_ShowsError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        ConnectionConfig? receivedConfig = null;
        viewModel.ConnectionSucceeded += (_, config) => receivedConfig = config;

        // Все попытки неуспешны
        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        receivedConfig.Should().BeNull();
        viewModel.StatusMessage.Should().Contain("Не удалось подключиться");
        viewModel.StatusType.Should().Be(StatusMessageType.Error);
    }

    [Test]
    public async Task ConnectAsync_SetsTotalAttemptsFromCredentialsList()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        viewModel.TotalAttempts.Should().Be(DefaultCredentials.List.Count);
    }

    [Test]
    public async Task ConnectAsync_UpdatesCurrentAttempt()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        var attemptNumbers = new List<int>();

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                attemptNumbers.Add(viewModel.CurrentAttempt);
                return Task.FromResult(false);
            });

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        attemptNumbers.Should().BeInAscendingOrder();
        attemptNumbers.Should().HaveCount(DefaultCredentials.List.Count);
    }

    [Test]
    public async Task ConnectAsync_SetsIsLoadingDuringExecution()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        var isLoadingDuringCall = false;

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                isLoadingDuringCall = viewModel.IsLoading;
                return Task.FromResult(true);
            });

        _mockDatabaseService
            .Setup(s => s.GetDatabasesAsync(It.IsAny<ConnectionConfig>()))
            .ReturnsAsync(new List<string> { "db1" });

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        isLoadingDuringCall.Should().BeTrue();
        viewModel.IsLoading.Should().BeFalse();
    }

    [Test]
    public async Task ConnectAsync_SetsCanCancelDuringExecution()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        var canCancelDuringCall = false;

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                canCancelDuringCall = viewModel.CanCancel;
                return Task.FromResult(true);
            });

        _mockDatabaseService
            .Setup(s => s.GetDatabasesAsync(It.IsAny<ConnectionConfig>()))
            .ReturnsAsync(new List<string> { "db1" });

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        canCancelDuringCall.Should().BeTrue();
        viewModel.CanCancel.Should().BeFalse();
    }

    [Test]
    public async Task ConnectAsync_Cancelled_StopsIteration()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        var attemptCount = 0;

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns<ConnectionConfig, TimeSpan, CancellationToken>(async (_, _, ct) =>
            {
                attemptCount++;
                if (attemptCount == 2)
                {
                    // Симулируем отмену после второй попытки
                    viewModel.CancelCommand.Execute(null);
                }
                await Task.Delay(10, ct);
                return false;
            });

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        attemptCount.Should().BeLessThanOrEqualTo(3); // Должен остановиться после отмены
        viewModel.StatusMessage.Should().Contain("Отменено");
    }

    [Test]
    public async Task ConnectAsync_ConnectionSuccessButNoDatabases_ContinuesIteration()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        var attemptCount = 0;

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                attemptCount++;
                return Task.FromResult(true);
            });

        // Все подключения успешны, но баз данных нет
        _mockDatabaseService
            .Setup(s => s.GetDatabasesAsync(It.IsAny<ConnectionConfig>()))
            .ReturnsAsync(new List<string>());

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        attemptCount.Should().Be(DefaultCredentials.List.Count);
        viewModel.StatusMessage.Should().Contain("Не удалось подключиться");
    }

    [Test]
    public async Task ConnectAsync_Exception_ShowsError()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        viewModel.StatusMessage.Should().Contain("Network error");
        viewModel.StatusType.Should().Be(StatusMessageType.Error);
    }

    #endregion

    #region NotificationRequested Event Tests

    [Test]
    public async Task ConnectAsync_Success_RaisesNotificationRequested()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        string? notificationMessage = null;
        StatusMessageType? notificationType = null;

        viewModel.NotificationRequested += (_, args) =>
        {
            notificationMessage = args.Message;
            notificationType = args.Type;
        };

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockDatabaseService
            .Setup(s => s.GetDatabasesAsync(It.IsAny<ConnectionConfig>()))
            .ReturnsAsync(new List<string> { "db1", "db2" });

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        notificationMessage.Should().NotBeNull();
        notificationMessage.Should().Contain("Автоподключение успешно");
        notificationType.Should().Be(StatusMessageType.Success);
    }

    [Test]
    public async Task ConnectAsync_Failure_RaisesNotificationRequested()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        string? notificationMessage = null;
        StatusMessageType? notificationType = null;

        viewModel.NotificationRequested += (_, args) =>
        {
            notificationMessage = args.Message;
            notificationType = args.Type;
        };

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        notificationMessage.Should().NotBeNull();
        notificationMessage.Should().Contain("Не удалось подключиться");
        notificationType.Should().Be(StatusMessageType.Error);
    }

    #endregion

    #region Settings Loading Tests

    [Test]
    public async Task Constructor_LoadsSettingsFromService()
    {
        // Arrange
        _mockSettingsService
            .Setup(s => s.LoadConnectionAsync())
            .ReturnsAsync(new ConnectionConfig
            {
                Host = "saved-host",
                Port = 3307
            });

        // Act
        var viewModel = CreateViewModel();

        // Даем время на загрузку настроек
        await Task.Delay(100);

        // Assert
        viewModel.Host.Should().Be("saved-host");
        viewModel.Port.Should().Be("3307");
    }

    [Test]
    public async Task Constructor_LoadSettingsError_UsesDefaultValues()
    {
        // Arrange
        _mockSettingsService
            .Setup(s => s.LoadConnectionAsync())
            .ThrowsAsync(new Exception("File not found"));

        // Act
        var viewModel = CreateViewModel();

        // Даем время на загрузку настроек
        await Task.Delay(100);

        // Assert - должны остаться дефолтные значения
        viewModel.Host.Should().Be("192.168.233.101");
        viewModel.Port.Should().Be("3306");
    }

    #endregion

    #region Property Changed Notifications

    [Test]
    public void Host_Changed_NotifiesCanConnectChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var canConnectChanged = false;

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AutoConnectionWindowViewModel.CanConnect))
                canConnectChanged = true;
        };

        // Act
        viewModel.Host = "new-host";

        // Assert
        canConnectChanged.Should().BeTrue();
    }

    [Test]
    public void Port_Changed_NotifiesCanConnectChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var canConnectChanged = false;

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AutoConnectionWindowViewModel.CanConnect))
                canConnectChanged = true;
        };

        // Act
        viewModel.Port = "3307";

        // Assert
        canConnectChanged.Should().BeTrue();
    }

    #endregion

    #region StatusMessage Update Tests

    [Test]
    public async Task ConnectAsync_UpdatesStatusMessageDuringIteration()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.Host = "localhost";
        viewModel.Port = "3306";

        var statusMessages = new List<string>();

        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AutoConnectionWindowViewModel.StatusMessage))
                statusMessages.Add(viewModel.StatusMessage);
        };

        _mockDatabaseService
            .Setup(s => s.TestConnectionAsync(
                It.IsAny<ConnectionConfig>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await viewModel.ConnectCommand.ExecuteAsync(null);

        // Assert
        statusMessages.Should().Contain(m => m.Contains("Попытка"));
    }

    #endregion
}
