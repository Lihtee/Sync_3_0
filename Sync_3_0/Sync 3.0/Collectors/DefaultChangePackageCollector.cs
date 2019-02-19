using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync_3._0.Collectors
{
    using System.Threading;

    using MessageBuilding.MessageBuilders;

    using Sync_3._0.Abstract;

    /// <summary>
    /// Базовый сборщик изменений.
    /// </summary>
    class DefaultChangePackageCollector: IChangePackageCollector
    {
        /// <summary>
        /// ID, название или еще что-то, что позволяет опознать синхронизацию, к которой пицеплен данный сборщик изменений.
        /// </summary>
        private string SyncID;

        /// <summary>
        /// Мапперы, используемые для упаковки.
        /// Упаковщик должен знать, что ему надо упаковывать.
        /// Текущий вариант - список мапперов.
        /// </summary>
        private List<IMapper> mappers;

        /// <summary>
        /// Закешированый список имен классов, которые нужно запаковать.
        /// </summary>
        private List<string> mappingClassNames;

        /// <summary>
        /// Компонент для сборки и разборки сообщений.
        /// </summary>
        private MessageBuilder messageBuilder;

        public DefaultChangePackageCollector(mappers)
        {
            mappers = mappers;
            messageBuilder = new DefaultMessageBuilder();

            // Получим имена классов.
            mappingClassNames = MappersHelper.GetMappingClassNames(mappers);
            // Начинаем следить за БД.
            var t = new Thread(Listen);
            t.Start();
        }

        /// <summary>
        /// Следить за БД и ждать, пока там появится объект Packaging.
        /// </summary>
        void Listen()
        {
            // Загрузить Packaging, у которых synchronization = SyncID
            // и статус = "ожидает упауовки" (или еще какие-нибудь отличительные характеристики).
            // Здесь можно читать только PK объекта и PK связанной синхронизации.
            var Packaging = DataService.LoadObjects();

            // Если такие объекты есть, то для каждого запускаем PackChanges().
            if (Packaging.Any())
            {
                foreach (var pc in Packaging)
                {
                    PackChanges(pc);
                }
            }
        }

        /// <summary>
        /// Запаковывает изменения.
        /// Метод можно разбить на несколько кусков, чтобы можно было модифицировать при прикладном использовании.
        /// </summary>
        /// <param name=""></param>
        public void PackChanges(Packaging)
        {
            // Догрузить Packaging.
            DataService.LoadObject(Packaging);

            // Период изменений. 
            // Если только dateFrom == null, то брать все изменения до dateTo.
            // Если только dateTo, то брать изменения начиная с dateFrom.
            // Если обе даты == null, то брать все изменения. 
            DateTime dateFrom = PackageCollecting.DateFrom;
            DateTime dateTo = PackageCollecting.DateTo;

            // Читаем факты изменения из БД.
            // Выбираем только факты нужных классов.
            // Сортируем по дате.
            // Порционная вычитка опущена для простоты.
            seClassLimit = In<SyncEntity>(se => se.ClassName, mappingClassNames);
            var syncEntities = DataService.LoadObjects(seClassLimit);

            foreach (var se in syncEntities)
            {
                // Проверим, было ли запаковано изменение в данной синхронизации. 
                // Текущая идея - проверять существование такого ObjectChangePackage:
                packagedLimit = And(
                    Equals<ObjectChangePackage>(ocp => ocp.Package.Packaging.Synchronization, SyncID),
                    Equals<ObjectChangePackage>(ocp => ocp.syncEntity, se)
                );
                var objectChangePackage = DataService.LoadObjects(packagedLimit).FirstOrDefault();

                // Если было, то попробовать взять готовый пакет.
                if (objectChangePackage != null)
                {
                    if (objectChangePackage.data != null)
                    {
                        MessageBuilder.AppendMessage(packaged);
                    }

                    // Если готового пакета нет, то запаковываем.
                }
                else
                {
                    // Выполняем чтение и маппинг.
                    // Если обозреваемый объект превращается в несколько целевых объектов, то маппетов будет несколько.
                    // Базовый механизм маппинга будет таким же, как в прошлом синхронизаторе.
                    var currentMappers = mappers.Where(map => map.GetObservingType() == se.type);
                    foreach (var currentMapper in currentMappers)
                    {
                        // Если еще не запаковывали, то читаем сами изменения из БД приложения.
                        // Тут должно быть по-разному: вариант для старого аудита, и вариант для нового аудита.
                        // Чтение уже реализовано в предыдущих вариантах синхронизаторов.
                        // Например, прочитаем со старым аудитом.
                        // Должны получить сам объект, тип объекта и список измененных атрибутов.
                        var change = PackageHelper.GetChangeFromAppDB(se, tAuditType.Old, map.GetObservingView());

                        // Получаем целевые объекты.
                        var destinationObject = currentMapper.Map(change);

                        // Собираем сообщение.
                        // Скорее всего, messageBuilder будет сам решать, как собирать пакеты, поэтому просто отправлем ему всю информацию об изменениях.
                        messageBuilder.CreateMessage(destinationObject, change, objectChangePackage);
                    }
                }

                // Как только пакет в messageBuilder готов, достаем его и все его сообщения.
                var package = messageBuilder.PopPackage();
                if (package != null)
                {
                    // Пакет сообщения складываем в бд.
                    DataService.UpdateObject(package);
                    package = null;
                }
            }

            // Создать пакет из оставшихся сообщений.
            var forcedPackage = messageBuilder.PopPackageForced();
            DataService.UpdateObject(forcedPackage);

            // Теперь в бд лежат пакеты и составляющие сообщения. 
            // Если это будет занимать слишком много памяти,
            // то можно очищать ObjectChangePackage.data (можно и не хранить, но тогда иногда придется упаковывать по нескольку раз)
            // и Package.data (после успешной синхронизации).

            // Как часто поток будет искать в БД объект Package.
            Thread.Sleep(10000);
        }
    }
}
