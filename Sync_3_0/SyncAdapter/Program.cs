namespace SyncAdapter
{
    using MappersExample;

    using MessageBuilding.MessageBuilders;

    internal class Program
    {
        /// <summary>
        /// Точка входа.
        /// Отсюда снициализируются все куски адаптера.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            // Сейчас все инициализируется с помощью new,
            // потом предметнонезависимые куски будут вынесены в nuget-пакеты, там можно через фабрику.
            // Инициализировать через UnityContainer, наверное, сейчас нет особой необходимости,
            // так как всех компонентов в одном адаптере может быть по нескольку штук, 
            // или какие-то компоненты могут быть получены в результате наследования базовых.

            var mappers = new[]
            {
                new AToBMapper()
            };

            var messageBuilder = new DefaultMessageBuilder();

            // Запускаем экземпляр сборщика. 
            // Сейчас через конструктор, но можно сделать отдельный метод.
            var packageCollector = new DefaultChangePackageCollector(mappers, messageBuilder);

            // Запускаем экземпляр применителя изменений.
            // По идее, он должен находиться в другом адаптере, но суть будет такая же.
            // Сейчас через конструктор, но можно сделать отдельный метод.
            var changesApplier = new DefaultChangesApplier(messageBuilder);

            // Готово. 
            // Компоненты начали следить за БД, этапы синхронизации начнутся, когда появятся нужные объекты в БД.
        }
    }
}