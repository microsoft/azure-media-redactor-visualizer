using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace AzureMediaRedactor
{
    static class ModuleLoader
    {
        private static CompositionContainer GetCompositionContainer()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DirectoryCatalog catalog = new DirectoryCatalog(directory);
            return new CompositionContainer(catalog);
        }

        public static T Load<T>()
        {
            return GetCompositionContainer().GetExportedValue<T>();
        }

        public static void Load<T>(string contractName, out T value)
        {
            value = GetCompositionContainer().GetExportedValue<T>(contractName);
        }

        public static void Load<T, U>(string contractName, string parameterName, U parameterValue, out T value)
        {
            CompositionContainer container = GetCompositionContainer();
            container.ComposeExportedValue(parameterName, parameterValue);
            value = container.GetExportedValue<T>(contractName);
        }

        public static void Load<T, U1, U2>(string contractName, string parameter1Name, U1 parameter1Value, string parameter2Name, U2 parameter2Value, out T value)
        {
            CompositionContainer container = GetCompositionContainer();
            container.ComposeExportedValue(parameter1Name, parameter1Value);
            container.ComposeExportedValue(parameter2Name, parameter2Value);
            value = container.GetExportedValue<T>(contractName);
        }
    }
}
