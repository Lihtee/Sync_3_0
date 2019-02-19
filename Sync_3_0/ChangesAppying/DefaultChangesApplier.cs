using System;

namespace ChangesAppying
{
    /// <summary>
    /// Компонент для применения изменений к БД приложения.
    /// </summary>
    public class DefaultChangesApplier
    {
        /// <summary>
        /// ID, название или еще что-то, что позволяет опознать синхронизацию, к которой пицеплен данный сборщик изменений.
        /// </summary>
        private string SyncID;

        /// <summary>
        /// Компонент для сборки и разборки сообщений.
        /// Возможно, что тот же самый, что используется при сборке сообщений.
        /// </summary>
        private MessageBuilder messageBuilder;

        void Listen()
        {
            // Загрузить ChangesApplying, у которых synchronization = SyncID
            // и статус = "ожидает применения" (или еще какие-нибудь отличительные характеристики).
            // Здесь можно читать только PK объекта и PK связанной синхронизации.
            var ChangesApplyings = DataService.LoadObjects();

            // Если такие объекты есть, то для каждого запускаем ApplyChanges().
            if (ChangesApplyings.Any())
            {
                foreach (var ca in ChangesApplyings)
                {
                    ApplyChanges(ca);
                }
            }
        }

        public void ApplyChanges(ChangesApplying changesApplying)
        {
            // Догрузить changesApplying.
            DataService.LoadObject(changesApplying);

            // Прочитаем из бд входящие изменения.
            // Выберем только те, которые еще не применены
            Message[] changes = DataService.LoadObjects();

            foreach (var message in changes)
            {
                // Указать messageBuilder-у распаковать изменения и получить объекты.
                // На данном этапе MessageBuilder должен знать, в какие именно объекты он должен распаковывать изменения. 
                // Если он не ссылается на объекты целевого приложения, то какие-то "шаблоны" объектов может выдавать сам ChangesApplier.
                // Поэтому получаем объект package - пакет и набор ObjectChangePackage.
                var package = messageBuilder.GetChangesPackage(message);

                foreach (var objectChangePackage in package.ObjectChangePackages)
                {
                    // Создадим объект с типом, указанным в ObjectChangePackages.
                    var obj = Activator.CreateInstance(Type.GetType(objectChangePackage.type));
                    // Затем надо достать изменения и применить их к объекту. 
                    obj = messageBuilder.FillObject(obj, objectChangePackage);

                    // Выполнить обновление объекта в БД приложения.
                    AppDataService.UpdateObject(obj);

                    // Todo подумать над откатом изменений, транзакционностью.
                    // Первый вариант, если классы объекты не аудируются, перед обновлением объекта читать изменяемые атрибуты этого объекта из БД приложения (если он там есть),
                    // а при ошибке - откатывать все успшные изменения.
                    // Второй вариант, если целевые классы аудируются, то откатывать изменения на дату начала применения изменений с помощью аудита.
                }

            }
        }


    }
}
