namespace Sync_3._0
{
    using System.ComponentModel.Design;

    internal class PackageHelper
    {

        /// <summary>
        /// Выполнить загрузку изменений объекта.
        /// </summary>
        /// <param name="se">Факт изменения объекта.</param>
        /// <param name="auditType">Тип аудита - старый или новый. Для демонстрации сделано так, потом можно делать через фабрику.</param>
        /// <param name="syncView">Представление объекта.</param>
        /// <returns></returns>
        public Change GetChangeFromAppDB(SyncEntity se, tAuditType auditType, View syncView)
        {
            // Возьмется логика из предыдущих синхронизаторов. 

        }
    }
}