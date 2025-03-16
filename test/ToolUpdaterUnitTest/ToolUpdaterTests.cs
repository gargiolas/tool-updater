using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ToolUpdater;

namespace ToolUpdaterUnitTest
{
    public class ToolUpdaterTests
    {
        [Fact]
        public void GetInstalledGlobalTools_ShouldReturnListOfTools()
        {
            // Arrange
            var mockExecutor = new Mock<ICommandExecutor>();
            mockExecutor
                .Setup(e => e.Execute("dotnet", "tool list --global"))
                .Returns(["Tool1", "Tool2"]);

            var services = new ServiceCollection();
            services.AddTransient(_ => mockExecutor.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var tools = GetInstalledGlobalTools(serviceProvider);

            // Assert
            Assert.NotNull(tools);
            Assert.Equal(2, tools.Length);
            Assert.Contains("Tool1", tools);
            Assert.Contains("Tool2", tools);
        }

        [Fact]
        public void GetInstalledGlobalTools_ShouldReturnEmptyArrayWhenNoToolsAreInstalled()
        {
            // Arrange
            var mockExecutor = new Mock<ICommandExecutor>();
            mockExecutor
                .Setup(e => e.Execute("dotnet", "tool list --global"))
                .Returns([]);

            var services = new ServiceCollection();
            services.AddTransient(_ => mockExecutor.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var tools = GetInstalledGlobalTools(serviceProvider);

            // Assert
            Assert.NotNull(tools);
            Assert.Empty(tools);
        }

        [Fact]
        public void CheckForUpdate_WithUpdateAvailable_ShouldReturnTrue()
        {
            // Arrange: Mock del wrapper con l'output fittizio
            var mockProcess = CreateMockProcess("Update available!");
            var processWrapper = mockProcess.Object;

            // Act: Usare il wrapper nel metodo
            var result = CheckForUpdate(processWrapper);

            // Assert: Risultato corretto
            Assert.True(result);
        }

        [Fact]
        public void CheckForUpdate_WithoutUpdateAvailable_ShouldReturnFalse()
        {
            // Arrange
            var mockProcess = CreateMockProcess("Tool 'TestTool' is up to date");
            var processWrapper = mockProcess.Object;

            // Act
            var result = CheckForUpdate(mockProcess.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UpdateTool_WithSuccessfulUpdate_ShouldReturnTrue()
        {
            // Arrange: simula un processo con output positivo
            var mockProcess = new Mock<IProcessExecutor>();
            mockProcess.Setup(p => p.StandardOutput).Returns("Update completed");
            mockProcess.Setup(p => p.HasExited).Returns(true);

            // Act: testa il metodo con il processo simulato
            var result = UpdateTool(mockProcess.Object);

            // Assert: verifica che l'aggiornamento sia stato segnato come riuscito
            Assert.True(result);
        }

        [Fact]
        public void UpdateTool_WithSuccessfulUpdate_ShouldReturnFalse()
        {
            // Arrange: Simuliamo un processo che non restituisce l'output di successo
            var mockProcess = new Mock<IProcessExecutor>();
            mockProcess.Setup(p => p.StandardOutput).Returns("Update failed"); // Output diverso da "Update completed"
            mockProcess.Setup(p => p.HasExited).Returns(true);

            // Act: Chiamiamo il metodo con il processo simulato
            var result = UpdateTool(mockProcess.Object);

            // Assert: Verifichiamo che il metodo restituisca false
            Assert.False(result);
        }

        private Mock<IProcessExecutor> CreateMockProcess(string standardOutput)
        {
            var mockProcess = new Mock<IProcessExecutor>();
            mockProcess.Setup(p => p.StandardOutput).Returns(standardOutput);
            mockProcess.Setup(p => p.HasExited).Returns(true);

            return mockProcess;
        }

        // Functions for testing (simulated versions for the test)
        private string[] GetInstalledGlobalTools(IServiceProvider serviceProvider)
        {
            using var serviceScope = serviceProvider.CreateScope();
            var provider = serviceScope.ServiceProvider;
            var commandExecutor = provider.GetRequiredService<ICommandExecutor>();

            var output = commandExecutor.Execute("dotnet", "tool list --global");
            return output;
        }

        private static bool CheckForUpdate(IProcessExecutor mockProcess)
        {
            mockProcess.Start(It.IsAny<ProcessStartInfo>());
            mockProcess.WaitForExit();
            var output = mockProcess.StandardOutput;
            return output.Contains("Update available!");
        }

        private static bool UpdateTool(IProcessExecutor mockProcess)
        {
            try
            {
                // Avvia il processo simulato
                mockProcess.Start(It.IsAny<ProcessStartInfo>());

                // Aspetta la sua conclusione
                mockProcess.WaitForExit();

                // Leggi l'output standard per determinare se l'aggiornamento è stato effettuato
                var output = mockProcess.StandardOutput;

                // Logica per verificare se l'update è stato completato con successo
                if (output.Contains("Update completed"))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Puoi aggiungere log per debug o gestire l'eccezione
                return false;
            }
        }
    }
}